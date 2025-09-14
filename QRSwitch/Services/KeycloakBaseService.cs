using Microsoft.Extensions.Configuration;
using QRSwitch.Models;
using QRSwitch.Models.Auth;
using System.Text.Json;

namespace QRSwitch.Services
{
 
    public class KeycloakBaseService
    {
        protected readonly HttpClient _httpClient;
        protected readonly IConfiguration _config;
        private string _adminToken = null!;
        private DateTime _tokenExpiry;
        public KeycloakBaseService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }
        public async Task<string?> GetAdminTokenAsync()
        {
            if (!string.IsNullOrEmpty(_adminToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _adminToken;
            }

            try
            {
                var keycloakConfig = _config.GetSection("Keycloak");
                var adminBaseUrl = keycloakConfig["AdminBaseUrl"] ?? "http://localhost:8080";
                var tokenEndpoint = $"{adminBaseUrl}/realms/master/protocol/openid-connect/token";

                Console.WriteLine($"Getting admin token from: {tokenEndpoint}");

                var formData = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "password"),
                    new("client_id", "admin-cli"),
                    new("username", keycloakConfig["AdminUsername"] ?? "admin"),
                    new("password", keycloakConfig["AdminPassword"] ?? "admin")
                };

                var formContent = new FormUrlEncodedContent(formData);
                formContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                Console.WriteLine($"Request data: grant_type=password&client_id=admin-cli&username={keycloakConfig["AdminUsername"]}&password=***");

                var response = await _httpClient.PostAsync(tokenEndpoint, formContent);

                Console.WriteLine($"Token response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Token response: {jsonResponse}");

                    var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(jsonResponse);

                    if (tokenResponse != null)
                    {
                        _adminToken = tokenResponse.AccessToken;
                        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Refresh 1 minute before expiry
                        Console.WriteLine("Admin token obtained successfully");
                        return _adminToken;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Keycloak Admin Token Error: {response.StatusCode} - {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Keycloak Admin Token Exception: {ex.Message}");
                return null;
            }
        }
        protected string GetKeycloakBaseUrl()
        {
            return $"http://{_config["Keycloak:Url"]}";
        }

        public async Task<string?> ExchangeForRptTokenAsync(string accessToken)
        {
            try
            {
                var keycloakConfig = _config.GetSection("Keycloak");
                var tokenEndpoint = $"http://{keycloakConfig["Url"]}/realms/{keycloakConfig["Realm"]}/protocol/openid-connect/token";

                var formData = new List<KeyValuePair<string, string>>
                 {
                     new("grant_type", "urn:ietf:params:oauth:grant-type:uma-ticket"),
                     new("client_id", keycloakConfig["ClientId"]),
                     new("client_secret", keycloakConfig["ClientSecret"]),
                     new("audience", keycloakConfig["ClientId"]),
                     new("subject_token", accessToken)
                 };
                
                var formContent = new FormUrlEncodedContent(formData);
                formContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var response = await _httpClient.PostAsync(tokenEndpoint, formContent);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"RPT Token Error: {response.StatusCode} - {jsonResponse}");
                    return null;
                }

                var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(jsonResponse);
                return tokenResponse?.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExchangeForRptTokenAsync Exception: {ex.Message}");
                return null;
            }
        }

        protected readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

    }
}
