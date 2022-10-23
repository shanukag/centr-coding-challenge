using System.Net;
using Loup.DotNet.Challenge.FunctionApp.Models;
using Newtonsoft.Json;

namespace Loup.DotNet.Challenge.FunctionApp
{
    public interface IHttpResult
    {
        [JsonProperty("code")]
        HttpStatusCode HttpStatusCode { get; set; }
    }

    public class HttpResult : IHttpResult
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public HttpResult(HttpStatusCode statusCode)
        {
            HttpStatusCode = statusCode;
        }
    }
    
    public class SuccessfulHttpResult : HttpResult
    {
        public Recipe Recipe { get; set; }

        public SuccessfulHttpResult(Recipe recipe, HttpStatusCode httpStatusCode) : base(httpStatusCode)
        {
            Recipe = recipe;
        }
    } 

}