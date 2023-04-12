using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using static System.Net.WebRequestMethods;
using ShopManager.Interfaces;
using ShopManager.Models.CatalogViewModels;
using ShopManager.Models;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ShopManager.Controllers
{
    [Authorize]
    public class CatalogController : Controller
    {
        private ICatalogService _catalog;
        private CatalogTree[] _catalogtree;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public CatalogController(ICatalogService catalog, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {

            _catalog = catalog;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> IndexCatalog(int? CatalogId, int? BrandFilterApplied, int? TypesFilterApplied, int? page, [FromQuery] string errorMsg)
        {
            //CatalogViewModel model = new CatalogViewModel();
            //model.Catalog = await GetCatalog();
            // Pass the data into the View
            int _page;
            var itemsPage = 6;
            _page = page == null ? 0 : page.Value;

            _catalogtree = await _catalog.GetCatalogTree();
            var catalog = await _catalog.GetCatalogItems(CatalogId, _page, itemsPage);

            //
            //var catalog = await _catalog.GetCatalogItems(0, page ?? 0, itemsPage, BrandFilterApplied, TypesFilterApplied);

            var vm = new IndexViewModel()
            {
                //CatalogItems = catalog.Data,
                //Brands = await _catalog.GetBrands(),
                //Types = await _catalog.GetTypes(),
                //BrandFilterApplied = BrandFilterApplied ?? 0,
                //TypesFilterApplied = TypesFilterApplied ?? 0,

                Catalog = catalog,
                CatalogItems = catalog.Data,
                CatalogTree = _catalogtree,
                PaginationInfo = new PaginationInfo()
                {
                    ActualPage = page ?? 0,
                    ItemsPerPage = itemsPage, // catalog.Data.Count,
                    TotalItems = catalog.Count,
                    TotalPages = (int)Math.Ceiling((decimal)catalog.Count / itemsPage)
                }
            };

            vm.PaginationInfo.Next = vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1 ? "is-disabled" : "";
            vm.PaginationInfo.Previous = vm.PaginationInfo.ActualPage == 0 ? "is-disabled" : "";

            ViewBag.BasketInoperativeMsg = errorMsg;

            return View(vm);

        }

        [HttpPost]
        public async Task<IActionResult> _catalogitems(int? CatalogId, int? BrandFilterApplied, int? TypesFilterApplied, int? page, [FromQuery] string errorMsg)
        {
            //CatalogViewModel model = new CatalogViewModel();
            //model.Catalog = await GetCatalog();
            // Pass the data into the View
            int _page;
            var itemsPage = 6;
            _page = page == null ? 0 : page.Value;

            //var catalogtree = await _catalog.GetCatalogTree();
            var catalog = await _catalog.GetCatalogItems(CatalogId, _page, itemsPage);

            //
            //var catalog = await _catalog.GetCatalogItems(0, page ?? 0, itemsPage, BrandFilterApplied, TypesFilterApplied);

            var vm = new IndexViewModel()
            {
                //CatalogItems = catalog.Data,
                //Brands = await _catalog.GetBrands(),
                //Types = await _catalog.GetTypes(),
                //BrandFilterApplied = BrandFilterApplied ?? 0,
                //TypesFilterApplied = TypesFilterApplied ?? 0,

                Catalog = catalog,
                CatalogItems = catalog.Data,
                CatalogTree = _catalogtree,
                PaginationInfo = new PaginationInfo()
                {
                    ActualPage = page ?? 0,
                    ItemsPerPage = itemsPage, // catalog.Data.Count,
                    TotalItems = catalog.Count,
                    TotalPages = (int)Math.Ceiling((decimal)catalog.Count / itemsPage)
                }
            };

            vm.PaginationInfo.Next = vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1 ? "is-disabled" : "";
            vm.PaginationInfo.Previous = vm.PaginationInfo.ActualPage == 0 ? "is-disabled" : "";

            ViewBag.BasketInoperativeMsg = errorMsg;
            //$"Unexpected error occurred deleteing user with ID '{userId}'."
            Log.Information($"CatalogId '{CatalogId}', page '{page}', Count '{vm.CatalogItems.Count()}'");
            return View(vm);
            // return View("_catalogitems", "_LayoutEmpty", vm);

        }

        /*  Proxy
        [HttpGet]
        [Route("v1/items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        // GET: /<controller>/
        public async Task<ActionResult> GetImageAsync(int catalogItemId)
        {
            if (catalogItemId <= 0)
            {
                return BadRequest();
            }
            try
            {
                string mimetype = "asd";
                //var buffer = System.IO.File.ReadAllBytes("pics/1.png");
                //var test = _catalog.GetCatalogPicture1(catalogItemId);
                //byte[] buffer1 = test.
                //Byte[] b = (GetImageByteArray());

                //var buffer = _catalog.GetCatalogPicture2(catalogItemId);
                var stream =  _catalog.GetCatalogPicture(catalogItemId);
                var ad= File(stream, "image/png");
                
                return ad;
            }
            catch (Exception)
            {
               return NotFound();
            }
         }
        */
        public IActionResult Catalog()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
