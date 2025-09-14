using Microsoft.Extensions.Configuration;
using QRSwitch.Models;
using QRSwitch.Models.Shared;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QRSwitch.Services
{
   
    public class KeycloakUserService : KeycloakBaseService
    {
        public KeycloakUserService(HttpClient httpClient, IConfiguration config) 
            : base(httpClient, config)
        {
        }
        public async Task<KeycloakResult> CreateUserAsync(string realm, string username, string email, string password, string firstName, string lastName)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users";
            var payload = new
            {
                username,
                email,
                firstName,
                lastName,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new { type = "password", value = password, temporary = false }
                }
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

                if (response.IsSuccessStatusCode)
                {
                    var location = response.Headers.Location?.ToString();
                    var userId = location?.Split('/').Last();

                    return new KeycloakResult
                    {
                        Success = true,
                        UserId = userId,
                        RawResponse = body,
                        StatusCode = response.StatusCode,
                    };
                }

                return new KeycloakResult
                {
                    Success = false,
                    ErrorMessage = body,
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
        public async Task<KeycloakResult> UpdateUserAsync(string realm, string userId, string? email = null, string? firstName = null, string? lastName = null, bool? enabled = null)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users/{userId}";
            
            var payload = new Dictionary<string, object>();
            if (email != null) payload["email"] = email;
            if (firstName != null) payload["firstName"] = firstName;
            if (lastName != null) payload["lastName"] = lastName;
            if (enabled.HasValue) payload["enabled"] = enabled.Value;

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
        public async Task<KeycloakResult> DeleteUserAsync(string realm, string userId)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users/{userId}";
            
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
        public async Task<KeycloakResult> GetAllUsersAsync(string realm, int first = 0, int max = 100)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users?first={first}&max={max}";
            
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
        public async Task<KeycloakResult> GetUserByUsernameAsync(string realm, string username)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users?username={username}&exact=true";
            
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
        public async Task<KeycloakResult> ResetUserPasswordAsync(string realm, string userId, string newPassword)
        {
            var token = await GetAdminTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new KeycloakResult { Success = false, ErrorMessage = "Failed to get admin token" };
            }

            var url = $"{GetKeycloakBaseUrl()}/admin/realms/{realm}/users/{userId}/reset-password";
            var payload = new
            {
                type = "password",
                value = newPassword,
                temporary = false
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
    }
}
