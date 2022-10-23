using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Loup.DotNet.Challenge.FunctionApp;
using Loup.DotNet.Challenge.FunctionApp.Models;
using Loup.DotNet.Challenge.TestFramework;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Loup.DotNet.Challenge.Tests
{
    /// <summary>
    /// Add your tests to this class.
    /// </summary>
    [Collection(nameof(FunctionAppTestHost))]
    public class Tests
    {
        private readonly ITestOutputHelper _helper;
        private readonly FunctionAppTestHost _testHost;
        private readonly string _endpoint = "http://localhost:7071/api/recipes";
        private readonly UserContext _user;

        public Tests(ITestOutputHelper helper, FunctionAppTestHost testHost)
        {
            _helper = helper;
            _testHost = testHost;
            _user = new UserContext(id: "123", isSubscribed: true, isAuthenticated: true, firstName: "Shanuka", lastName: "Gomes", created: DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Test200Response()
        {
            //Arrange
            int contentId = 5143;
            var recipe = new Recipe()
            {
                ContentId = contentId,
                ContentType = 1,
                Name = "Green Super Smoothie",
                Summary = "This is a Super Smoothie, specifically created for men and women on the Build Muscle goal. Our post-workout Super Smoothies make it easier to meet your daily energy needs for training and muscle gain. To view the full selection or swap to a different flavor for your post-workout meal, search “super smoothie” in Meals.",
                UrlPartial = "green-super-smoothie",
                ServingSize = 0,
                Energy = 1750.93,
                Calories = 418.1929,
                Carbs = 43.668,
                Protein = 33.07,
                DietryFibre = 9.9745,
                Fat = 39.038,
                SatFat = 2.372,
                Sugar = 39.038
            };

            _testHost.MockRepository.Setup(x => x.Get<Recipe>(contentId)).Returns(recipe); //Mock repository Get() to return mocked recipe 
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get, 
                RequestUri = new Uri($"{_endpoint}/{contentId}")
            };
            httpRequest.SetupUserContext(_user.Id, _user.IsAuthenticated, _user.IsSubscribed, _user.FirstName, _user.LastName);
            var client = _testHost.CreateClient();
            
            //Act
            var responseMessage = await client.SendAsync(httpRequest, new CancellationToken());
            var response = await responseMessage.Content.ReadAsStringAsync();
            var httpResult = JsonConvert.DeserializeObject<SuccessfulHttpResult>(response);
            
            //Get the expected sample response 
            var targetResponse = _testHost.Entities.SingleOrDefault(x => x.Key == $"{contentId}_{(int)HttpStatusCode.OK}");
            var targetRecipe = JsonConvert.DeserializeObject<Recipe>(targetResponse.Value);
            
            //Assert
            var expectedHttpCode = Convert.ToInt32(targetResponse.Key.Split("_")[1]); //Decode the HttpStatusCode from the sample file to compare
            Assert.Equal(expectedHttpCode, (int)httpResult.HttpStatusCode);
            Assert.True(targetRecipe != null && httpResult != null && targetRecipe.Equals(httpResult.Recipe));
        }

        [Fact]
        public async Task Test400Response()
        {
            //Arrange
            int contentId = 0;
            var recipe = new Recipe() { ContentId = contentId };
            
            _testHost.MockRepository.Setup(x => x.Get<Recipe>(contentId)).Returns(recipe);
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get, 
                RequestUri = new Uri($"{_endpoint}/{contentId}")
            };
            httpRequest.SetupUserContext(_user.Id, _user.IsAuthenticated, _user.IsSubscribed, _user.FirstName, _user.LastName);
            var client = _testHost.CreateClient();

            //Act
            var responseMessage = await client.SendAsync(httpRequest, new CancellationToken());
            var response = await responseMessage.Content.ReadAsStringAsync();
            var errorResult = JsonConvert.DeserializeObject<ErrorResult>(response);
            
            //Get the expected sample response
            var targetedResponse = _testHost.Entities.SingleOrDefault(x => x.Key == $"5143_{(int)HttpStatusCode.BadRequest}");
            var targetErrorResult = JsonConvert.DeserializeObject<ErrorResult>(targetedResponse.Value);
            
            Assert.NotNull(targetErrorResult);
            Assert.NotNull(errorResult);
            Assert.Equal(targetErrorResult.HttpStatusCode, errorResult.HttpStatusCode);
            Assert.Equal(targetErrorResult.Message, errorResult.Message);
        }
        
        [Fact]
        public async Task Test401Response()
        {
            //Arrange
            int contentId = 5143;
            var recipe = new Recipe() { ContentId = contentId };
            _testHost.MockRepository.Setup(x => x.Get<Recipe>(contentId)).Returns(recipe);
            
            _user.SetIsAuthenticated(false);
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get, 
                RequestUri = new Uri($"{_endpoint}/{contentId}")
            };
            httpRequest.SetupUserContext(_user.Id, _user.IsAuthenticated, _user.IsSubscribed, _user.FirstName, _user.LastName);
            var client = _testHost.CreateClient();

            //Act
            var responseMessage = await client.SendAsync(httpRequest, new CancellationToken());
            var response = await responseMessage.Content.ReadAsStringAsync();
            var errorResult = JsonConvert.DeserializeObject<ErrorResult>(response);

            //Get the expected sample response
            var targetResponse = _testHost.Entities.SingleOrDefault(x => x.Key == $"5143_{(int)HttpStatusCode.Unauthorized}");
            var targetResult = JsonConvert.DeserializeObject<ErrorResult>(targetResponse.Value);

            //Assert
            Assert.NotNull(targetResult);
            Assert.NotNull(errorResult);
            Assert.Equal(targetResult.HttpStatusCode, errorResult.HttpStatusCode);
            Assert.Contains(targetResult.Message, errorResult.Message);
        }
    }
}