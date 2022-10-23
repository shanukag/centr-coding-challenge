using System.Net;
using Newtonsoft.Json;

namespace Loup.DotNet.Challenge.FunctionApp
{
    public class ErrorResult : HttpResult
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        public ErrorResult(HttpStatusCode httpStatusCode, string message) : base(httpStatusCode)
        {
            Message = message;
        }
    }
}
