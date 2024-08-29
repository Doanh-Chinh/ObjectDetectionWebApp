using SharedClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnnxObjectDetectionWeb.Models
{
    public interface IDetectionBoxRepository
    {
        // get all boxes
        Task<IEnumerable<DetectionBox>> GetDetectionBoxes();
        // get by box id
        Task<DetectionBox> GetDetectionBoxByBoxId(int detectionBoxId);
        // get by image id
        Task<IEnumerable<DetectionBox>> GetDetectionBoxByImageId(int userImageId);

        // add detection box
        Task<List<DetectionBox>> AddDetectionBoxes(List<DetectionBox> detectionBoxes);

        // update detection box
        Task<List<DetectionBox>> UpdateDetectionBoxes(int imageId, List<DetectionBox> detectionBoxes);


        // delete detection box
        Task DeleteDetectionBox(int imageId);
    }
}
