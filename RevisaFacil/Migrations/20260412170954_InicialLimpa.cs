using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RevisaFacil.Migrations
{
    /// <inheritdoc />
    public partial class InicialLimpa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Intervalo1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo3 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo4 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo5 = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disciplinas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplinas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotasCalendario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Conteudo = table.Column<string>(type: "TEXT", nullable: false),
                    AssuntoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasCalendario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    DisciplinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDestacado = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Int1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int3 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int4 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int5 = table.Column<int>(type: "INTEGER", nullable: false),
                    Rev1Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev2Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev3Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev4Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev5Concluida = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assuntos_Disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "Disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assuntos_DisciplinaId",
                table: "Assuntos",
                column: "DisciplinaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assuntos");

            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropTable(
                name: "NotasCalendario");

            migrationBuilder.DropTable(
                name: "Disciplinas");
        }
    }
}
