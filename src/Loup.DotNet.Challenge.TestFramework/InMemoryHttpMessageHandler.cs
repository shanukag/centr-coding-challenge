﻿using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Loup.DotNet.Challenge.TestFramework
{
    public class InMemoryHttpMessageHandler : HttpMessageHandler
    {
        private readonly IApiTestHost _testHost;
        private readonly IServiceProvider _serviceProvider;

        public InMemoryHttpMessageHandler(IApiTestHost testHost, IServiceProvider serviceProvider)
        {
            _testHost = testHost;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri.LocalPath;
            var function = _testHost.ResolveEndpoint(request.Method, path);

            if (function.Endpoint != null)
            {
                var scopeFatory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
                using (var serviceScope = scopeFatory.CreateScope())
                {
                    var userClaimsContext = serviceScope.ServiceProvider.GetRequiredService<UserContext>();
                    if (request.Headers.TryGetValues("X-ClaimsContext", out IEnumerable<string> headerValues))
                    {
                        var encodedHeader = headerValues.First();
                        var base64HeaderBytes = Convert.FromBase64String(encodedHeader);
                        var userContextJson = Encoding.UTF8.GetString(base64HeaderBytes);
                        userClaimsContext = JsonConvert.DeserializeObject<UserContext>(userContextJson);

                        // Strip header - no longer needed past this point.

                        request.Headers.Remove("X-ClaimsContext");
                    }
                    var httpContext = new DefaultHttpContext();
                    if (request.Headers.TryGetValues("Forwarded-Host", out IEnumerable<string> hostHeaders))
                        httpContext.Request.Headers.Add("Forwarded-Host", hostHeaders.First());

                    httpContext.Request.Method = request.Method.Method;
                    if (request.Method == HttpMethod.Post)
                    {
                        string postBodyStr = "{}";
                        if (request.Content != null)
                        {
                            //    postBodyStr = JsonConvert.SerializeObject(postBody);
                        }

                        request.Content = new StringContent(postBodyStr);

                        httpContext.Request.Body = new MemoryStream();
                        var uniEncoding = new UnicodeEncoding();
                        httpContext.Request.Body.Write(uniEncoding.GetBytes(postBodyStr));
                    }

                    httpContext.Request.Path = new PathString(request.RequestUri.LocalPath);
                    httpContext.Request.QueryString = new QueryString(request.RequestUri.Query);
                    var httpContextAccessor = serviceScope.ServiceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
                    Mock.Get<IHttpContextAccessor>(httpContextAccessor).Setup(r => r.HttpContext).Returns(httpContext);
                    httpContext.RequestServices = serviceScope.ServiceProvider;
                    var parameters = new List<object>();
                    // parameters.Add(httpContext.Request); //SG - Encountered exception as httpContext.Request is of type DefaultHttpRequest and the GetRecipe function expects the type HttpRequestMessage
                    parameters.Add(request);  
                    parameters.AddRange(function.Parameters.Values);
                    parameters.Add(userClaimsContext);

                    var repository = serviceScope.ServiceProvider.GetService(typeof(IRepository)) as IRepository;
                    var actionResult = (ObjectResult)await function.Endpoint.InvokeAsync(repository, parameters.ToArray());
                    var statusResult = (IStatusCodeActionResult)actionResult;
                    
                    Enum.TryParse<HttpStatusCode>(statusResult.StatusCode.GetValueOrDefault().ToString(), out HttpStatusCode statusCode);
                    return new HttpResponseMessage(statusCode)
                    {
                        Content = new StringContent(actionResult.Value.ToString())
                    };
                }
            }
            return null;
        }
    }
}
