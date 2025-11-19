// QueueWorker.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


    public sealed class QueueWorker : BackgroundService
    {
        private readonly QueueClient _queue;
        private readonly PsftWrapperClient _wrapper;
        private readonly ILogger<QueueWorker> _log;

        private readonly int _concurrency;
        private readonly int _batchSize;
        private readonly TimeSpan _visibility;

        public QueueWorker(
            QueueClient queue,
            PsftWrapperClient wrapper,
            ILogger<QueueWorker> log,
            IConfiguration cfg)
        {
            _queue = queue;
            _wrapper = wrapper;
            _log = log;

            _concurrency = int.TryParse(cfg["WORKER_CONCURRENCY"], out var c) ? Math.Max(1, c) : 4;
            var configuredBatch = int.TryParse(cfg["WORKER_BATCH_SIZE"], out var b) ? b : _concurrency * 2;
            _batchSize = Math.Min(Math.Max(1, configuredBatch), 32); // Azure limit
            var visMin = int.TryParse(cfg["WORKER_VISIBILITY_MINUTES"], out var m) ? m : 30;
            _visibility = TimeSpan.FromMinutes(visMin);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _queue.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
            _log.LogInformation("Worker listening on {QueueName} with concurrency={Concurrency}, batchSize={Batch}",
                _queue.Name, _concurrency, _batchSize);

            var running = new HashSet<Task>();

            while (!stoppingToken.IsCancellationRequested)
            {
                // back-pressure
                if (running.Count >= _concurrency)
                {
                    var finished = await Task.WhenAny(running);
                    running.Remove(finished);
                    continue;
                }

                // pull a batch
                var batch = await _queue.ReceiveMessagesAsync(
                    maxMessages: _batchSize,
                    visibilityTimeout: TimeSpan.FromMinutes(10),      // enough for a migration step
                    cancellationToken: stoppingToken);

                // nothing to do?
                if (batch.Value.Length == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                foreach (var m in batch.Value)
                {
                    running.Add(Task.Run(async () =>
                    {
                        var ct = stoppingToken; // pass through

                        try
                        {
                            // Your queue now carries a plain sysId (text) — keep it simple
                            var sysId = m.Body.ToString();

                            // (optional) write “In Progress” in SN from the worker
                            // await _sn.UpdateStatusAsync(sysId, "In Progress", ct);

                            // call wrapper
                            var ok = await _wrapper.RunTicketAsync(sysId, ct);

                            if (ok)
                            {
                                await _queue.DeleteMessageAsync(m.MessageId, m.PopReceipt, ct);
                                // (optional) await _sn.UpdateStatusAsync(sysId, "Completed", ct);
                            }
                            else
                            {
                                // re-queue immediately (or choose a delay)
                                await _queue.UpdateMessageAsync(
                                    m.MessageId, m.PopReceipt, m.Body, TimeSpan.Zero, ct);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Error processing message {MessageId}", m.MessageId);

                            // abandon so it can be retried
                            try
                            {
                                await _queue.UpdateMessageAsync(
                                    m.MessageId, m.PopReceipt, m.Body, TimeSpan.Zero, ct);
                            }
                            catch { /* swallow secondary errors */ }
                        }
                    }, stoppingToken));
                }
            }

            // drain before exit
            await Task.WhenAll(running);
        }


        private async Task ProcessOneAsync(QueueMessage m, CancellationToken ct)
        {
            // Treat message as plain text sysId
            var raw = m.Body.ToString() ?? string.Empty;
            var sysId = raw.Trim('"', ' ', '\r', '\n', '\t');

            if (string.IsNullOrWhiteSpace(sysId))
            {
                _log.LogWarning("Empty sysId; deleting message. MessageId={MessageId}", m.MessageId);
                await _queue.DeleteMessageAsync(m.MessageId, m.PopReceipt, ct);
                return;
            }

            try
            {
                _log.LogInformation("Start {SysId} (DequeueCount={Count})", sysId, m.DequeueCount);

                await _wrapper.RunTicketAsync(sysId, ct);

                await _queue.DeleteMessageAsync(m.MessageId, m.PopReceipt, ct);
                _log.LogInformation("Done  {SysId}", sysId);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _log.LogWarning("Canceled while processing {SysId}; message will reappear.", sysId);
            }
            catch (Exception ex)
            {
                var tooMany = m.DequeueCount >= 5;
                _log.LogError(ex, "Failed {SysId} (DequeueCount={Count}) {Hint}",
                    sysId, m.DequeueCount, tooMany ? "→ deleting (poison)" : "→ releasing");

                if (tooMany)
                {
                    await _queue.DeleteMessageAsync(m.MessageId, m.PopReceipt, ct);
                    // TODO: optionally enqueue to a poison queue
                }
                else
                {
                    // make visible immediately for retry
                    await _queue.UpdateMessageAsync(
         m.MessageId,
         m.PopReceipt,
         m.Body.ToString(),        // keep the same payload
         TimeSpan.Zero,            // make visible immediately
         ct);
                }
            }
        }
    }
