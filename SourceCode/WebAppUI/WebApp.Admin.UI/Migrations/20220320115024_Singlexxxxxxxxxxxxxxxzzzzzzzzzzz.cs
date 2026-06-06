using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Admin.UI.Migrations
{
    public partial class Singlexxxxxxxxxxxxxxxzzzzzzzzzzz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                column: "ConcurrencyStamp",
                value: "1f9a4ff5-d94e-4646-b65a-64b862e8f73c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e1ae1f42-75b2-4604-97ec-10f844b1962f",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1aeecc82-0ff9-43db-b1ae-2752aad7a09a", "AQAAAAEAACcQAAAAEHatDID7EfqB8eydxNXlylSsXC6hqGxuJK3YZhs6aAyuefkBomECnP46cjmVXmOJvA==", "98b72351-27e6-4ca5-9ec0-15fff43f3478" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
