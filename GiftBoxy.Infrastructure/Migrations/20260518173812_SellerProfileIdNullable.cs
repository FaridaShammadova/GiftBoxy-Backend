using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftBoxy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SellerProfileIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_SellerProfiles_SellerProfileId",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "SellerProfileId",
                table: "Products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SellerProfiles_SellerProfileId",
                table: "Products",
                column: "SellerProfileId",
                principalTable: "SellerProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_SellerProfiles_SellerProfileId",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "SellerProfileId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SellerProfiles_SellerProfileId",
                table: "Products",
                column: "SellerProfileId",
                principalTable: "SellerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
