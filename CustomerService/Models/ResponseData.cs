using System.Net;

namespace CustomerService.Models
{
    public class ResponseData
    {
        public HttpStatusCode? Status { get; set; }
        public string? ResponseMessage { get; set; }
        public object? data { get; set; }
        public string? Token {  get; set; }
    }
}
