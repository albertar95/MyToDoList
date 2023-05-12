using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoListWebApp.Migrations
{
    public partial class Initialize_sqliteDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    NidUser = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProfilePic = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDisabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.NidUser);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    NidGoal = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    GoalStatus = table.Column<byte>(type: "INTEGER", nullable: false),
                    FromDate = table.Column<DateTime>(type: "date", nullable: false),
                    ToDate = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.NidGoal);
                    table.ForeignKey(
                        name: "FK_Goals_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "NidUser");
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    NidTask = table.Column<Guid>(type: "TEXT", nullable: false),
                    GoalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TaskStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    TaskWeight = table.Column<byte>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ClosureDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.NidTask);
                    table.ForeignKey(
                        name: "FK_Goals_Tasks",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "NidGoal");
                    table.ForeignKey(
                        name: "FK_Tasks_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "NidUser");
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    NidSchedule = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.NidSchedule);
                    table.ForeignKey(
                        name: "FK_Schedules_Tasks",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "NidTask");
                });

            migrationBuilder.CreateTable(
                name: "Progresses",
                columns: table => new
                {
                    NidProgress = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProgressTime = table.Column<short>(type: "INTEGER", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progresses", x => x.NidProgress);
                    table.ForeignKey(
                        name: "FK_Progresses_Schedules",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "NidSchedule");
                    table.ForeignKey(
                        name: "FK_Progresses_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "NidUser");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId",
                table: "Goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_ScheduleId",
                table: "Progresses",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_UserId",
                table: "Progresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_TaskId",
                table: "Schedules",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_GoalId",
                table: "Tasks",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId",
                table: "Tasks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Progresses");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
