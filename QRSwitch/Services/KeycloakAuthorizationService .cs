using QRSwitch.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QRSwitch.Services
{
    public class KeycloakAuthorizationService : KeycloakBaseService
    {
        public KeycloakAuthorizationService(HttpClient httpClient, IConfiguration config)
            : base(httpClient, config) { }

        private async Task<HttpResponseMessage> SendAuthorizedRequest(HttpMethod method, string url, object? payload = null)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
                throw new Exception("Failed to get admin token");

            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (payload != null)
            {
                //request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                request.Content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");

            }

            return await _httpClient.SendAsync(request);
        }

        // Create Scope
        public async Task<KeycloakResult> CreateScopeAsync(string realm, string clientIdFromUi, CreateScopeRequest req)
        {
            var clientUuid = await GetClientUuidAsync(realm, clientIdFromUi);

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients/{clientUuid}/authz/resource-server/scope";
            var response = await SendAuthorizedRequest(HttpMethod.Post, url, req);
            var body = await response.Content.ReadAsStringAsync();

            return new KeycloakResult
            {
                Success = response.IsSuccessStatusCode,
                RawResponse = body,
                StatusCode = response.StatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? null : body
            };
        }


        // Create Resource
        public async Task<KeycloakResult> CreateResourceAsync(string realm, string clientId, CreateResourceRequest req)
        {
            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients/{clientId}/authz/resource-server/resource";
            var response = await SendAuthorizedRequest(HttpMethod.Post, url, req);
            var body = await response.Content.ReadAsStringAsync();

            return new KeycloakResult
            {
                Success = response.IsSuccessStatusCode,
                RawResponse = body,
                StatusCode = response.StatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? null : body
            };
        }

        // Create Policy
        public async Task<KeycloakResult> CreatePolicyAsync(string realm, string clientId, CreatePolicyRequest req)
        {
            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients/{clientId}/authz/resource-server/policy/role";
            var payload = new
            {
                name = req.Name,
                type = "role",
                roles = req.Roles.Select(r => new { id = (string?)null, role = r })
            };

            var response = await SendAuthorizedRequest(HttpMethod.Post, url, payload);
            var body = await response.Content.ReadAsStringAsync();

            return new KeycloakResult
            {
                Success = response.IsSuccessStatusCode,
                RawResponse = body,
                StatusCode = response.StatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? null : body
            };
        }

        // Create Permission
        public async Task<KeycloakResult> CreatePermissionAsync(string realm, string clientId, CreatePermissionRequest req)
        {
            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients/{clientId}/authz/resource-server/permission/scope";
            var payload = new
            {
                name = req.Name,
                type = "scope",
                scopes = req.Scopes,
                policies = req.Policies
            };

            var response = await SendAuthorizedRequest(HttpMethod.Post, url, payload);
            var body = await response.Content.ReadAsStringAsync();

            return new KeycloakResult
            {
                Success = response.IsSuccessStatusCode,
                RawResponse = body,
                StatusCode = response.StatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? null : body
            };
        }

        private async Task<string> GetClientUuidAsync(string realm, string clientIdFromUi)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
                throw new Exception("Failed to get admin token");

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients?clientId={clientIdFromUi}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var clients = JsonSerializer.Deserialize<List<JsonElement>>(json);

            if (clients == null || clients.Count == 0)
                throw new Exception($"Client with clientId '{clientIdFromUi}' not found in realm '{realm}'");

            return clients[0].GetProperty("id").GetString()!;
        }


    }


}
