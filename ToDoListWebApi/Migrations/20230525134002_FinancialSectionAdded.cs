using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoListWebApi.Migrations
{
    public partial class FinancialSectionAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    NidAccount = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 0)", nullable: false),
                    LendAmount = table.Column<decimal>(type: "decimal(18, 0)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.NidAccount);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    NidTransaction = table.Column<Guid>(type: "TEXT", nullable: false),
                    TransactionType = table.Column<byte>(type: "INTEGER", nullable: false),
                    PayerAccount = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecieverAccount = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18, 0)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    TransactionReason = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.NidTransaction);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
