using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnnxObjectDetectionWeb.Infrastructure;
using OnnxObjectDetectionWeb.Services;
using OnnxObjectDetectionWeb.Utilities;
using OnnxObjectDetection;
using OnnxObjectDetectionWeb.Models;
using SharedClass;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace OnnxObjectDetectionWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectDetectionController : ControllerBase
    {
        private readonly string _imagesTmpFolder;
        private readonly string _imagesFolder;
        private readonly string _DetectionBoxFolder;

        private readonly ILogger<ObjectDetectionController> _logger;
        private readonly IDetectionBoxRepository _detectionBoxRepository;
        private  readonly IObjectDetectionService _objectDetectionService;
        private List<BoundingBox> filteredBoxes;
        private int imageID;

        private string base64String = string.Empty;
        //

        //[HttpGet]
        //public IActionResult GetBox()
        //{
        //    return Ok("TestAPI");
        //}
        //
        public ObjectDetectionController(IObjectDetectionService ObjectDetectionService, ILogger<ObjectDetectionController> logger, IImageFileWriter imageWriter,
            IDetectionBoxRepository detectionBoxRepository) //When using DI/IoC (IImageFileWriter imageWriter)
        {
            //Get injected dependencies
            _objectDetectionService = ObjectDetectionService;
            _logger = logger;
            _detectionBoxRepository = detectionBoxRepository;
            _imagesTmpFolder = CommonHelpers.GetAbsolutePath(@"../../../ImagesTemp");
            _imagesFolder = CommonHelpers.GetAbsolutePath(@"../../../assets/imagesList");
            _DetectionBoxFolder = CommonHelpers.GetAbsolutePath(@"../../../assets/DetectionBoxes");


        }

        public class Result
        {
            public string imageString { get; set; }
        }


        [HttpGet("{id:int}")]
        [HttpGet()]
        public IActionResult Get(int id, [FromQuery] string url)  // url--> image source in index.cshtml.  public IActionResult Get([FromQuery] string url)
        {
            if (id != 0)
            {
                url = $"/imagesList/image{id}.jpg";
                imageID = id;

            }
            else
            {
                // Define a regular expression pattern to match the numeric part
                string pattern = @"(\d+)";

                // Use Regex.Match to find the match in the input string
                Match match = Regex.Match(url, pattern);

                // Check if a match was found
                if (match.Success)
                {
                    // Extract the numeric part from the match
                    int numericPart = Int32.Parse(match.Groups[1].Value);
                    imageID = numericPart;

                }
                else
                {
                    // No match found
                    imageID = -1;
                }
            }
            string imageFileRelativePath = @"../../../assets" + url;
            string imageFilePath = CommonHelpers.GetAbsolutePath(imageFileRelativePath);
            //
            //imageFilePath = @"~/assets/imageList/image1.jpg";
            //
            try
            {
                Image image = Image.FromFile(imageFilePath);
                //Convert to Bitmap
                Bitmap bitmapImage = (Bitmap)image;

                //Set the specific image data into the ImageInputData type used in the DataView
                ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

                //Detect the objects in the image                
                var result = DetectAndPaintImage(imageInputData, imageFilePath);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                //return BadRequest();
                return NotFound();
            }
        }


        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("IdentifyObjects")]
        public async Task<IActionResult> IdentifyObjects(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
                //return BadRequest();
                return NotFound();
            try
            {
                MemoryStream imageMemoryStream = new MemoryStream();
                await imageFile.CopyToAsync(imageMemoryStream);

                //Check that the image is valid
                byte[] imageData = imageMemoryStream.ToArray();
                if (!imageData.IsValidImage())
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType);

                //Convert to Image
                Image image = Image.FromStream(imageMemoryStream);
                string fileName = string.Format("{0}.Jpeg", image.GetHashCode());
                string imageFilePath = Path.Combine(_imagesTmpFolder, fileName);
                //save image to a path
                image.Save(imageFilePath, ImageFormat.Jpeg);

                //Convert to Bitmap
                Bitmap bitmapImage = (Bitmap)image;

                _logger.LogInformation($"Start processing image...");

                //Measure execution time
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Set the specific image data into the ImageInputData type used in the DataView
                ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

                //Detect the objects in the image                
                var result = DetectAndPaintImage(imageInputData, imageFilePath);

                //Stop measuring time
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                //return BadRequest();
                return NotFound();
            }
        }

        public class ImageRequestModel
        {
            public int ImageID { get; set; }
            public string ImageFile { get; set; }
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Route("IdentifyObjects2")]
        public IActionResult IdentifyObjects2([FromBody] ImageRequestModel requestModel)
        {
            try
            {
                if (requestModel == null || string.IsNullOrWhiteSpace(requestModel.ImageFile))
                    //return BadRequest();
                    return NotFound();

                // Convert base64 string to byte array
                byte[] imageData = Convert.FromBase64String(requestModel.ImageFile);

                // Check that the image is valid
                if (!imageData.IsValidImage())
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType);

                // Convert to Image
                using (MemoryStream imageMemoryStream = new MemoryStream(imageData))
                using (Image image = Image.FromStream(imageMemoryStream))
                {
                    string fileName = $"{Guid.NewGuid()}.jpeg";
                    string imageFilePath = Path.Combine(_imagesTmpFolder, fileName);

                    // Save image to a path
                    image.Save(imageFilePath, ImageFormat.Jpeg);

                    // Save image to another path
                    imageID = requestModel.ImageID;
                    string fileName2 = $"Image{imageID}.jpg";
                    string imageFilePath2 = Path.Combine(_imagesFolder, fileName2);
                    image.Save(imageFilePath2);

                    // Convert to Bitmap
                    using (Bitmap bitmapImage = new Bitmap(image))
                    {
                        _logger.LogInformation("Start processing image...");

                        // Measure execution time
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        // Set the specific image data into the ImageInputData type used in the DataView
                        ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

                        // Detect the objects in the image
                        var result = DetectAndPaintImage(imageInputData, imageFilePath);

                        // Stop measuring time
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        _logger.LogInformation($"Image processed in {elapsedMs} milliseconds");

                        return Ok(result);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                //return BadRequest();
                return NotFound();
            }
        }


        private Result DetectAndPaintImage(ImageInputData imageInputData, string imageFilePath)
        {
            //Predict the objects in the image
            filteredBoxes = _objectDetectionService.DetectObjectsUsingModel(imageInputData);
            var img = _objectDetectionService.DrawBoundingBox(imageFilePath);

            // Save bounding box detected

            List<DetectionBox> detectionBoxes = GetDetectionBoxes();

            // Specify the file name for the temporary JSON file for boxes
            string fileName = "tempBoundingBoxes.json";

            // Combine the folder path and file name
            string filePath = Path.Combine(_DetectionBoxFolder, fileName);

            // Serialize the List<BoundingBox> to JSON and write it to the file
            string jsonContent = JsonConvert.SerializeObject(detectionBoxes, Formatting.Indented);
            System.IO.File.WriteAllText(filePath, jsonContent);

            //

            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, img.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                base64String = Convert.ToBase64String(imageBytes);
                var result = new Result { imageString = base64String };
                return result;
            }
        }

        

        private List<DetectionBox> GetDetectionBoxes()
        {
            List<DetectionBox> detectionBoxes = new List<DetectionBox>();
            foreach (var box in filteredBoxes)
            {
                detectionBoxes.Add(new DetectionBox()
                {
                    Label = box.Label,
                    Confidence = box.Confidence,
                    X = box.Dimensions.X,
                    Y = box.Dimensions.Y,
                    Width = box.Dimensions.Width,
                    Height = box.Dimensions.Height,
                    ImageId = imageID,
                    ModifiedDate = DateTime.Now
                });

            }
            return detectionBoxes;
        }

    }

}
