// ExcelService.cs — v1.3.0
// ALTERAÇÕES:
// 1. EscreverLinha(): células de revisões CONCLUÍDAS (RevNConcluida = true)
//    e linhas de assuntos DESTACADOS (IsDestacado = true) são pintadas em verde.
// 2. LerLinhas(): lê a cor de fundo das células e importa IsDestacado e status
//    de cada revisão (verde → concluída).
// 3. Importar(): aplica IsDestacado e RevNConcluida lidos do Excel.
//
// Formato exportado (inalterado):
//   Aba "Todos"        → Disciplina | Assunto | DataInicio | Rev1 | Rev2 | ... | RevN
//   Aba "<Disciplina>" → Assunto | DataInicio | Rev1 | Rev2 | ... | RevN
//
// Verde no Excel = XLColor.FromArgb(39, 174, 96)  (#27AE60)
// Verde claro na linha = XLColor.FromArgb(212, 239, 223) (#D4EFDF)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using RevisaFacil.Data;
using RevisaFacil.Helpers;
using RevisaFacil.Models;

namespace RevisaFacil.Services
{
    public static class ExcelService
    {
        // ── Cores ─────────────────────────────────────────────────────────────────
        private static readonly XLColor CorVerdeCelula = XLColor.FromArgb(39, 174, 96);   // botão verde = revisão concluída
        private static readonly XLColor CorVerdeLinhaDestacada = XLColor.FromArgb(212, 239, 223); // linha destacada (#D4EFDF)

        // ── Exportar ──────────────────────────────────────────────────────────────

        public static string Exportar(string caminhoDestino)
        {
            int qtdRev = TemaManager.GetQuantidadeRevisoes();

            using var db = new EstudoDbContext(TemaManager.GetDbPath());
            var assuntos = db.Assuntos
                             .Include(a => a.Disciplina)
                             .OrderBy(a => a.Disciplina.Nome)
                             .ThenBy(a => a.Titulo)
                             .ToList();

            using var wb = new XLWorkbook();

            // ── Aba "Todos" ───────────────────────────────────────────────────────
            var wsTodos = wb.Worksheets.Add("Todos");
            EscreverCabecalho(wsTodos, incluirDisciplina: true, qtdRev);

            int linha = 2;
            foreach (var a in assuntos)
            {
                EscreverLinha(wsTodos, linha, a, incluirDisciplina: true, qtdRev);
                linha++;
            }
            FormatarPlanilha(wsTodos, incluirDisciplina: true, qtdRev, linha - 1);

            // ── Uma aba por disciplina ────────────────────────────────────────────
            var porDisciplina = assuntos.GroupBy(a => a.Disciplina?.Nome ?? "Sem Disciplina");

            foreach (var grupo in porDisciplina)
            {
                string nomeAba = SanitizarNomeAba(grupo.Key);
                var ws = wb.Worksheets.Add(nomeAba);
                EscreverCabecalho(ws, incluirDisciplina: false, qtdRev);

                int l = 2;
                foreach (var a in grupo)
                {
                    EscreverLinha(ws, l, a, incluirDisciplina: false, qtdRev);
                    l++;
                }
                FormatarPlanilha(ws, incluirDisciplina: false, qtdRev, l - 1);
            }

            wb.SaveAs(caminhoDestino);
            return caminhoDestino;
        }

        // ── Importar ──────────────────────────────────────────────────────────────

        public class ResultadoImportacao
        {
            public bool Sucesso { get; set; }
            public string Mensagem { get; set; }
            public int AssuntosNovos { get; set; }
            public int AssuntosAtualizados { get; set; }
            public int DisciplinasNovas { get; set; }
            public bool TemaNovosCriado { get; set; }
        }

        public static ResultadoImportacao Importar(string caminhoArquivo)
        {
            var resultado = new ResultadoImportacao();

            try
            {
                using var wb = new XLWorkbook(caminhoArquivo);

                if (!wb.Worksheets.Contains("Todos"))
                    return Erro("Formato inválido: aba 'Todos' não encontrada.");

                var ws = wb.Worksheet("Todos");

                var (qtdRevArquivo, erroHeader) = LerCabecalho(ws);
                if (erroHeader != null) return Erro(erroHeader);

                if (qtdRevArquivo < 1 || qtdRevArquivo > 30)
                    return Erro($"Formato inválido: quantidade de revisões ({qtdRevArquivo}) fora do intervalo 1-30.");

                string nomeTema = Path.GetFileNameWithoutExtension(caminhoArquivo);

                var linhas = LerLinhas(ws, qtdRevArquivo);
                if (linhas == null)
                    return Erro("Formato inválido: erro ao ler os dados da aba 'Todos'.");

                bool temaExistia = TemaManager.ListarTemas().Contains(nomeTema);

                if (!temaExistia)
                {
                    TemaManager.TemaAtual = nomeTema;
                    using var dbNovo = new EstudoDbContext(TemaManager.GetDbPath());
                    dbNovo.Database.EnsureCreated();
                    TemaManager.MigrarConfiguracoes();
                    TemaManager.MigrarConfiguracoesDisciplinas();
                    resultado.TemaNovosCriado = true;
                }
                else
                {
                    TemaManager.TemaAtual = nomeTema;
                    TemaManager.MigrarConfiguracoes();
                    TemaManager.MigrarConfiguracoesDisciplinas();
                }

                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    var config = db.Configuracoes.FirstOrDefault();
                    if (config == null)
                    {
                        config = new Configuracao { QuantidadeRevisoes = qtdRevArquivo };
                        db.Configuracoes.Add(config);
                    }
                    else
                    {
                        config.QuantidadeRevisoes = qtdRevArquivo;
                    }
                    db.SaveChanges();

                    foreach (var row in linhas)
                    {
                        // Disciplina
                        var disc = db.Disciplinas
                            .FirstOrDefault(d => d.Nome.ToLower() == row.NomeDisciplina.ToLower());

                        if (disc == null)
                        {
                            disc = new Disciplina { Nome = row.NomeDisciplina };
                            db.Disciplinas.Add(disc);
                            db.SaveChanges();
                            resultado.DisciplinasNovas++;
                        }

                        // Assunto
                        var assunto = db.Assuntos
                            .FirstOrDefault(a =>
                                a.DisciplinaId == disc.Id &&
                                a.Titulo.ToLower() == row.Titulo.ToLower());

                        if (assunto == null)
                        {
                            assunto = new Assunto
                            {
                                Titulo = row.Titulo,
                                DisciplinaId = disc.Id,
                                DataInicio = row.DataInicio,
                                IsDestacado = row.IsDestacado
                            };
                            AplicarIntervalos(assunto, row.Intervalos, qtdRevArquivo);
                            // Aplica status de revisões lido do Excel (cor verde)
                            for (int i = 0; i < qtdRevArquivo; i++)
                                assunto.SetRevConcluida(i + 1, row.RevisoesConcluidas[i]);

                            db.Assuntos.Add(assunto);
                            resultado.AssuntosNovos++;
                        }
                        else
                        {
                            bool mudou = false;

                            if (assunto.DataInicio.Date != row.DataInicio.Date)
                            {
                                assunto.DataInicio = row.DataInicio;
                                mudou = true;
                            }

                            if (assunto.IsDestacado != row.IsDestacado)
                            {
                                assunto.IsDestacado = row.IsDestacado;
                                mudou = true;
                            }

                            for (int i = 0; i < qtdRevArquivo; i++)
                            {
                                int intervaloAtual = ObterIntervalo(assunto, i + 1);
                                int intervaloNovo = row.Intervalos[i];
                                if (intervaloAtual != intervaloNovo)
                                {
                                    DefinirIntervalo(assunto, i + 1, intervaloNovo);
                                    mudou = true;
                                }

                                // Atualiza status de revisão se diferente
                                if (assunto.GetRevConcluida(i + 1) != row.RevisoesConcluidas[i])
                                {
                                    assunto.SetRevConcluida(i + 1, row.RevisoesConcluidas[i]);
                                    mudou = true;
                                }
                            }

                            if (mudou) resultado.AssuntosAtualizados++;
                        }
                    }

                    db.SaveChanges();
                }

                TemaManager.SincronizarCalendarioGlobal();

                resultado.Sucesso = true;
                resultado.Mensagem = temaExistia
                    ? $"Tema '{nomeTema}' atualizado com sucesso."
                    : $"Tema '{nomeTema}' criado com sucesso.";
            }
            catch (Exception ex)
            {
                resultado.Sucesso = false;
                resultado.Mensagem = $"Erro ao importar: {ex.Message}";
            }

            return resultado;
        }

        // ── Helpers de escrita ────────────────────────────────────────────────────

        private static void EscreverCabecalho(IXLWorksheet ws, bool incluirDisciplina, int qtdRev)
        {
            int col = 1;
            if (incluirDisciplina) ws.Cell(1, col++).Value = "Disciplina";
            ws.Cell(1, col++).Value = "Assunto";
            ws.Cell(1, col++).Value = "DataInicio";
            for (int i = 1; i <= qtdRev; i++)
                ws.Cell(1, col++).Value = $"Rev{i}";
        }

        /// <summary>
        /// Escreve uma linha de dados e aplica cor verde nas células de revisões concluídas.
        /// Se o assunto estiver destacado (IsDestacado), pinta toda a linha em verde claro.
        /// </summary>
        private static void EscreverLinha(IXLWorksheet ws, int linha, Assunto a, bool incluirDisciplina, int qtdRev)
        {
            int colInicio = 1;
            int col = colInicio;

            if (incluirDisciplina)
            {
                var cellDisc = ws.Cell(linha, col++);
                cellDisc.Value = a.Disciplina?.Nome ?? "Sem Disciplina";
                if (a.IsDestacado)
                    cellDisc.Style.Fill.BackgroundColor = CorVerdeLinhaDestacada;
            }

            var cellAssunto = ws.Cell(linha, col++);
            cellAssunto.Value = a.Titulo;
            if (a.IsDestacado)
                cellAssunto.Style.Fill.BackgroundColor = CorVerdeLinhaDestacada;

            var cellData = ws.Cell(linha, col++);
            cellData.Value = a.DataInicio;
            cellData.Style.NumberFormat.Format = "DD/MM/YYYY";
            if (a.IsDestacado)
                cellData.Style.Fill.BackgroundColor = CorVerdeLinhaDestacada;

            for (int i = 1; i <= qtdRev; i++)
            {
                var cellRev = ws.Cell(linha, col++);
                cellRev.Value = a.GetDataRev(i);
                cellRev.Style.NumberFormat.Format = "DD/MM/YYYY";

                // Verde forte = revisão concluída
                if (a.GetRevConcluida(i))
                {
                    cellRev.Style.Fill.BackgroundColor = CorVerdeCelula;
                    cellRev.Style.Font.FontColor = XLColor.White;
                    cellRev.Style.Font.Bold = true;
                }
                // Verde claro = assunto destacado mas revisão não concluída
                else if (a.IsDestacado)
                {
                    cellRev.Style.Fill.BackgroundColor = CorVerdeLinhaDestacada;
                }
            }
        }

        private static void FormatarPlanilha(IXLWorksheet ws, bool incluirDisciplina, int qtdRev, int ultimaLinha)
        {
            int totalCols = (incluirDisciplina ? 1 : 0) + 2 + qtdRev;
            var headerRange = ws.Range(1, 1, 1, totalCols);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(44, 62, 80);
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Column(1).Width = 28;
            ws.Column(2).Width = 35;

            int colData = (incluirDisciplina ? 3 : 2);
            for (int c = colData; c <= totalCols; c++)
                ws.Column(c).Width = 14;

            if (ultimaLinha >= 2)
            {
                var dataRange = ws.Range(1, 1, ultimaLinha, totalCols);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
                dataRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(189, 195, 199);
                dataRange.Style.Border.InsideBorderColor = XLColor.FromArgb(189, 195, 199);
            }

            // Linhas alternadas — mas não sobrescreve células já coloridas
            for (int r = 2; r <= ultimaLinha; r++)
            {
                if (r % 2 == 0)
                {
                    for (int c = 1; c <= totalCols; c++)
                    {
                        var cell = ws.Cell(r, c);
                        // Só aplica a cor alternada se a célula não tiver cor personalizada
                        if (cell.Style.Fill.BackgroundColor == XLColor.NoColor ||
                            cell.Style.Fill.BackgroundColor == XLColor.White)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(244, 247, 246);
                        }
                    }
                }
            }
        }

        // ── Helpers de leitura ────────────────────────────────────────────────────

        private static (int qtdRev, string erro) LerCabecalho(IXLWorksheet ws)
        {
            string col1 = ws.Cell(1, 1).GetString().Trim();
            string col2 = ws.Cell(1, 2).GetString().Trim();
            string col3 = ws.Cell(1, 3).GetString().Trim();

            if (!col1.Equals("Disciplina", StringComparison.OrdinalIgnoreCase))
                return (0, "Formato inválido: coluna A1 deve ser 'Disciplina'.");
            if (!col2.Equals("Assunto", StringComparison.OrdinalIgnoreCase))
                return (0, "Formato inválido: coluna B1 deve ser 'Assunto'.");
            if (!col3.Equals("DataInicio", StringComparison.OrdinalIgnoreCase))
                return (0, "Formato inválido: coluna C1 deve ser 'DataInicio'.");

            int qtdRev = 0;
            int col = 4;
            while (true)
            {
                string header = ws.Cell(1, col).GetString().Trim();
                if (string.IsNullOrEmpty(header)) break;
                if (!header.Equals($"Rev{qtdRev + 1}", StringComparison.OrdinalIgnoreCase))
                    return (0, $"Formato inválido: esperado 'Rev{qtdRev + 1}' na coluna {col}, encontrado '{header}'.");
                qtdRev++;
                col++;
                if (qtdRev > 30) break;
            }

            if (qtdRev == 0)
                return (0, "Formato inválido: nenhuma coluna de revisão encontrada.");

            return (qtdRev, null);
        }

        private class LinhaDados
        {
            public string NomeDisciplina { get; set; }
            public string Titulo { get; set; }
            public DateTime DataInicio { get; set; }
            public int[] Intervalos { get; set; }
            public bool[] RevisoesConcluidas { get; set; } // lido pela cor verde
            public bool IsDestacado { get; set; }          // lido pela cor verde claro na linha
        }

        /// <summary>
        /// Lê as linhas de dados e detecta cor verde nas células de revisão.
        /// Verde forte (#27AE60 / rgb 39,174,96) → revisão concluída.
        /// Verde claro (#D4EFDF / rgb 212,239,223) em Disciplina ou Assunto → IsDestacado.
        /// </summary>
        private static List<LinhaDados> LerLinhas(IXLWorksheet ws, int qtdRev)
        {
            var lista = new List<LinhaDados>();
            int linha = 2;

            while (true)
            {
                string disciplina = ws.Cell(linha, 1).GetString().Trim();
                if (string.IsNullOrEmpty(disciplina)) break;

                string assunto = ws.Cell(linha, 2).GetString().Trim();
                if (string.IsNullOrEmpty(assunto))
                    throw new Exception($"Linha {linha}: assunto vazio.");

                if (!TentarLerData(ws.Cell(linha, 3), out DateTime dataInicio))
                    throw new Exception($"Linha {linha}: DataInicio inválida.");

                // Detecta IsDestacado pela cor de fundo da célula de Disciplina ou Assunto
                bool isDestacado = EhVerdeClaro(ws.Cell(linha, 1)) || EhVerdeClaro(ws.Cell(linha, 2));

                var intervalos = new int[qtdRev];
                var revisoesConcluidas = new bool[qtdRev];

                for (int i = 0; i < qtdRev; i++)
                {
                    var cellRev = ws.Cell(linha, 4 + i);

                    if (!TentarLerData(cellRev, out DateTime dataRev))
                        throw new Exception($"Linha {linha}, Rev{i + 1}: data inválida.");

                    // Intervalo acumulado desde a data de início (compatível com a lógica encadeada)
                    int dias = (int)(dataRev.Date - dataInicio.Date).TotalDays;
                    if (dias < 1)
                        throw new Exception($"Linha {linha}, Rev{i + 1}: data de revisão deve ser posterior à DataInicio.");

                    intervalos[i] = dias;

                    // Detecta revisão concluída pela cor verde da célula
                    revisoesConcluidas[i] = EhVerdeConcluida(cellRev);
                }

                lista.Add(new LinhaDados
                {
                    NomeDisciplina = disciplina,
                    Titulo = assunto,
                    DataInicio = dataInicio,
                    Intervalos = intervalos,
                    RevisoesConcluidas = revisoesConcluidas,
                    IsDestacado = isDestacado
                });

                linha++;
            }

            return lista;
        }

        // ── Detecção de cor ───────────────────────────────────────────────────────

        /// <summary>Verde forte = revisão concluída. rgb(39,174,96) = #27AE60</summary>
        private static bool EhVerdeConcluida(IXLCell cell)
        {
            try
            {
                var bg = cell.Style.Fill.BackgroundColor;
                if (bg.ColorType == XLColorType.Color)
                {
                    var c = bg.Color;
                    return c.R == 39 && c.G == 174 && c.B == 96;
                }
            }
            catch { }
            return false;
        }

        /// <summary>Verde claro = assunto destacado. rgb(212,239,223) = #D4EFDF</summary>
        private static bool EhVerdeClaro(IXLCell cell)
        {
            try
            {
                var bg = cell.Style.Fill.BackgroundColor;
                if (bg.ColorType == XLColorType.Color)
                {
                    var c = bg.Color;
                    return c.R == 212 && c.G == 239 && c.B == 223;
                }
            }
            catch { }
            return false;
        }

        private static bool TentarLerData(IXLCell cell, out DateTime data)
        {
            try
            {
                if (cell.DataType == XLDataType.DateTime)
                {
                    data = cell.GetDateTime();
                    return true;
                }
                string s = cell.GetString().Trim();
                return DateTime.TryParseExact(s,
                    new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out data);
            }
            catch { data = default; return false; }
        }

        // ── Helpers de intervalo dinâmico ─────────────────────────────────────────

        private static void AplicarIntervalos(Assunto a, int[] intervalos, int qtdRev)
        {
            for (int i = 0; i < qtdRev; i++)
                DefinirIntervalo(a, i + 1, intervalos[i]);
        }

        private static int ObterIntervalo(Assunto a, int n) => a.GetIntervalo(n);

        private static void DefinirIntervalo(Assunto a, int n, int valor) => a.SetIntervalo(n, valor);

        // ── Utilitários ───────────────────────────────────────────────────────────

        private static string SanitizarNomeAba(string nome)
        {
            foreach (char c in new[] { '\\', '/', '?', '*', '[', ']', ':' })
                nome = nome.Replace(c, '-');
            return nome.Length > 31 ? nome.Substring(0, 31) : nome;
        }

        private static ResultadoImportacao Erro(string msg) =>
            new ResultadoImportacao { Sucesso = false, Mensagem = msg };
    }
}
