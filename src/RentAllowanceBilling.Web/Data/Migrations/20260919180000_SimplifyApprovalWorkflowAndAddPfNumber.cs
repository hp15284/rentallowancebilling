using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAllowanceBilling.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyApprovalWorkflowAndAddPfNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountantActionAt",
                table: "RentAllowanceBills");

            migrationBuilder.DropColumn(
                name: "AccountantRemark",
                table: "RentAllowanceBills");

            migrationBuilder.DropColumn(
                name: "AccountantUserId",
                table: "RentAllowanceBills");

            migrationBuilder.DropColumn(
                name: "ApprovingAuthorityActionAt",
                table: "RentAllowanceBills");

            migrationBuilder.DropColumn(
                name: "ApprovingAuthorityRemark",
                table: "RentAllowanceBills");

            migrationBuilder.DropColumn(
                name: "ApprovingAuthorityUserId",
                table: "RentAllowanceBills");

            migrationBuilder.DropColumn(
                name: "VerifiedAmount",
                table: "RentAllowanceBills");

            migrationBuilder.AddColumn<string>(
                name: "PfNumber",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PfNumber",
                table: "Employees");

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountantActionAt",
                table: "RentAllowanceBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountantRemark",
                table: "RentAllowanceBills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountantUserId",
                table: "RentAllowanceBills",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovingAuthorityActionAt",
                table: "RentAllowanceBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovingAuthorityRemark",
                table: "RentAllowanceBills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovingAuthorityUserId",
                table: "RentAllowanceBills",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VerifiedAmount",
                table: "RentAllowanceBills",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
