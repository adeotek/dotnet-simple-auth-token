using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using SimpleAuthToken;

namespace SimpleAuthToken.Example.Client;

public class ApiClient
{
    private const string ApiRoute = "SecureData";

    private readonly TokenConfig _tokenConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(TokenConfig tokenConfig, IHttpClientFactory httpClientFactory, ILogger<ApiClient> logger)
    {
        _tokenConfig = tokenConfig;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task Run()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("DefaultHttpClient");
            // Get custom generated Auth token
            var (token, _) = TokenProvider.GenerateToken(_tokenConfig.PublicKey, _tokenConfig.SecretKey, _tokenConfig.Issuer, 60);
            _logger.LogDebug("Simple Auth Token: [{}]", token);
            // Set Header Bearer
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            _logger.LogInformation("Sending HTTP request GET {}/{} ...", httpClient.BaseAddress!.AbsoluteUri, ApiRoute);
            var response = await httpClient.GetAsync(ApiRoute);
            if (response == null)
            {
                throw new Exception("HTTP response is null!");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Invalid response StatusCode: [{(int)response.StatusCode}] {response.StatusCode} >>\n{content}");
            }
            
            _logger.LogInformation("Response:\n{Content}", content);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "API call error:");
        }
    }
}