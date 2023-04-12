using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopManager.Models.CatalogViewModels
{
    public class CatalogTree
    {
        public int id { get; set; }

        public int idp { get; set; }

        public string n { get; set; }

        public List<CatalogTree> s { get; set; }
    }
}
