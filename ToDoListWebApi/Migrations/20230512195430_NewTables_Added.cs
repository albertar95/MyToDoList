using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoListWebApi.Migrations
{
    public partial class NewTables_Added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoteGroups",
                columns: table => new
                {
                    NidGroup = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteGroups", x => x.NidGroup);
                    table.ForeignKey(
                        name: "FK_NoteGroups_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "NidUser");
                });

            migrationBuilder.CreateTable(
                name: "Routines",
                columns: table => new
                {
                    NidRoutine = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    RepeatType = table.Column<byte>(type: "INTEGER", nullable: false),
                    RepeatDays = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    FromDate = table.Column<DateTime>(type: "date", nullable: false),
                    Todate = table.Column<DateTime>(type: "date", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routines", x => x.NidRoutine);
                    table.ForeignKey(
                        name: "FK_Routines_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "NidUser");
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    NidNote = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NoteContent = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.NidNote);
                    table.ForeignKey(
                        name: "FK_Notes_NoteGroups",
                        column: x => x.GroupId,
                        principalTable: "NoteGroups",
                        principalColumn: "NidGroup");
                });

            migrationBuilder.CreateTable(
                name: "RoutineProgresses",
                columns: table => new
                {
                    NidRoutineProgress = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoutineId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProgressDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineProgresses", x => x.NidRoutineProgress);
                    table.ForeignKey(
                        name: "FK_RoutineProgresses_Routines",
                        column: x => x.RoutineId,
                        principalTable: "Routines",
                        principalColumn: "NidRoutine");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoteGroups_UserId",
                table: "NoteGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_GroupId",
                table: "Notes",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineProgresses_RoutineId",
                table: "RoutineProgresses",
                column: "RoutineId");

            migrationBuilder.CreateIndex(
                name: "IX_Routines_UserId",
                table: "Routines",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "RoutineProgresses");

            migrationBuilder.DropTable(
                name: "NoteGroups");

            migrationBuilder.DropTable(
                name: "Routines");
        }
    }
}
