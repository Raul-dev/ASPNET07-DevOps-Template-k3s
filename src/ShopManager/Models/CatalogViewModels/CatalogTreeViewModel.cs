using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ShopManager.Models.CatalogViewModels
{
    public class CatalogTreeViewModel
    {

        /*
        public IEnumerable<CatalogItem> CatalogItems { get; set; }
        public IEnumerable<SelectListItem> Brands { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
        public int? BrandFilterApplied { get; set; }
        public int? TypesFilterApplied { get; set; }
        */
        public PaginationInfo PaginationInfo { get; set; }

        public CatalogTree[] CatalogTree;
        public IEnumerable<CatalogItem> CatalogItems { get; set; }
        public Catalog Catalog;
    }
}
