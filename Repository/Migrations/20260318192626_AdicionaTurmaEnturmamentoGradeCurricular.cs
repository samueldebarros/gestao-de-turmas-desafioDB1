using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTurmaEnturmamentoGradeCurricular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Turmas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identificador = table.Column<string>(type: "CHAR(1)", maxLength: 1, nullable: false),
                    AnoLetivo = table.Column<short>(type: "SMALLINT", nullable: false),
                    Serie = table.Column<byte>(type: "TINYINT", nullable: false),
                    VagasMaximas = table.Column<byte>(type: "TINYINT", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turmas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enturmamentos",
                columns: table => new
                {
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    TurmaId = table.Column<int>(type: "int", nullable: false),
                    DataEnturmamento = table.Column<DateTime>(type: "DATE", nullable: false),
                    Situacao = table.Column<byte>(type: "TINYINT", nullable: false, defaultValue: (byte)1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enturmamentos", x => new { x.AlunoId, x.TurmaId });
                    table.ForeignKey(
                        name: "FK_Enturmamentos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enturmamentos_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GradeCurricular",
                columns: table => new
                {
                    TurmaId = table.Column<int>(type: "int", nullable: false),
                    DisciplinaId = table.Column<int>(type: "int", nullable: false),
                    DocenteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeCurricular", x => new { x.TurmaId, x.DisciplinaId });
                    table.ForeignKey(
                        name: "FK_GradeCurricular_Disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "Disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradeCurricular_Docentes_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Docentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradeCurricular_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enturmamentos_TurmaId",
                table: "Enturmamentos",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeCurricular_DisciplinaId",
                table: "GradeCurricular",
                column: "DisciplinaId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeCurricular_DocenteId",
                table: "GradeCurricular",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_Identificador_AnoLetivo_Serie",
                table: "Turmas",
                columns: new[] { "Identificador", "AnoLetivo", "Serie" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enturmamentos");

            migrationBuilder.DropTable(
                name: "GradeCurricular");

            migrationBuilder.DropTable(
                name: "Turmas");
        }
    }
}
