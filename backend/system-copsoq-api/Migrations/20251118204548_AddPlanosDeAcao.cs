using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace system_copsoq_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanosDeAcao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanosDeAcao",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAtivo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosDeAcao", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlanosDeAcao_Empresas_EmpresaID",
                        column: x => x.EmpresaID,
                        principalTable: "Empresas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Acoes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prazo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanoDeAcaoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acoes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Acoes_PlanosDeAcao_PlanoDeAcaoID",
                        column: x => x.PlanoDeAcaoID,
                        principalTable: "PlanosDeAcao",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acoes_PlanoDeAcaoID",
                table: "Acoes",
                column: "PlanoDeAcaoID");

            migrationBuilder.CreateIndex(
                name: "IX_PlanosDeAcao_EmpresaID",
                table: "PlanosDeAcao",
                column: "EmpresaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acoes");

            migrationBuilder.DropTable(
                name: "PlanosDeAcao");
        }
    }
}
