using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using Loup.DotNet.Challenge.TestFramework;

namespace Loup.DotNet.Challenge.FunctionApp
{
    public class GetRecipeFunction
    {
        private readonly IRepository _repository;

        public GetRecipeFunction(IRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("GetRecipe")]
        public Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "recipes/{recipeId}")] HttpRequestMessage req, int recipeId, UserContext user)
        {
            //Initialize function handler
            var functionHandler = new GetRecipeFunctionHandler(req, recipeId, user, _repository);
            
            //Run 
            var initResult = functionHandler.Run();
            
            //Return result
            return Task.FromResult<IActionResult>(initResult);
        }
    }
}




