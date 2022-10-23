using System;
using System.Net;
using System.Net.Http;
using Loup.DotNet.Challenge.FunctionApp.Models;
using Loup.DotNet.Challenge.TestFramework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Loup.DotNet.Challenge.FunctionApp
{
    public class GetRecipeFunctionHandler 
    {
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly int _recipeId;
        private readonly UserContext _userContext;
        private readonly IRepository _repository;

        public GetRecipeFunctionHandler(HttpRequestMessage httpRequestMessage, int recipeId, UserContext userContext, IRepository repository)
        {
            _httpRequestMessage = httpRequestMessage;
            _recipeId = recipeId;
            _userContext = userContext;
            _repository = repository;
        }

        public ObjectResult Run()
        {
            //Validate all fields
            var validateResult = Validate();
            if (validateResult.Successful)
            {
                var recipe = _repository.Get<Recipe>(_recipeId);
                if (recipe != null)
                {
                    var httpResult = new SuccessfulHttpResult(recipe, HttpStatusCode.OK);
                    return new OkObjectResult(JsonConvert.SerializeObject(httpResult));    
                }

                //Return BadRequest if recipe cannot be found
                var error = new ErrorResult(HttpStatusCode.BadRequest, "Recipe not found.");
                return new BadRequestObjectResult(JsonConvert.SerializeObject(error));
            }

            return new BadRequestObjectResult(JsonConvert.SerializeObject(validateResult.ErrorResult));
        }

        private ValidateResult Validate()
        {
            //Validate recipe Id
            if (_recipeId == 0)
            {
                var error = new ErrorResult(HttpStatusCode.BadRequest, "Invalid contentId. Expected contentId greater than zero.");
                return new ValidateResult(successful: false, error);
            }
            
            //Validate UserContext
            try
            {
                _userContext.Validate();
            }
            catch (ArgumentNullException e)
            {
                var error = new ErrorResult(HttpStatusCode.Unauthorized, "No user found");
                return new ValidateResult(successful: false, error);
            }
            catch (ArgumentException e)
            {
                var error = new ErrorResult(HttpStatusCode.Unauthorized, e.Message);
                return new ValidateResult(successful: false, error);
            }
            catch (Exception e)
            {
                var error = new ErrorResult(HttpStatusCode.Unauthorized, "Error validating user");
                return new ValidateResult(successful: false, error);
            }

            return new ValidateResult(successful: true);
        }
    }

    public class ValidateResult
    {
        public bool Successful { get; }
        public ErrorResult ErrorResult { get; }
        public ValidateResult(bool successful, ErrorResult errorResult = null)
        {
            Successful = successful;
            ErrorResult = errorResult;
        }
    }
}