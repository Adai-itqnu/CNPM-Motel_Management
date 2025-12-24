using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_NhaTro_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingIdToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingId",
                table: "Contracts",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BookingId",
                table: "Contracts",
                column: "BookingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Bookings_BookingId",
                table: "Contracts",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Bookings_BookingId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_BookingId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Contracts");
        }
    }
}
