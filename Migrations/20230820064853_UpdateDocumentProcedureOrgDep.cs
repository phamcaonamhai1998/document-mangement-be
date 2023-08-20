using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentProcedureOrgDep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps");

            migrationBuilder.AddColumn<string>(
                name: "OrgDriveFolderId",
                table: "Organizations",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcedureStepId",
                table: "DocumentProcedureSteps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentDriveFolderId",
                table: "Departments",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps",
                column: "ProcedureStepId",
                principalTable: "ProcedureSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps");

            migrationBuilder.DropColumn(
                name: "OrgDriveFolderId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DepartmentDriveFolderId",
                table: "Departments");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcedureStepId",
                table: "DocumentProcedureSteps",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps",
                column: "ProcedureStepId",
                principalTable: "ProcedureSteps",
                principalColumn: "Id");
        }
    }
}
