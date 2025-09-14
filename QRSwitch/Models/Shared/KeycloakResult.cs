using System.Net;

namespace QRSwitch.Models.Shared
{
    public class KeycloakResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
        public string? RawResponse { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
