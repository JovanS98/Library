using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotekaWeb.Migrations.ApplicationDb
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReturnDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActiveReservation = table.Column<bool>(type: "bit", nullable: false),
                    PendingReservation = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfAvailableCopies = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => new { x.UserId, x.BookId, x.Time });
                });
  
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "Processes");

        }
    }
}
