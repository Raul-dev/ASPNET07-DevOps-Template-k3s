using CatalogApi.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CatalogApi.Data
{
    public class ProductApiContext : DbContext
    {
        public ProductApiContext(DbContextOptions options) : base(options)
        {
        }


        public DbSet<CatalogApi.Data.Models.ProductSubCategory> ProductSubCategory { get; set; } = default!;

        public DbSet<CatalogApi.Data.Models.Product> Product { get; set; } = default!;

        public DbSet<CatalogApi.Data.Models.Image> Image { get; set; } = default!;
    }
}
