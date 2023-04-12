using Microsoft.AspNetCore.Mvc;

using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using Azure;
using NuGet.Protocol;
using Microsoft.Extensions.Options;
using CatalogApi.Extensions;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Sample : ControllerBase
    {
        private readonly IOptions<IdentityS4Settings> _IdentityS4Settings;

        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

      

        public Sample(IOptions<IdentityS4Settings> identityS4Settings)
        {
            _IdentityS4Settings = identityS4Settings;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpGet]
        [Route("/[controller]/[action]/{id}")]
        public ActionResult Overview(int? id)
        {
            return Ok(id);
        }

        [HttpGet]
        [Route("/[controller]/[action]")]
        public async Task<string> GetToken()
        {
            var tokenResponse = await GetAutorisationToken();

            if (tokenResponse == null)
                return "null";
            return tokenResponse.AccessToken.ToString();
        }
        
        [HttpGet]
        [Route("/[controller]/[action]")]
        public async Task<string> GetRoot()
        {
            var tokenResponse = await GetAutorisationToken();

            if (tokenResponse == null)
                return "null";

            // call api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync(_IdentityS4Settings.Value.BaseURL);
            if (!response.IsSuccessStatusCode)
            {
                
                Log.Debug(response.StatusCode.ToString());
                return "failed :(" + response.StatusCode;
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
        }
        protected async Task<TokenResponse> GetAutorisationToken()
        {
            
            Log.Debug($"GetDiscoveryDocumentAsync: {_IdentityS4Settings.Value.AuthorityURL}");
            var client = new HttpClient();
            //var disco = await client.GetDiscoveryDocumentAsync(_IdentityS4Settings.Value.AuthorityURL);
            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _IdentityS4Settings.Value.AuthorityURL,
                Policy =
                {
                    RequireHttps = false
                }
            });

            if (disco.IsError)
            {
                Console.WriteLine($"Disco error: {disco.Error}");
                Log.Error($"Disco error:  { disco.Error}");
                return null;
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "catalog.client.api",
                ClientSecret = _IdentityS4Settings.Value.Secret,
                Scope = "catalog.api.read"
            });
            

            if (tokenResponse.IsError)
            {
                Console.WriteLine($"Token error: {disco.Error}");
                Log.Error($"Token error: {tokenResponse.Error}");

                return null;
            }
            
            Log.Debug(tokenResponse.Json.ToString());
            return tokenResponse;

        }
    }
}