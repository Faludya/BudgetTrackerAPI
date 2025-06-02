using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class improvedCateogoryColorOrderIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconName",
                table: "Categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "Categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserBudgetItems_CategoryId",
                table: "UserBudgetItems",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBudgetItems_Categories_CategoryId",
                table: "UserBudgetItems",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBudgetItems_Categories_CategoryId",
                table: "UserBudgetItems");

            migrationBuilder.DropIndex(
                name: "IX_UserBudgetItems_CategoryId",
                table: "UserBudgetItems");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IconName",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "Categories");
        }
    }
}
