using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CurrencyRatesAPI.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyRates",
                columns: table => new
                {
                    CuerrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyRates", x => new { x.CuerrencyCode, x.Date });
                });

            migrationBuilder.InsertData(
                table: "DailyRates",
                columns: new[] { "CuerrencyCode", "Date", "Rate" },
                values: new object[] { "PLN", new DateTime(2021, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.111 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyRates");
        }
    }
}
