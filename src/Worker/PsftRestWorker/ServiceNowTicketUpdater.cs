using Microsoft.Extensions.Configuration;
using Psft.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;

public sealed class ServiceNowTicketUpdater : ITicketSink
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _table;

    public ServiceNowTicketUpdater(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _baseUrl = cfg["SN:Instance"]!.TrimEnd('/');
        _table = cfg["SN:TableName"]!;
        var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{cfg["SN:Username"]}:{cfg["SN:Password"]}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
    }

    public async Task UpdateStatusAsync(
        string id,
        string status,
        string? reason,
        DateTimeOffset? queuedOnUtc,
        DateTimeOffset? migratedOnUtc,
        CancellationToken ct)
    {
        var url = $"{_baseUrl}/api/now/table/{_table}/{id}";
        var body = new Dictionary<string, object?>
        {
            ["u_status"] = status,
            ["u_reason"] = string.IsNullOrWhiteSpace(reason) ? null : reason,
            ["u_queued_on"] = queuedOnUtc,
            ["u_migrated_on"] = migratedOnUtc
        };

        using var req = new HttpRequestMessage(HttpMethod.Patch, url)
        { Content = JsonContent.Create(body) };

        var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }
}
