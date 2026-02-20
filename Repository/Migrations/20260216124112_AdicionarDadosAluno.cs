using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDadosAluno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Alunos",
                type: "CHAR(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(14)");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Alunos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataNascimento",
                table: "Alunos",
                type: "date",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Alunos",
                type: "VARCHAR(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Matricula",
                table: "Alunos",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'000000'");

            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "Alunos",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'NaoInformado'");

            migrationBuilder.Sql("UPDATE Alunos SET Matricula = CAST(Id AS VARCHAR)");

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_Matricula",
                table: "Alunos",
                column: "Matricula",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alunos_Matricula",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "Alunos");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "Alunos");

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Alunos",
                type: "VARCHAR(14)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "CHAR(14)",
                oldMaxLength: 14);
        }
    }
}
