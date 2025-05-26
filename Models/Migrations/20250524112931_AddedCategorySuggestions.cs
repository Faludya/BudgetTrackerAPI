using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class AddedCategorySuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategorySuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImportedTransactionId = table.Column<int>(type: "integer", nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric", nullable: false),
                    IsFromMLModel = table.Column<bool>(type: "boolean", nullable: false),
                    SourceKeyword = table.Column<string>(type: "text", nullable: true),
                    SuggestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorySuggestions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategorySuggestions_ImportedTransactions_ImportedTransactio~",
                        column: x => x.ImportedTransactionId,
                        principalTable: "ImportedTransactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategorySuggestions_CategoryId",
                table: "CategorySuggestions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySuggestions_ImportedTransactionId",
                table: "CategorySuggestions",
                column: "ImportedTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorySuggestions");
        }
    }
}
