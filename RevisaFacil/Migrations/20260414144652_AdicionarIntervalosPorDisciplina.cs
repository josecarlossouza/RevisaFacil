using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RevisaFacil.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIntervalosPorDisciplina : Migration
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
                    QuantidadeRevisoes = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimaDisciplinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo3 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo4 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo5 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo6 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo7 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo8 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo9 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo10 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo11 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo12 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo13 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo14 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo15 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo16 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo17 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo18 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo19 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo20 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo21 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo22 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo23 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo24 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo25 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo26 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo27 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo28 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo29 = table.Column<int>(type: "INTEGER", nullable: false),
                    Intervalo30 = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    QuantidadeRevisoes = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo1 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo2 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo3 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo4 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo5 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo6 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo7 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo8 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo9 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo10 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo11 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo12 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo13 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo14 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo15 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo16 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo17 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo18 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo19 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo20 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo21 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo22 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo23 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo24 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo25 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo26 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo27 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo28 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo29 = table.Column<int>(type: "INTEGER", nullable: true),
                    Intervalo30 = table.Column<int>(type: "INTEGER", nullable: true)
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
                    Int6 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int7 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int8 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int9 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int10 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int11 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int12 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int13 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int14 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int15 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int16 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int17 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int18 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int19 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int20 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int21 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int22 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int23 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int24 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int25 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int26 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int27 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int28 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int29 = table.Column<int>(type: "INTEGER", nullable: false),
                    Int30 = table.Column<int>(type: "INTEGER", nullable: false),
                    Rev1Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev2Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev3Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev4Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev5Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev6Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev7Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev8Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev9Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev10Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev11Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev12Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev13Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev14Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev15Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev16Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev17Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev18Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev19Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev20Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev21Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev22Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev23Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev24Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev25Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev26Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev27Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev28Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev29Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rev30Concluida = table.Column<bool>(type: "INTEGER", nullable: false)
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
