using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace system_copsoq_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDisparoAndRespostaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Disparos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionarioID = table.Column<int>(type: "int", nullable: false),
                    FuncionarioID = table.Column<int>(type: "int", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenAcesso = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Respondido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disparos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Disparos_Funcionarios_FuncionarioID",
                        column: x => x.FuncionarioID,
                        principalTable: "Funcionarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Disparos_Questionarios_QuestionarioID",
                        column: x => x.QuestionarioID,
                        principalTable: "Questionarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespostasFuncionarios",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisparoID = table.Column<int>(type: "int", nullable: false),
                    PerguntaID = table.Column<int>(type: "int", nullable: false),
                    ValorResposta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasFuncionarios", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RespostasFuncionarios_Disparos_DisparoID",
                        column: x => x.DisparoID,
                        principalTable: "Disparos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespostasFuncionarios_Perguntas_PerguntaID",
                        column: x => x.PerguntaID,
                        principalTable: "Perguntas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Disparos_FuncionarioID",
                table: "Disparos",
                column: "FuncionarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Disparos_QuestionarioID",
                table: "Disparos",
                column: "QuestionarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Disparos_TokenAcesso",
                table: "Disparos",
                column: "TokenAcesso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RespostasFuncionarios_DisparoID",
                table: "RespostasFuncionarios",
                column: "DisparoID");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasFuncionarios_PerguntaID",
                table: "RespostasFuncionarios",
                column: "PerguntaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RespostasFuncionarios");

            migrationBuilder.DropTable(
                name: "Disparos");
        }
    }
}
