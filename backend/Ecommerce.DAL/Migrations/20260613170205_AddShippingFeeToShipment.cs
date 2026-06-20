using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingFeeToShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlatormCommission",
                table: "orders",
                newName: "PlatformCommission");

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "shipments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "shipments");

            migrationBuilder.RenameColumn(
                name: "PlatformCommission",
                table: "orders",
                newName: "PlatormCommission");
        }
    }
}
