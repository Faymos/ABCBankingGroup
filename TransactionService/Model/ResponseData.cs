using System.Net;

namespace TransactionService.Model
{
    public class ResponseData
    {
        public HttpStatusCode? Status { get; set; }
        public string? ResponseMessage { get; set; }
        public object? data { get; set; }
    }
}
