using Microsoft.Extensions.Configuration;
using QRSwitch.Models;
using QRSwitch.Models.Shared;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QRSwitch.Services
{
 
    public class KeycloakRoleService : KeycloakBaseService
    {
        public KeycloakRoleService(HttpClient httpClient, IConfiguration config) 
            : base(httpClient, config)
        {
        }

        public async Task<KeycloakResult> CreateRoleAsync(string realm, string roleName, string? description = null)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/roles";
            var payload = new
            {
                name = roleName,
                description = description ?? $"Role {roleName}",
                composite = false,
                clientRole = false
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                return new KeycloakResult
                {
                    Success = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? null : body,
                    RawResponse = body,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new KeycloakResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        public async Task<KeycloakResult> UpdateRoleAsync(string realm, string roleName, string? newDescription = null)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/roles/{roleName}";
            var payload = new
            {
                name = roleName,
                description = newDescription ?? $"Updated role {roleName}"
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                return new KeycloakResult
                {
                    Success = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? null : body,
                    RawResponse = body,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new KeycloakResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        public async Task<KeycloakResult> DeleteRoleAsync(string realm, string roleName)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/roles/{roleName}";
            
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                return new KeycloakResult
                {
                    Success = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? null : body,
                    RawResponse = body,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new KeycloakResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        public async Task<KeycloakResult> GetAllRolesAsync(string realm)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/roles";
            
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                };

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                return new KeycloakResult
                {
                    Success = response.IsSuccessStatusCode,
                    ErrorMessage = response.IsSuccessStatusCode ? null : body,
                    RawResponse = body,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new KeycloakResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        public async Task<KeycloakResult> GetRolesForUserByUsernameAsync(string realm, string username)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            // First get user by username
            var userUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users?username={username}&exact=true";
            
            try
            {
                var userRequest = new HttpRequestMessage(HttpMethod.Get, userUrl)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                };

                var userResponse = await _httpClient.SendAsync(userRequest);
                if (!userResponse.IsSuccessStatusCode)
                {
                    return new KeycloakResult { Success = false, ErrorMessage = "User not found" };
                }

                var userBody = await userResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(userBody);
                if (!doc.RootElement.EnumerateArray().Any())
                {
                    return new KeycloakResult { Success = false, ErrorMessage = "User not found" };
                }

                var userId = doc.RootElement.EnumerateArray().First().GetProperty("id").GetString();

                // Get roles for this user
                var rolesUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users/{userId}/role-mappings/realm";
                var rolesRequest = new HttpRequestMessage(HttpMethod.Get, rolesUrl)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                };

                var rolesResponse = await _httpClient.SendAsync(rolesRequest);
                var rolesBody = await rolesResponse.Content.ReadAsStringAsync();

                return new KeycloakResult
                {
                    Success = rolesResponse.IsSuccessStatusCode,
                    ErrorMessage = rolesResponse.IsSuccessStatusCode ? null : rolesBody,
                    RawResponse = rolesBody,
                    StatusCode = rolesResponse.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new KeycloakResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        public async Task<KeycloakResult> AssignRoleToUserAsync(string realm, string userId, string roleName)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            try
            {
                // First get role details
                var roleUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/roles/{roleName}";
                var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleUrl)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
                };

                var roleResponse = await _httpClient.SendAsync(roleRequest);
                if (!roleResponse.IsSuccessStatusCode)
                {
                    return new KeycloakResult { Success = false, ErrorMessage = "Role not found" };
                }

                var roleBody = await roleResponse.Content.ReadAsStringAsync();
                using var roleDoc = JsonDocument.Parse(roleBody);
                var roleId = roleDoc.RootElement.GetProperty("id").GetString();

                // Assign role to user
                var assignUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users/{userId}/role-mappings/realm";
                var payload = new[]
                {
                    new { id = roleId, name = roleName }
                };

                var assignRequest = new HttpRequestMessage(HttpMethod.Post, assignUrl)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };

                var assignResponse = await _httpClient.SendAsync(assignRequest);
                var assignBody = await assignResponse.Content.ReadAsStringAsync();

                return new KeycloakResult
                {
                    Success = assignResponse.IsSuccessStatusCode,
                    ErrorMessage = assignResponse.IsSuccessStatusCode ? null : assignBody,
                    RawResponse = assignBody,
                    StatusCode = assignResponse.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new KeycloakResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        //custom endpoint to create multibale role in one time
        public async Task<Dictionary<string, string>> CreateRolesAsync(List<string> roleNames,string realm)
        {
            var accessToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var results = new Dictionary<string, string>();

            foreach (var roleName in roleNames)
            {
                var role = new
                {
                    name = roleName,
                    description = $"Role {roleName}"
                };
                var  roleUrl = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/roles";
                var response = await _httpClient.PostAsJsonAsync(
                    roleUrl,role);

                if (response.IsSuccessStatusCode)
                {
                    results[roleName] = "Created ";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    results[roleName] = $"Failed : {error}";
                }
            }

            return results;
        }

    }
}
