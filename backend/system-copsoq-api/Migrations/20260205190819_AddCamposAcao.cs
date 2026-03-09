using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace system_copsoq_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposAcao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataConclusao",
                table: "Acoes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justificativa",
                table: "Acoes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataConclusao",
                table: "Acoes");

            migrationBuilder.DropColumn(
                name: "Justificativa",
                table: "Acoes");
        }
    }
}
