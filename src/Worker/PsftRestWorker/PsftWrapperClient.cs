using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using static System.Net.WebRequestMethods;
using System.Net;

public sealed class PsftWrapperClient
{
    private readonly HttpClient _http;
    public PsftWrapperClient(HttpClient http) => _http = http;

   /* public async Task<bool> RunTicketAsync(string sysId, CancellationToken ct)
    {
        using var resp = await _http.PostAsync($"/api/migrate/run-ticket/{sysId}", content: null, ct);
        return resp.IsSuccessStatusCode;
    }
   */
public async Task<bool> RunTicketAsync(string sysId, CancellationToken ct)
{
    using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/migrate/run-ticket/{sysId}");

    // Only wait for headers, not the full response body
    var rsp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

    return rsp.StatusCode == HttpStatusCode.Accepted || rsp.IsSuccessStatusCode;
}
}