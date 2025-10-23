using System.Net.Http.Json;
using System.Text.Json;

namespace Infra.ExternalServices.Client;

public class ExternalApiWebClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    
    private static async Task<T?> Deserialize<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<T>(content);
    }
    
    public async Task<T?> Get<T>(string uri, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        return await Deserialize<T>(response, cancellationToken);
    }

    public async Task<T?> Post<T, A>(string uri, A data, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(uri, data, cancellationToken);
        return await Deserialize<T>(response, cancellationToken);
    }

    public async Task<T?> Put<T, A>(string uri, A data, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsJsonAsync(uri, data, cancellationToken);
        return await Deserialize<T>(response, cancellationToken);
    }

    public async Task<T?> Patch<T, A>(string uri, A data, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PatchAsJsonAsync(uri, data, cancellationToken);
        return await Deserialize<T>(response, cancellationToken);
    }

    public async Task Delete(string uri, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}