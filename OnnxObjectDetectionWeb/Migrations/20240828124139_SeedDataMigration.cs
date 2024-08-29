using Microsoft.EntityFrameworkCore.Migrations;

namespace OnnxObjectDetectionWeb.Migrations
{
    public partial class SeedDataMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "UserImages",
                columns: new[] { "ImageId", "ImageName", "ImagePath" },
                values: new object[,]
                {
                    { 1, "image1", "imagesList/image1.jpg" },
                    { 2, "image2", "imagesList/image2.jpg" },
                    { 3, "image3", "imagesList/image3.jpg" },
                    { 4, "image4", "imagesList/image4.jpg" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserImages",
                keyColumn: "ImageId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "UserImages",
                keyColumn: "ImageId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "UserImages",
                keyColumn: "ImageId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserImages",
                keyColumn: "ImageId",
                keyValue: 4);
        }
    }
}
