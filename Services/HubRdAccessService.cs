using System.Net.Http.Json;
using CoreWhatIf.Models;
using Microsoft.Extensions.Options;

namespace CoreWhatIf.Services;

public class HubRdAccessService
{
    private readonly HttpClient _httpClient;
    private readonly HubRdAccessSettings _settings;
    private readonly ILogger<HubRdAccessService> _logger;

    public HubRdAccessService(
        HttpClient httpClient,
        IOptions<HubRdAccessSettings> settings,
        ILogger<HubRdAccessService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<HubRdAccessResponse?> CheckAccessAsync(string email)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/api/access?app={Uri.EscapeDataString(_settings.AppName)}&email={Uri.EscapeDataString(email)}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Api-Key", _settings.ApiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HubRD returned {Status} for {Email}",
                    (int)response.StatusCode, email);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<HubRdAccessResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling HubRD for {Email}", email);
            return null;
        }
    }
}
