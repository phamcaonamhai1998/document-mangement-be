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
                name: "FK_DocumentProcedureSteps_Documents_DocumentId",
                table: "DocumentProcedureSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcedureStepId",
                table: "DocumentProcedureSteps",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "DocumentProcedureSteps",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProcedureSteps_Documents_DocumentId",
                table: "DocumentProcedureSteps",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps",
                column: "ProcedureStepId",
                principalTable: "ProcedureSteps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentProcedureSteps_Documents_DocumentId",
                table: "DocumentProcedureSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcedureStepId",
                table: "DocumentProcedureSteps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "DocumentProcedureSteps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProcedureSteps_Documents_DocumentId",
                table: "DocumentProcedureSteps",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentProcedureSteps_ProcedureSteps_ProcedureStepId",
                table: "DocumentProcedureSteps",
                column: "ProcedureStepId",
                principalTable: "ProcedureSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
