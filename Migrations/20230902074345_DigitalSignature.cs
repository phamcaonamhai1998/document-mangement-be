using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class DigitalSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocSignedPath",
                table: "DocumentProcedureSteps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSigned",
                table: "DocumentProcedureSteps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CertFolderId",
                table: "Accounts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocSignedPath",
                table: "DocumentProcedureSteps");

            migrationBuilder.DropColumn(
                name: "IsSigned",
                table: "DocumentProcedureSteps");

            migrationBuilder.DropColumn(
                name: "CertFolderId",
                table: "Accounts");
        }
    }
}
