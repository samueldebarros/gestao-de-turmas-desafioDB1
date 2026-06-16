using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaConstraintsERemoveDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Sexo",
                table: "Alunos",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'NaoInformado'");

            migrationBuilder.AlterColumn<string>(
                name: "Matricula",
                table: "Alunos",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'000000'");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataNascimento",
                table: "Alunos",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Turmas_Capacidade",
                table: "Turmas",
                sql: "[Capacidade] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Turmas_Serie",
                table: "Turmas",
                sql: "[Serie] IN (1,2,3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Turmas_Turno",
                table: "Turmas",
                sql: "[Turno] IN (1,2,3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enturmamentos_Situacao",
                table: "Enturmamentos",
                sql: "[Situacao] IN (1,2,3,4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Docentes_Cpf_Numerico",
                table: "Docentes",
                sql: "[Cpf] NOT LIKE '%[^0-9]%'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Disciplinas_CargaHoraria",
                table: "Disciplinas",
                sql: "[CargaHoraria] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Alunos_Cpf_Numerico",
                table: "Alunos",
                sql: "[Cpf] NOT LIKE '%[^0-9]%'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Alunos_Sexo",
                table: "Alunos",
                sql: "[Sexo] IN ('Masculino','Feminino','Outro')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Turmas_Capacidade",
                table: "Turmas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Turmas_Serie",
                table: "Turmas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Turmas_Turno",
                table: "Turmas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enturmamentos_Situacao",
                table: "Enturmamentos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Docentes_Cpf_Numerico",
                table: "Docentes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Disciplinas_CargaHoraria",
                table: "Disciplinas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Alunos_Cpf_Numerico",
                table: "Alunos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Alunos_Sexo",
                table: "Alunos");

            migrationBuilder.AlterColumn<string>(
                name: "Sexo",
                table: "Alunos",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'NaoInformado'",
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Matricula",
                table: "Alunos",
                type: "VARCHAR(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'000000'",
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DataNascimento",
                table: "Alunos",
                type: "date",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
