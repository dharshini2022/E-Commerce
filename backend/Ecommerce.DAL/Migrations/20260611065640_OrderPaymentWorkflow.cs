using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.DAL.Migrations
{
    /// <inheritdoc />
    public partial class OrderPaymentWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vendor_settlements_orders_OrderId",
                table: "vendor_settlements");

            migrationBuilder.DropForeignKey(
                name: "FK_vendor_settlements_vendors_VendorId",
                table: "vendor_settlements");

            migrationBuilder.DropIndex(
                name: "IX_vendor_settlements_VendorId",
                table: "vendor_settlements");

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingAmount",
                table: "vendor_settlements",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "vendor_settlements",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    VariantId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsReleased = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_reservations_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_reservations_product_variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "product_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vendor_settlements_VendorId_OrderId",
                table: "vendor_settlements",
                columns: new[] { "VendorId", "OrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_StripePaymentIntentId",
                table: "orders",
                column: "StripePaymentIntentId",
                unique: true,
                filter: "\"StripePaymentIntentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_IsReleased",
                table: "stock_reservations",
                column: "IsReleased");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_OrderId_VariantId",
                table: "stock_reservations",
                columns: new[] { "OrderId", "VariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_VariantId",
                table: "stock_reservations",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_vendor_settlements_orders_OrderId",
                table: "vendor_settlements",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vendor_settlements_vendors_VendorId",
                table: "vendor_settlements",
                column: "VendorId",
                principalTable: "vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vendor_settlements_orders_OrderId",
                table: "vendor_settlements");

            migrationBuilder.DropForeignKey(
                name: "FK_vendor_settlements_vendors_VendorId",
                table: "vendor_settlements");

            migrationBuilder.DropTable(
                name: "stock_reservations");

            migrationBuilder.DropIndex(
                name: "IX_vendor_settlements_VendorId_OrderId",
                table: "vendor_settlements");

            migrationBuilder.DropIndex(
                name: "IX_orders_StripePaymentIntentId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ShippingAmount",
                table: "vendor_settlements");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "vendor_settlements");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "orders");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "IX_vendor_settlements_VendorId",
                table: "vendor_settlements",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_vendor_settlements_orders_OrderId",
                table: "vendor_settlements",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vendor_settlements_vendors_VendorId",
                table: "vendor_settlements",
                column: "VendorId",
                principalTable: "vendors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
