using System.Text.Json.Serialization;

namespace QRSwitch.Models.Responses
{
    public class GetAllRolesResponse : BaseResponse
    {
        public List<RoleDto> Data { get; set; } = new List<RoleDto>();
    }

    public class RoleDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("composite")]
        public bool Composite { get; set; }
        
        [JsonPropertyName("clientRole")]
        public bool ClientRole { get; set; }
        
        [JsonPropertyName("containerId")]
        public string ContainerId { get; set; } = null!;
    }
}
