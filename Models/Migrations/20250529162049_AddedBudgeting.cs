using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class AddedBudgeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBudgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Month = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBudgets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BudgetTemplateId = table.Column<int>(type: "integer", nullable: false),
                    CategoryType = table.Column<string>(type: "text", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetTemplateItems_BudgetTemplates_BudgetTemplateId",
                        column: x => x.BudgetTemplateId,
                        principalTable: "BudgetTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBudgetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserBudgetId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    CategoryType = table.Column<string>(type: "text", nullable: true),
                    Limit = table.Column<decimal>(type: "numeric", nullable: false),
                    PredictedSpending = table.Column<decimal>(type: "numeric", nullable: true),
                    TrendDirection = table.Column<string>(type: "text", nullable: true),
                    IsAIRecommended = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBudgetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBudgetItems_UserBudgets_UserBudgetId",
                        column: x => x.UserBudgetId,
                        principalTable: "UserBudgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetTemplateItems_BudgetTemplateId",
                table: "BudgetTemplateItems",
                column: "BudgetTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBudgetItems_UserBudgetId",
                table: "UserBudgetItems",
                column: "UserBudgetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetTemplateItems");

            migrationBuilder.DropTable(
                name: "UserBudgetItems");

            migrationBuilder.DropTable(
                name: "BudgetTemplates");

            migrationBuilder.DropTable(
                name: "UserBudgets");
        }
    }
}
