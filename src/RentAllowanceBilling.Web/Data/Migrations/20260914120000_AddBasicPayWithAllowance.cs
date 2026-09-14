using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAllowanceBilling.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicPayWithAllowance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BasicPayWithAllowances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinBasicPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxBasicPay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AllowanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasicPayWithAllowances", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BasicPayWithAllowances");
        }
    }
}
