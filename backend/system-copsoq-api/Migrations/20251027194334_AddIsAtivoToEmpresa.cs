using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace system_copsoq_api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAtivoToEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAtivo",
                table: "Empresas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAtivo",
                table: "Empresas");
        }
    }
}
