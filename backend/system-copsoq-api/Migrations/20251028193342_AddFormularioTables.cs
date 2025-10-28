using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace system_copsoq_api.Migrations
{
    /// <inheritdoc />
    public partial class AddFormularioTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questionarios",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextoIntroducao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextoConsentimento = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionarios", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Dimensoes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomeIndicador = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    QuestionarioID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dimensoes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Dimensoes_Questionarios_QuestionarioID",
                        column: x => x.QuestionarioID,
                        principalTable: "Questionarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionarioSetoresAplicaveis",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionarioID = table.Column<int>(type: "int", nullable: false),
                    Setor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionarioSetoresAplicaveis", x => x.ID);
                    table.ForeignKey(
                        name: "FK_QuestionarioSetoresAplicaveis_Questionarios_QuestionarioID",
                        column: x => x.QuestionarioID,
                        principalTable: "Questionarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Perguntas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionarioID = table.Column<int>(type: "int", nullable: false),
                    DimensaoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perguntas", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Perguntas_Dimensoes_DimensaoID",
                        column: x => x.DimensaoID,
                        principalTable: "Dimensoes",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Perguntas_Questionarios_QuestionarioID",
                        column: x => x.QuestionarioID,
                        principalTable: "Questionarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dimensoes_QuestionarioID",
                table: "Dimensoes",
                column: "QuestionarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Perguntas_DimensaoID",
                table: "Perguntas",
                column: "DimensaoID");

            migrationBuilder.CreateIndex(
                name: "IX_Perguntas_QuestionarioID",
                table: "Perguntas",
                column: "QuestionarioID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionarioSetoresAplicaveis_QuestionarioID",
                table: "QuestionarioSetoresAplicaveis",
                column: "QuestionarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Perguntas");

            migrationBuilder.DropTable(
                name: "QuestionarioSetoresAplicaveis");

            migrationBuilder.DropTable(
                name: "Dimensoes");

            migrationBuilder.DropTable(
                name: "Questionarios");
        }
    }
}
