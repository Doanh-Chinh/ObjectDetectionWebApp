using Microsoft.EntityFrameworkCore;
using SharedClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnnxObjectDetectionWeb.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            :base(options)
        {

        }

        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<DetectionBox> DetectionBoxes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Seed data for UserImage table
            modelBuilder.Entity<UserImage>().HasData(
                new UserImage { ImageId = 1, ImageName = "image1", ImagePath = "imagesList/image1.jpg" },
                new UserImage { ImageId = 2, ImageName = "image2", ImagePath = "imagesList/image2.jpg" },
                new UserImage { ImageId = 3, ImageName = "image3", ImagePath = "imagesList/image3.jpg" },
                new UserImage { ImageId = 4, ImageName = "image4", ImagePath = "imagesList/image4.jpg" }
            );
        }

    }
}
