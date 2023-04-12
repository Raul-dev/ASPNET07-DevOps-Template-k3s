using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using CatalogApi.ViewModel;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using CatalogApi.Data.Models;
using CatalogApi.Data;
using Serilog;
namespace CatalogApi.Controllers
{
    [ApiController]
    [Route("v1")]
    public class CatalogController : ControllerBase
    {


        //private static List<CatalogItem> _itemsList;
        //private static IEnumerable<CatalogTree> _treeArray;
        private static CatalogDataFake _catalogData;
        public CatalogController()
        {

            _catalogData = new CatalogDataFake();
            Refresh().Wait();
        }

        [HttpPut]
        [Route("refresh")]
        public async Task Refresh(bool bForce = false)
        {
            await _catalogData.Refresh();
        }

        [HttpGet]
        [Route("tree")]

        public async Task<IEnumerable<CatalogTree>> GetTree()
        {
            await Refresh();
            return _catalogData.GetTreeArray();
        }
        // GET api/v1/[controller]/items[?pageSize=3&pageIndex=10]
        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]

        public async Task<IActionResult> ItemsAsync([FromQuery] int catalogId = 0, [FromQuery] int pageSize = 6, [FromQuery] int pageIndex = 0, string ids = null)
        {

            await Task.Run(() => { Log.Information("Item generation"); });
            //long totalItems = 10;
            if (catalogId != 0)
            {
                await Task.Run(() => { Log.Information("Item generation by CatalogId"); });
                var items0 = await _catalogData.GetItemsByCatalogIdsAsync(catalogId);
                var model0 = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, items0.Count, items0);
                return Ok(model0);
            }

            if (!string.IsNullOrEmpty(ids))
            {
                await Task.Run(() => { Log.Information("Item generation by ids"); });
                var items = await _catalogData.GetItemsByIdsAsync(ids);

                //if (!items.Any())
                //{
                //    return BadRequest("ids value invalid. Must be comma-separated list of numbers");
                //}
                var model1 = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, items.Count, items);
                return Ok(model1);
            }



            var itemsOnPage = _catalogData.GetItemsList()
                            .OrderBy(c => c.Name)
                            .Skip(pageSize * pageIndex)
                            .Take(pageSize);
            //.ToListAsync();

            var model = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, _catalogData.GetItemsList().Count(), itemsOnPage);

            return Ok(model);
        }

        [HttpGet]
        [Route("GetValue")]
        public async Task<IActionResult> GetValue()
        {

            //If(userdb.connect() == “Successful”)
            //{
            //    headers ={‘http_status’:200, ‘cache - control’:  ‘no - cache’}
            //    body ={‘status’: ‘available’}
            //}
            //Else {
            //    headers ={‘http_status’:500, ‘cache - control’:  ‘no - cache’}
            //    body ={‘status’: ‘unavailable’}
            //}

            return Ok(123);
        }
        [HttpGet]
        [Route("healthcheck")]
        public IActionResult GetHealthcheck()
        {

            //If(userdb.connect() == “Successful”)
            //{
            //    headers ={‘http_status’:200, ‘cache - control’:  ‘no - cache’}
            //    body ={‘status’: ‘available’}
            //}
            //Else {
            //    headers ={‘http_status’:500, ‘cache - control’:  ‘no - cache’}
            //    body ={‘status’: ‘unavailable’}
            //}
            var i = 0;
            //var p= 23/i;

            return Ok();
        }
    }
}
