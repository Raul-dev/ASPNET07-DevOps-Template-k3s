using ShopManager.Models;
using Microsoft.AspNetCore.Mvc;
using ShopManager.Models.CatalogViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShopManager.Interfaces
{
    public interface ICatalogService
    {
        //public string RemotePictureUrl(int ItemId);
        public Task<Catalog> GetCatalogItems(int? CatalogId, int? page, int? take);
        public Task<CatalogTree[]> GetCatalogTree();
        public Stream GetCatalogPicture(int CatalogId);
        public HttpResponseMessage GetCatalogPicture1(int CatalogId);
        public Task<byte[]> GetCatalogPicture2(int CatalogId);
    }
}
