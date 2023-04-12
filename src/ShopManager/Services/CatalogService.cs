using ShopManager.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using ShopManager.Models.CatalogViewModels;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;
using Serilog;
using ShopManager.Interfaces;

namespace ShopManager.Services
{
    public class CatalogService : ICatalogService
    {

        private readonly HttpClient _httpClient;
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly string _remoteServiceBaseUrl;
        private readonly string _catalogUrl;
        /*        public async Task<IActionResult> Catalog()
                {
                    CatalogTreeViewModel model = new CatalogTreeViewModel();
                    model.Catalog = await GetCatalog();
                    // Pass the data into the View
                    return View(model);
                }
        */

        public CatalogService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _apiSettings = apiSettings;
            
            _httpClient = httpClient;
            _remoteServiceBaseUrl = $"{_apiSettings.Value.CatalogUrl}/v1";
            _catalogUrl = _apiSettings.Value.CatalogUrl;

            
            Log.Information("Initial parameter CatalogHost_EXTERNAL {0} , CatalogUrl {1}", _apiSettings.Value.CatalogHost_EXTERNAL, _apiSettings.Value.CatalogUrl);
        }
        public CatalogService(string CatalogUrl)
        //HttpClient httpClient, ILoggerFactory loggerFactory, IOptions<ApiSettings> apiSettings)
        {
            //_apiSettings = apiSettings;
            
            _httpClient = new HttpClient(); //httpClient;
            _remoteServiceBaseUrl = $"{CatalogUrl}/v1";

        }
        protected string RemotePictureUrl(int ItemId)
        {
            return $"{_catalogUrl}/c/v1/{ItemId}/pic";
        }

        public async Task<CatalogTree[]> GetCatalogTree()
        {
            // Get an instance of HttpClient from the factpry that we registered
            // in Startup.cs
            var client = _httpClient; //.CreateClient("Catalog API Client");

            // Call the API & wait for response. 
            // If the API call fails, call it again according to the re-try policy
            // specified in Startup.cs
            string uri = _remoteServiceBaseUrl + "/tree";
            //var result = await client.GetAsync(uri);
            var result = await _httpClient.GetAsync(uri);

            if (result.IsSuccessStatusCode)
            {
                // Read all of the response and deserialise it into an instace of
                // WeatherForecast class
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CatalogTree[]>(content);
            }
            return null;
        }

        public async Task<Catalog> GetCatalogItems(int? CatalogId, int? page, int? take)
        {
            var uri = GetAllCatalogItems(_remoteServiceBaseUrl, CatalogId, page, take);
            //string uri = _remoteServiceBaseUrl + "/items";
            try
            {
                var result = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
                if (result.IsSuccessStatusCode)
                {
                    var responseString = await result.Content.ReadAsStringAsync();
                    var catalog = JsonConvert.DeserializeObject<Catalog>(responseString);

                    return catalog;
                }
            } catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            return null;
        }
        string GetAllCatalogItems(string baseUri, int? CatalogId, int? page, int? take)
        {
            var filterQs = "";
            string pindx = "";


            if (CatalogId.HasValue)
            {
                var catQs = (CatalogId.HasValue) ? CatalogId.Value.ToString() : string.Empty;
                filterQs = $"catalogId={catQs}&";

            }
            /*
            if (type.HasValue)
            {
                var brandQs = (brand.HasValue) ? brand.Value.ToString() : string.Empty;
                filterQs = $"/type/{type.Value}/brand/{brandQs}";

            }
            else if (brand.HasValue)
            {
                var brandQs = (brand.HasValue) ? brand.Value.ToString() : string.Empty;
                filterQs = $"/type/all/brand/{brandQs}";
            }
            else
            {
                filterQs = string.Empty;
            }
            */
            if (!take.HasValue)
                take = 6;

            if (!page.HasValue)
                pindx = $"& pageIndex ={page}";

            return $"{baseUri}/items?{filterQs}pageSize={take}{pindx}";
        }

        public Stream GetCatalogPicture(int CatalogId)
        {
            // Call the API & wait for response. 
            // If the API call fails, call it again according to the re-try policy
            // specified in Startup.cs
            string uri = _remoteServiceBaseUrl + $"/items/{CatalogId}/pic";
            //var result = await client.GetAsync(uri);
            var result = _httpClient.GetStreamAsync(uri).Result;
            return result;
        }

        
        public HttpResponseMessage GetCatalogPicture1(int CatalogId)
        {
            // Call the API & wait for response. 
            // If the API call fails, call it again according to the re-try policy
            // specified in Startup.cs
            string uri = _remoteServiceBaseUrl + $"/items/{CatalogId}/pic";
            //var result = await client.GetAsync(uri);
            var result = _httpClient.GetAsync(uri).Result;
            //var result1 = result.Content.ReadAsByteArrayAsync() ;
            //var dd = new IActionResult();
            return result; 
        }

        public async Task<byte[]> GetCatalogPicture2(int CatalogId)
        {
            // Call the API & wait for response. 
            // If the API call fails, call it again according to the re-try policy
            // specified in Startup.cs
            string uri = _remoteServiceBaseUrl + $"/items/{CatalogId}/pic";
            
            byte[] mybytearray = null;
            var response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                mybytearray = await response.Content.ReadAsByteArrayAsync();//Here is the problem
            }
            return mybytearray;
        }

    }
}
