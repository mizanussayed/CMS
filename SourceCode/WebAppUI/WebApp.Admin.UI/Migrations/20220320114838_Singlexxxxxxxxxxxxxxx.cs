using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Admin.UI.Migrations
{
    public partial class Singlexxxxxxxxxxxxxxx : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "742bc632-7b1b-44f4-88ad-b7e7919e7f20");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e1ae1f42-75b2-4604-97ec-10f844b1962f",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "127c6b68-5fa8-4fc1-8d52-dd9be283f64c", "AQAAAAEAACcQAAAAEDspiW54qalBgrk5DrHiYh0y2DREHcPC1mHNGca9In0DbMkYYYT9owDb7tCiFmmLdw==", "b5984a7d-65cc-4127-bd78-660f3273bdc3" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "43d73416-11cf-4675-a082-902e3a670f98");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e1ae1f42-75b2-4604-97ec-10f844b1962f",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3bd8310a-d0ac-48f8-82a0-394c3456618c", "AQAAAAEAACcQAAAAEN1E2cnuDeKdJnbtEaw5nD8h1dJ83CaYUGzn68JvE3ksKWltIbsMQAZ9Jzsc8zTwFw==", "ea3a8117-d825-4093-b225-083cd34c3653" });
        }
    }
}
