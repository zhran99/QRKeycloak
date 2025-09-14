namespace QRSwitch.Models.Responses
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }
    }
}
