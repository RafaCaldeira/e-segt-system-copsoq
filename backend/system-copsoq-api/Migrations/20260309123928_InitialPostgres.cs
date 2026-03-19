using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace system_copsoq_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeEmpresa = table.Column<string>(type: "text", nullable: false),
                    NomeResponsavel = table.Column<string>(type: "text", nullable: false),
                    SetorAtuacao = table.Column<string>(type: "text", nullable: false),
                    Cidade = table.Column<string>(type: "text", nullable: false),
                    Cnpj = table.Column<string>(type: "text", nullable: false),
                    IsAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Questionarios",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    TextoIntroducao = table.Column<string>(type: "text", nullable: false),
                    TextoConsentimento = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionarios", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: false),
                    Cargo = table.Column<string>(type: "text", nullable: false),
                    Setor = table.Column<string>(type: "text", nullable: false),
                    CPF = table.Column<string>(type: "text", nullable: false),
                    EmpresaID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Funcionarios_Empresas_EmpresaID",
                        column: x => x.EmpresaID,
                        principalTable: "Empresas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanosDeAcao",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaID = table.Column<int>(type: "integer", nullable: false)
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
                name: "Usuarios",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    EmpresaID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Usuarios_Empresas_EmpresaID",
                        column: x => x.EmpresaID,
                        principalTable: "Empresas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Dimensoes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    NomeIndicador = table.Column<string>(type: "text", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    QuestionarioID = table.Column<int>(type: "integer", nullable: false)
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
                name: "OpcoesResposta",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    Valor = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    QuestionarioID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcoesResposta", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OpcoesResposta_Questionarios_QuestionarioID",
                        column: x => x.QuestionarioID,
                        principalTable: "Questionarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionarioSetoresAplicaveis",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionarioID = table.Column<int>(type: "integer", nullable: false),
                    Setor = table.Column<string>(type: "text", nullable: false)
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
                name: "Disparos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionarioID = table.Column<int>(type: "integer", nullable: false),
                    FuncionarioID = table.Column<int>(type: "integer", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TokenAcesso = table.Column<Guid>(type: "uuid", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Respondido = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "Acoes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Prazo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Justificativa = table.Column<string>(type: "text", nullable: true),
                    PlanoDeAcaoID = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Perguntas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    QuestionarioID = table.Column<int>(type: "integer", nullable: false),
                    DimensaoID = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "RespostasFuncionarios",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisparoID = table.Column<int>(type: "integer", nullable: false),
                    PerguntaID = table.Column<int>(type: "integer", nullable: false),
                    ValorResposta = table.Column<int>(type: "integer", nullable: true),
                    TextoResposta = table.Column<string>(type: "text", nullable: true)
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
                name: "IX_Acoes_PlanoDeAcaoID",
                table: "Acoes",
                column: "PlanoDeAcaoID");

            migrationBuilder.CreateIndex(
                name: "IX_Dimensoes_QuestionarioID",
                table: "Dimensoes",
                column: "QuestionarioID");

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
                name: "IX_Funcionarios_EmpresaID",
                table: "Funcionarios",
                column: "EmpresaID");

            migrationBuilder.CreateIndex(
                name: "IX_OpcoesResposta_QuestionarioID",
                table: "OpcoesResposta",
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
                name: "IX_PlanosDeAcao_EmpresaID",
                table: "PlanosDeAcao",
                column: "EmpresaID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionarioSetoresAplicaveis_QuestionarioID",
                table: "QuestionarioSetoresAplicaveis",
                column: "QuestionarioID");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasFuncionarios_DisparoID",
                table: "RespostasFuncionarios",
                column: "DisparoID");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasFuncionarios_PerguntaID",
                table: "RespostasFuncionarios",
                column: "PerguntaID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaID",
                table: "Usuarios",
                column: "EmpresaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acoes");

            migrationBuilder.DropTable(
                name: "OpcoesResposta");

            migrationBuilder.DropTable(
                name: "QuestionarioSetoresAplicaveis");

            migrationBuilder.DropTable(
                name: "RespostasFuncionarios");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "PlanosDeAcao");

            migrationBuilder.DropTable(
                name: "Disparos");

            migrationBuilder.DropTable(
                name: "Perguntas");

            migrationBuilder.DropTable(
                name: "Funcionarios");

            migrationBuilder.DropTable(
                name: "Dimensoes");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "Questionarios");
        }
    }
}
