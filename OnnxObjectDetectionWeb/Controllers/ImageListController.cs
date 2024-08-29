using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnnxObjectDetectionWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace OnnxObjectDetectionWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageListController : ControllerBase
    {

        private readonly ILogger<ImageListController> _logger;
        private string base64String = string.Empty;

        public ImageListController(ILogger<ImageListController> logger) //When using DI/IoC (IImageFileWriter imageWriter)
        {
            //Get injected dependencies
            _logger = logger;
        }

        public class Result
        {
            public int imageId { get; set; }
            public string imageString { get; set; }
        }

        [HttpGet]
        public IActionResult Get()
        {
            string[] urls = new string[4];
            //List<string> urls = new List<string>();
            for (int i = 0; i < urls.Length; i++)
            {
                urls[i] = $"/imagesList/image{i + 1}.jpg";

            }
            Result[] results = new Result[urls.Length];
            for (int i = 0; i < urls.Length; i++)
            {
                string imageFileRelativePath = @"../../../assets" + urls[i];
                string imageFilePath = CommonHelpers.GetAbsolutePath(imageFileRelativePath);
                try
                {
                    Image image = Image.FromFile(imageFilePath);
                    //Convert to Bitmap
                    Bitmap img = (Bitmap)image;

                    using (MemoryStream m = new MemoryStream())
                    {
                        img.Save(m, img.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        base64String = Convert.ToBase64String(imageBytes);
                        var result = new Result { imageId = i+1, imageString = base64String };
                        results[i] = result;
                    }


                }
                catch (Exception e)
                {
                    _logger.LogInformation("Error is: " + e.Message);
                    return BadRequest();
                    //return NotFound();
                }

            }
            return Ok(results);

        }
    }
}
