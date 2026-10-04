using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using bossfight_client.Responses;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client.Requests;

public abstract class Request
{
    protected readonly StoreModel Store;
    private static readonly HttpClient HttpClient = new();

    protected Request(StoreModel store)
    {
        Store = store;
    }

    protected abstract string Endpoint { get; }
    protected virtual HttpMethod Method => HttpMethod.Get;
    protected virtual object? Body => null;

    public virtual Response Send()
    {
        return SendAsync().GetAwaiter().GetResult();
    }

    public virtual async Task<Response> SendAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri(new Uri(Store.ApiUrl), Endpoint);
        var request = new HttpRequestMessage(Method, uri);

        if (!string.IsNullOrWhiteSpace(Store.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Store.ApiKey);
        }

        if (Body != null && Method != HttpMethod.Get && Method != HttpMethod.Head)
        {
            var json = JsonSerializer.Serialize(Body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await HttpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return CreateResponse(response.IsSuccessStatusCode, content, response.StatusCode.ToString());
    }

    protected abstract Response CreateResponse(bool success, string content, string status);
}