using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoListWebApi.Migrations
{
    public partial class Shield_Table_Added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TargetUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shields_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "NidUser");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shields_UserId",
                table: "Shields",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shields");
        }
    }
}
