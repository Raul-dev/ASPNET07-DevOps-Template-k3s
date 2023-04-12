using CatalogApi.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CatalogApi.Controllers
{
    [ApiController]
    public class PicController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CatalogController> _logger;
        // private readonly CatalogContext _catalogContext;

        public PicController(IWebHostEnvironment env, ILogger<CatalogController> logger)
        {
            _env = env;
            _logger = logger;
            //  _catalogContext = catalogContext;
        }

        [HttpGet]
        [Route("v1/items/{catalogItemId:int}/pic")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        // GET: /<controller>/
        public ActionResult GetImageAsync(int catalogItemId)
        {
            if (catalogItemId <= 0)
            {
                return BadRequest();
            }
            CatalogItem ci = new CatalogItem()
            {
                Id = catalogItemId,
                Name = "s",
                Description = "as",
                Price = 12,
                PictureUri = catalogItemId.ToString() + ".png"
            };
            try
            {

                var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "pics"); ;
                var path = Path.Combine(webRoot, ci.PictureUri);
                Log.Information("Path to pictures {0}", path);
                string imageFileExtension = Path.GetExtension(ci.PictureUri);
                string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = System.IO.File.ReadAllBytes(path);

                return File(buffer, mimetype);
            }
            catch (Exception e)
            {
                Log.Error("Get picture exeption: {0}", e.Message);
                return NotFound();
            }
            /*var item = await _catalogContext.CatalogItems
            .SingleOrDefaultAsync(ci => ci.Id == catalogItemId);

        if (item != null)
        {
            var webRoot = _env.WebRootPath;
            var path = Path.Combine(webRoot, item.PictureFileName);

            string imageFileExtension = Path.GetExtension(item.PictureFileName);
            string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

            var buffer = System.IO.File.ReadAllBytes(path);

            return File(buffer, mimetype);
        }*/

            //  return NotFound();
        }

        private string GetImageMimeTypeFromImageFileExtension(string extension)
        {
            string mimetype;

            switch (extension)
            {
                case ".png":
                    mimetype = "image/png";
                    break;
                case ".gif":
                    mimetype = "image/gif";
                    break;
                case ".jpg":
                case ".jpeg":
                    mimetype = "image/jpeg";
                    break;
                case ".bmp":
                    mimetype = "image/bmp";
                    break;
                case ".tiff":
                    mimetype = "image/tiff";
                    break;
                case ".wmf":
                    mimetype = "image/wmf";
                    break;
                case ".jp2":
                    mimetype = "image/jp2";
                    break;
                case ".svg":
                    mimetype = "image/svg+xml";
                    break;
                default:
                    mimetype = "application/octet-stream";
                    break;
            }

            return mimetype;
        }
        // POST: api/Image
        [HttpPost]
        [Route("v1/items/putpic")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            // {catalogItemId:int}/
            if (string.IsNullOrWhiteSpace(_env.WebRootPath))
            {
                _env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploads = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return Ok();
        }

    }
}
