using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTurnoECapacidadeTurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VagasMaximas",
                table: "Turmas",
                newName: "Capacidade");

            migrationBuilder.AddColumn<byte>(
                name: "Turno",
                table: "Turmas",
                type: "TINYINT",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Turno",
                table: "Turmas");

            migrationBuilder.RenameColumn(
                name: "Capacidade",
                table: "Turmas",
                newName: "VagasMaximas");
        }
    }
}
