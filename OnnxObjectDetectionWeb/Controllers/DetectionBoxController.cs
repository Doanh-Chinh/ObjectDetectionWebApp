
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnnxObjectDetectionWeb.Models;
using OnnxObjectDetectionWeb.Utilities;
using SharedClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnnxObjectDetectionWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetectionBoxController : ControllerBase
    {
        private readonly IDetectionBoxRepository _detectionBoxRepository;
        private readonly string _DetectionBoxFolder;

        private List<DetectionBox> readTempBoundingBoxesJson()
        {
            // Specify the file name for the temporary JSON file for boxes
            string fileName = "tempBoundingBoxes.json";
            // Combine the folder path and file name
            string filePath = Path.Combine(_DetectionBoxFolder, fileName);
            // Read the content of the file
            string jsonContent = System.IO.File.ReadAllText(filePath);

            // Deserialize the JSON content into a List<BoundingBox>
            List<DetectionBox> detectionBoxes = JsonConvert.DeserializeObject<List<DetectionBox>>(jsonContent);
            return detectionBoxes;
        }

        public DetectionBoxController(IDetectionBoxRepository detectionBoxRepository)
        {
            _detectionBoxRepository = detectionBoxRepository;
            _DetectionBoxFolder = CommonHelpers.GetAbsolutePath(@"../../../assets/DetectionBoxes");
        }


        [HttpGet("{id:int}")]
        [Route("GetDetectionBox/{id}")]
        public async Task<ActionResult<DetectionBox>> GetDetectionBox(int id)
        {
            try
            {
                var result = await _detectionBoxRepository.GetDetectionBoxByBoxId(id);
                return Ok(result);
            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{imageId:int}")]
        [Route("GetDetectionBoxByImageId/{imageId}")]
        public async Task<ActionResult<DetectionBox>> GetDetectionBoxByImageId(int imageId)
        {
            try
            {
                if (imageId == -1) // if change url parameter, this block can be not excuted, default imageId = -1
                {
                    List<DetectionBox> detectionBoxes = readTempBoundingBoxesJson();
                    imageId = detectionBoxes[0].ImageId;

                }
                var result = await _detectionBoxRepository.GetDetectionBoxByImageId(imageId);
                return Ok(result);

            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, $"Message: {ex}");
            }
        }


        [HttpPost]
        [Route("CreateDetectionBoxes")]
        public async Task<ActionResult<DetectionBox>> CreateDetectionBoxes()
        {
            try
            {
                List<DetectionBox> detectionBoxes = readTempBoundingBoxesJson();

                // Check if the deserialization was successful
                if (detectionBoxes == null)
                {
                    return BadRequest("Unable to deserialize JSON content");
                }

                // Add the detection boxes to the repository
                var createdDetectionBox = await _detectionBoxRepository.AddDetectionBoxes(detectionBoxes);

                // Return the created detection box
                return CreatedAtAction(nameof(GetDetectionBoxByImageId), new { imageId = createdDetectionBox[0].ImageId }, createdDetectionBox); //createdDetectionBox.DetectionBoxId 

                //return StatusCode(StatusCodes.Status201Created, "Success save into database");

            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}"); }
        }


        [HttpDelete ("{imageId:int}")]
        [Route("DeleteDetectionBox/{imageId}")]
        public async Task<ActionResult> DeleteDetectionBox(int imageId)
        {
           
            try {
                List<DetectionBox> detectionBoxes = readTempBoundingBoxesJson();
                imageId = detectionBoxes[0].ImageId;

                var detetecionBoxesToDelete = await _detectionBoxRepository.GetDetectionBoxByImageId(imageId);  // return count 0 if no found in DB
                if (detetecionBoxesToDelete.Count() == 0)
                {
                    return NotFound($"No items to delete. ImageId = {imageId} is not found!");
                }
                await _detectionBoxRepository.DeleteDetectionBox(imageId);

                return Ok($"Detection Boxes with ImageId = {imageId} deleted");

            }
            catch (Exception)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "Error deleting detection box record");
            }
        }

        [HttpPut("{imageId:int}")]
        [Route("UpdateDetectionBoxes/{imageId}")]
        public async Task<ActionResult<List<DetectionBox>>> UpdateDetectionBoxes(int imageId)
        {
            try
            {
                List<DetectionBox>  newDetectionBoxes = readTempBoundingBoxesJson();
                imageId = newDetectionBoxes[0].ImageId;
                
                // check if already have those detectionbox before
                var detetecionBoxToUpdate = await _detectionBoxRepository.GetDetectionBoxByImageId(imageId);
                if (detetecionBoxToUpdate == null || detetecionBoxToUpdate.Count() == 0)
                {
                    return NotFound($"Detection Boxes with ImageId = {imageId} are not found to update. Save the detection results first!");
                }
                else
                {
                    var result = await _detectionBoxRepository.UpdateDetectionBoxes(imageId, newDetectionBoxes);
                    if (result != null)
                    {
                        return Ok($"Update Boxes with ImageId = {imageId} successfully");
                    }
                    return result;
                }

            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error updating detection box record: {ex}");
            }
        }

    }


}
