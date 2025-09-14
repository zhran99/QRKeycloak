using Microsoft.AspNetCore.Mvc;
using QRSwitch.Models;
using QRSwitch.Models.Auth;
using QRSwitch.Services;
using System.Text.Json;

namespace QRSwitch.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var tokenUrl = $"http://{_config["Keycloak:Url"]}/realms/{_config["Keycloak:Realm"]}/protocol/openid-connect/token";

            var parameters = new Dictionary<string, string>
              {
                  {"grant_type", "password"},
                  {"client_id", _config["Keycloak:ClientId"]},
                  {"client_secret", _config["Keycloak:ClientSecret"]},
                  {"username", request.Username},
                  {"password", request.Password}
              };
              
            var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(parameters));
            if (!response.IsSuccessStatusCode)
                return Unauthorized();

            var content = await response.Content.ReadAsStringAsync();
            var accessTokenObj = JsonDocument.Parse(content);
            var accessToken = accessTokenObj.RootElement.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Failed to get access token");

            var keycloakService = new KeycloakRoleService(client, _config);
            var rptToken = await keycloakService.ExchangeForRptTokenAsync(accessToken);

            if (string.IsNullOrEmpty(rptToken))
                return Unauthorized("Failed to get RPT token");

            return Ok(new
            {
                access_token = accessToken,
                rpt_token = rptToken
            });
        }


    }
}
