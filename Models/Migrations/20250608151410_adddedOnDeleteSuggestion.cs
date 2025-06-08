using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class adddedOnDeleteSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySuggestions_ImportedTransactions_ImportedTransactio~",
                table: "CategorySuggestions");

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySuggestions_ImportedTransactions_ImportedTransactio~",
                table: "CategorySuggestions",
                column: "ImportedTransactionId",
                principalTable: "ImportedTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategorySuggestions_ImportedTransactions_ImportedTransactio~",
                table: "CategorySuggestions");

            migrationBuilder.AddForeignKey(
                name: "FK_CategorySuggestions_ImportedTransactions_ImportedTransactio~",
                table: "CategorySuggestions",
                column: "ImportedTransactionId",
                principalTable: "ImportedTransactions",
                principalColumn: "Id");
        }
    }
}
