using QRSwitch.Models.Authorization;
using QRSwitch.Models.Shared;
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
                request.Content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");

            }

            return await _httpClient.SendAsync(request);
        }
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
        public async Task<KeycloakResult> CreateResourceAsync(string realm, string clientIdFromUi, CreateResourceRequest req)
        {
            var clientUuid = await GetClientUuidAsync(realm, clientIdFromUi);

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients/{clientUuid}/authz/resource-server/resource";
            var payload = new
            {
                name = req.Name,
                displayName = req.DisplayName,
                type = req.Type,
                scopes = req.Scopes?.Select(s => new { name = s }).ToList()
            };
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
        public async Task<KeycloakResult> CreatePolicyAsync(string realm, string clientIdFromUi, CreatePolicyRequest req)
        {
            var clientUuid = await GetClientUuidAsync(realm, clientIdFromUi);

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients/{clientUuid}/authz/resource-server/policy/role";

            var roleObjects = new List<object>();
            foreach (var roleName in req.Roles)
            {
                var roleId = await GetRoleIdAsync(realm, roleName); 
                if (roleId != null)
                {
                    roleObjects.Add(new { id = roleId, required = false });
                }
            }

            var payload = new
            {
                name = req.Name,
                type = req.Type.ToLowerInvariant(),
                logic = "POSITIVE",
                decisionStrategy = "UNANIMOUS",
                roles = roleObjects
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
        public async Task<KeycloakResult> CreatePermissionAsync(string realm, string clientIdFromUi, CreatePermissionRequest req)
        {
            var clientUuid = await GetClientUuidAsync(realm, clientIdFromUi);

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/clients/{clientUuid}/authz/resource-server/permission/scope";
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
        private async Task<string?> GetRoleIdAsync(string realm, string roleName)
        {
            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/roles/{roleName}";
            var response = await SendAuthorizedRequest(HttpMethod.Get, url);

            if (!response.IsSuccessStatusCode)
                return null;

            var body = await response.Content.ReadAsStringAsync();
            var roleObj = JsonSerializer.Deserialize<JsonElement>(body);
            return roleObj.GetProperty("id").GetString();
        }
    }


}
