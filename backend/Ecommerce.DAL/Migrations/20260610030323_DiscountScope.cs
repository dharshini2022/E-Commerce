using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DiscountScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "wishlist_items");

            migrationBuilder.DropColumn(
                name: "isInStock",
                table: "cart_items");

            migrationBuilder.RenameColumn(
                name: "MinOrderValue",
                table: "discounts",
                newName: "MinOrder");

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "discounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_discounts_CategoryId",
                table: "discounts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_discounts_ProductId",
                table: "discounts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_discounts_categories_CategoryId",
                table: "discounts",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_discounts_products_ProductId",
                table: "discounts",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_discounts_categories_CategoryId",
                table: "discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_discounts_products_ProductId",
                table: "discounts");

            migrationBuilder.DropIndex(
                name: "IX_discounts_CategoryId",
                table: "discounts");

            migrationBuilder.DropIndex(
                name: "IX_discounts_ProductId",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "discounts");

            migrationBuilder.RenameColumn(
                name: "MinOrder",
                table: "discounts",
                newName: "MinOrderValue");

            migrationBuilder.AddColumn<int>(
                name: "ProductVariantId",
                table: "wishlist_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isInStock",
                table: "cart_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
