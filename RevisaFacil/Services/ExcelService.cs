// ExcelService.cs  (Services/)
// Exporta e importa dados do RevisaFácil em formato .xlsx usando ClosedXML.
//
// NuGet necessário (instale pelo Package Manager Console):
//   Install-Package ClosedXML
//
// Formato do arquivo exportado:
//   Aba "Todos"          → todas as disciplinas/assuntos juntos
//   Aba "<Disciplina>"   → uma aba por disciplina, só os assuntos dela
//
// Colunas em cada aba:
//   Disciplina | Assunto | DataInicio | Rev1 | Rev2 | ... | RevN
//   (Disciplina só aparece na aba "Todos")

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
        // ── Exportar ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Exporta todos os assuntos do tema atual para um arquivo .xlsx.
        /// Retorna o caminho do arquivo gerado.
        /// </summary>
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
                // Nome da aba: máx 31 chars (limite do Excel), sem caracteres inválidos
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

        /// <summary>
        /// Importa de um .xlsx exportado por este serviço.
        /// Usa somente a aba "Todos" como fonte de verdade.
        /// </summary>
        public static ResultadoImportacao Importar(string caminhoArquivo)
        {
            var resultado = new ResultadoImportacao();

            try
            {
                using var wb = new XLWorkbook(caminhoArquivo);

                // ── Valida formato ────────────────────────────────────────────────
                if (!wb.Worksheets.Contains("Todos"))
                    return Erro("Formato inválido: aba 'Todos' não encontrada.");

                var ws = wb.Worksheet("Todos");

                // Lê cabeçalho para descobrir qtdRev do arquivo
                var (qtdRevArquivo, erroHeader) = LerCabecalho(ws);
                if (erroHeader != null)
                    return Erro(erroHeader);

                if (qtdRevArquivo < 1 || qtdRevArquivo > 30)
                    return Erro($"Formato inválido: quantidade de revisões no arquivo ({qtdRevArquivo}) fora do intervalo permitido (1-30).");

                // Nome do tema = nome do arquivo sem extensão
                string nomeTema = Path.GetFileNameWithoutExtension(caminhoArquivo);

                // ── Lê linhas de dados ────────────────────────────────────────────
                var linhas = LerLinhas(ws, qtdRevArquivo);
                if (linhas == null)
                    return Erro("Formato inválido: erro ao ler os dados da aba 'Todos'.");

                // ── Verifica/cria o tema ──────────────────────────────────────────
                bool temaExistia = TemaManager.ListarTemas().Contains(nomeTema);
                string temaAnterior = TemaManager.TemaAtual;

                if (!temaExistia)
                {
                    TemaManager.TemaAtual = nomeTema;
                    using (var dbNovo = new EstudoDbContext(TemaManager.GetDbPath()))
                        dbNovo.Database.EnsureCreated();
                    TemaManager.MigrarConfiguracoes();
                    resultado.TemaNovosCriado = true;
                }
                else
                {
                    TemaManager.TemaAtual = nomeTema;
                    TemaManager.MigrarConfiguracoes();
                }

                // ── Persiste os dados ─────────────────────────────────────────────
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    // Atualiza/cria configuração de qtdRev
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
                            // Novo assunto
                            assunto = new Assunto
                            {
                                Titulo = row.Titulo,
                                DisciplinaId = disc.Id,
                                DataInicio = row.DataInicio
                            };
                            AplicarIntervalos(assunto, row.Intervalos, qtdRevArquivo);
                            db.Assuntos.Add(assunto);
                            resultado.AssuntosNovos++;
                        }
                        else
                        {
                            // Atualiza se algo mudou
                            bool mudou = false;

                            if (assunto.DataInicio.Date != row.DataInicio.Date)
                            {
                                assunto.DataInicio = row.DataInicio;
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

        private static void EscreverLinha(IXLWorksheet ws, int linha, Assunto a, bool incluirDisciplina, int qtdRev)
        {
            int col = 1;
            if (incluirDisciplina)
                ws.Cell(linha, col++).Value = a.Disciplina?.Nome ?? "Sem Disciplina";

            ws.Cell(linha, col++).Value = a.Titulo;

            var cellData = ws.Cell(linha, col++);
            cellData.Value = a.DataInicio;
            cellData.Style.NumberFormat.Format = "DD/MM/YYYY";

            for (int i = 1; i <= qtdRev; i++)
            {
                var cellRev = ws.Cell(linha, col++);
                cellRev.Value = a.GetDataRev(i);
                cellRev.Style.NumberFormat.Format = "DD/MM/YYYY";
            }
        }

        private static void FormatarPlanilha(IXLWorksheet ws, bool incluirDisciplina, int qtdRev, int ultimaLinha)
        {
            // Cabeçalho em negrito com fundo azul escuro
            int totalCols = (incluirDisciplina ? 1 : 0) + 2 + qtdRev;
            var headerRange = ws.Range(1, 1, 1, totalCols);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(44, 62, 80);
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Auto-fit nas colunas de texto; largura fixa nas datas
            ws.Column(incluirDisciplina ? 1 : 1).Width = 28;
            ws.Column(incluirDisciplina ? 2 : 1).Width = 35;

            int colData = (incluirDisciplina ? 3 : 2);
            for (int c = colData; c <= totalCols; c++)
                ws.Column(c).Width = 14;

            // Bordas na tabela de dados
            if (ultimaLinha >= 2)
            {
                var dataRange = ws.Range(1, 1, ultimaLinha, totalCols);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
                dataRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(189, 195, 199);
                dataRange.Style.Border.InsideBorderColor = XLColor.FromArgb(189, 195, 199);
            }

            // Linhas alternadas
            for (int r = 2; r <= ultimaLinha; r++)
            {
                if (r % 2 == 0)
                    ws.Range(r, 1, r, totalCols).Style.Fill.BackgroundColor = XLColor.FromArgb(244, 247, 246);
            }
        }

        // ── Helpers de leitura ────────────────────────────────────────────────────

        private static (int qtdRev, string erro) LerCabecalho(IXLWorksheet ws)
        {
            // Espera: Disciplina | Assunto | DataInicio | Rev1 | Rev2 | ...
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
            public int[] Intervalos { get; set; } // diferença em dias entre DataInicio e cada RevX
        }

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
                    throw new Exception($"Linha {linha}: DataInicio inválida ('{ws.Cell(linha, 3).GetString()}').");

                var intervalos = new int[qtdRev];
                for (int i = 0; i < qtdRev; i++)
                {
                    if (!TentarLerData(ws.Cell(linha, 4 + i), out DateTime dataRev))
                        throw new Exception($"Linha {linha}, Rev{i + 1}: data inválida ('{ws.Cell(linha, 4 + i).GetString()}').");

                    int dias = (int)(dataRev.Date - dataInicio.Date).TotalDays;
                    if (dias < 1)
                        throw new Exception($"Linha {linha}, Rev{i + 1}: data de revisão deve ser posterior à DataInicio.");

                    intervalos[i] = dias;
                }

                lista.Add(new LinhaDados
                {
                    NomeDisciplina = disciplina,
                    Titulo = assunto,
                    DataInicio = dataInicio,
                    Intervalos = intervalos
                });

                linha++;
            }

            return lista;
        }

        private static bool TentarLerData(IXLCell cell, out DateTime data)
        {
            // ClosedXML pode retornar como DateTime diretamente ou como string
            try
            {
                if (cell.DataType == XLDataType.DateTime)
                {
                    data = cell.GetDateTime();
                    return true;
                }
                string s = cell.GetString().Trim();
                return DateTime.TryParseExact(s, new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out data);
            }
            catch { data = default; return false; }
        }

        // ── Helpers de intervalo dinâmico ─────────────────────────────────────────

        private static void AplicarIntervalos(Assunto a, int[] intervalos, int qtdRev)
        {
            for (int i = 0; i < qtdRev; i++)
                DefinirIntervalo(a, i + 1, intervalos[i]);
        }

        private static int ObterIntervalo(Assunto a, int n) => n switch
        {
            1 => a.Int1,
            2 => a.Int2,
            3 => a.Int3,
            4 => a.Int4,
            5 => a.Int5,
            6 => a.Int6,
            7 => a.Int7,
            8 => a.Int8,
            9 => a.Int9,
            10 => a.Int10,
            11 => a.Int11,
            12 => a.Int12,
            13 => a.Int13,
            14 => a.Int14,
            15 => a.Int15,
            16 => a.Int16,
            17 => a.Int17,
            18 => a.Int18,
            19 => a.Int19,
            20 => a.Int20,
            21 => a.Int21,
            22 => a.Int22,
            23 => a.Int23,
            24 => a.Int24,
            25 => a.Int25,
            26 => a.Int26,
            27 => a.Int27,
            28 => a.Int28,
            29 => a.Int29,
            30 => a.Int30,
            _ => 30
        };

        private static void DefinirIntervalo(Assunto a, int n, int valor)
        {
            switch (n)
            {
                case 1: a.Int1 = valor; break;
                case 2: a.Int2 = valor; break;
                case 3: a.Int3 = valor; break;
                case 4: a.Int4 = valor; break;
                case 5: a.Int5 = valor; break;
                case 6: a.Int6 = valor; break;
                case 7: a.Int7 = valor; break;
                case 8: a.Int8 = valor; break;
                case 9: a.Int9 = valor; break;
                case 10: a.Int10 = valor; break;
                case 11: a.Int11 = valor; break;
                case 12: a.Int12 = valor; break;
                case 13: a.Int13 = valor; break;
                case 14: a.Int14 = valor; break;
                case 15: a.Int15 = valor; break;
                case 16: a.Int16 = valor; break;
                case 17: a.Int17 = valor; break;
                case 18: a.Int18 = valor; break;
                case 19: a.Int19 = valor; break;
                case 20: a.Int20 = valor; break;
                case 21: a.Int21 = valor; break;
                case 22: a.Int22 = valor; break;
                case 23: a.Int23 = valor; break;
                case 24: a.Int24 = valor; break;
                case 25: a.Int25 = valor; break;
                case 26: a.Int26 = valor; break;
                case 27: a.Int27 = valor; break;
                case 28: a.Int28 = valor; break;
                case 29: a.Int29 = valor; break;
                case 30: a.Int30 = valor; break;
            }
        }

        // ── Utilitários ───────────────────────────────────────────────────────────

        private static string SanitizarNomeAba(string nome)
        {
            // Caracteres proibidos no nome de abas do Excel: \ / ? * [ ] :
            foreach (char c in new[] { '\\', '/', '?', '*', '[', ']', ':' })
                nome = nome.Replace(c, '-');
            return nome.Length > 31 ? nome.Substring(0, 31) : nome;
        }

        private static ResultadoImportacao Erro(string msg) =>
            new ResultadoImportacao { Sucesso = false, Mensagem = msg };
    }
}