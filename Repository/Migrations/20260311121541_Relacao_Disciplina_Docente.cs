using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class Relacao_Disciplina_Docente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especialidade",
                table: "Docentes");

            migrationBuilder.AddColumn<int>(
                name: "DisciplinaId",
                table: "Docentes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Docentes_DisciplinaId",
                table: "Docentes",
                column: "DisciplinaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Docentes_Disciplinas_DisciplinaId",
                table: "Docentes",
                column: "DisciplinaId",
                principalTable: "Disciplinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Docentes_Disciplinas_DisciplinaId",
                table: "Docentes");

            migrationBuilder.DropIndex(
                name: "IX_Docentes_DisciplinaId",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "DisciplinaId",
                table: "Docentes");

            migrationBuilder.AddColumn<string>(
                name: "Especialidade",
                table: "Docentes",
                type: "VARCHAR(50)",
                nullable: false,
                defaultValue: "");
        }
    }
}
