using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnnxObjectDetectionWeb.Models
{
    public class DetectionBoxRepository : IDetectionBoxRepository
    {
        private readonly AppDbContext appDbContext;

        public DetectionBoxRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        public async Task<List<DetectionBox>> AddDetectionBoxes(List<DetectionBox> detectionBoxes)
        {
            List<DetectionBox> results = new List<DetectionBox>();
            EntityEntry<DetectionBox> result = null;

            foreach (var item in detectionBoxes)
            {
                result = await appDbContext.DetectionBoxes.AddAsync(item);
                results.Add(result.Entity);
                await appDbContext.SaveChangesAsync();
            }


            return results;
        }

        public async Task DeleteDetectionBox(int imageId)
        {
            var boxesToDelete = await appDbContext.DetectionBoxes
                                                .Where(box => box.ImageId == imageId)
                                                .ToListAsync();

            if (boxesToDelete.Any())
            {
                appDbContext.DetectionBoxes.RemoveRange(boxesToDelete);
                await appDbContext.SaveChangesAsync();
            }
        }

        public async Task<DetectionBox> GetDetectionBoxByBoxId(int detectionBoxId)
        {
            DetectionBox detectionBox = await appDbContext.DetectionBoxes.FindAsync(detectionBoxId);

            return detectionBox;
        }

        public async Task<IEnumerable<DetectionBox>> GetDetectionBoxByImageId(int userImageId)
        {
            IQueryable<DetectionBox> query = appDbContext.DetectionBoxes;
            query = query.Where(box => box.ImageId == userImageId);

            return await query.ToListAsync();
        }


        public Task<IEnumerable<DetectionBox>> GetDetectionBoxes()
        {
            throw new NotImplementedException();
        }

        public async Task<List<DetectionBox>> UpdateDetectionBoxes(int imageId, List<DetectionBox> newDetectionBoxes)
        {
            // delete old
            await DeleteDetectionBox(imageId);

            // insert new
            var createdNewDetectionBox = await AddDetectionBoxes(newDetectionBoxes);

            await appDbContext.SaveChangesAsync();
            return createdNewDetectionBox;
            
        }

    }
}
