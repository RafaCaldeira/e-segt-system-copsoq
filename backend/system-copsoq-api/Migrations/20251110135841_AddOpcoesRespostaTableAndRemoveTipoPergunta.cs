using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace system_copsoq_api.Migrations
{
    /// <inheritdoc />
    public partial class AddOpcoesRespostaTableAndRemoveTipoPergunta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Perguntas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Perguntas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
