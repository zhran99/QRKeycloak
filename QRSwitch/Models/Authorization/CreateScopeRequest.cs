using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace QRSwitch.Models.Authorization
{
    public class CreateScopeRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("iconUri")]
        public string? IconUri { get; set; }
    }
}
