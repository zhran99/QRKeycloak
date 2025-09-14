using System.Text.Json.Serialization;

namespace QRSwitch.Models.Responses
{
    public class GetAllUsersResponse : BaseResponse
    {
        public List<UserDto> Data { get; set; } = new List<UserDto>();
        public PaginationDto Pagination { get; set; } = new PaginationDto();
    }

    public class UserDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        
        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;
        
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = null!;
        
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = null!;
        
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
        
        [JsonPropertyName("createdTimestamp")]
        public long CreatedTimestamp { get; set; }
        
        [JsonPropertyName("requiredActions")]
        public string[]? RequiredActions { get; set; }
    }

    public class PaginationDto
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public int First { get; set; }
        public int Total { get; set; }
    }
}
