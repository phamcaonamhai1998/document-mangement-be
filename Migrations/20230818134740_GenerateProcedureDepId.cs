using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class GenerateProcedureDepId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcedureSteps_Procedures_ProcedureId",
                table: "ProcedureSteps");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcedureId",
                table: "ProcedureSteps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentId",
                table: "Procedures",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcedureSteps_Procedures_ProcedureId",
                table: "ProcedureSteps",
                column: "ProcedureId",
                principalTable: "Procedures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcedureSteps_Procedures_ProcedureId",
                table: "ProcedureSteps");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Procedures");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcedureId",
                table: "ProcedureSteps",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcedureSteps_Procedures_ProcedureId",
                table: "ProcedureSteps",
                column: "ProcedureId",
                principalTable: "Procedures",
                principalColumn: "Id");
        }
    }
}
