// TemaManager.cs
// MODIFICAÇÕES:
// 1. MigrarConfiguracoes() agora também cria a coluna UltimaDisciplinaId em Configuracoes.
// 2. MigrarConfiguracoesDisciplinas() — novo método que cria as colunas de
//    QuantidadeRevisoes e Intervalo1..30 na tabela Disciplinas (via ALTER TABLE).
// 3. SincronizarCalendarioGlobal() permanece igual.
// 4. GetIntervaloEfetivo() — helper que retorna o intervalo de uma disciplina
//    (próprio ou fallback global), usado por AssuntosPage ao aplicar intervalos.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RevisaFacil.Data;
using RevisaFacil.Models;

namespace RevisaFacil.Helpers
{
    public static class TemaManager
    {
        // ── Tema atual ────────────────────────────────────────────────────────────

        public static string TemaAtual { get; set; } = "Padrao";

        public static string GetDbPath()
        {
            string pasta = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(pasta, $"{TemaAtual}.db");
        }

        public static List<string> ListarTemas()
        {
            string pasta = AppDomain.CurrentDomain.BaseDirectory;
            return Directory.GetFiles(pasta, "*.db")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(n => n != "Padrao" || true)   // inclui Padrao na lista
                .OrderBy(n => n)
                .ToList();
        }

        public static void DeletarBanco(string tema)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{tema}.db");
            if (File.Exists(path))
            {
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(path);
            }
        }

        // ── Migrações de schema ───────────────────────────────────────────────────

        /// <summary>
        /// Garante que a tabela Configuracoes tenha todas as colunas necessárias,
        /// incluindo as novas: UltimaDisciplinaId e os Intervalo1..30 ajustados para
        /// padrão 30 (lógica encadeada).
        /// </summary>
        public static void MigrarConfiguracoes()
        {
            try
            {
                string dbPath = GetDbPath();
                if (!File.Exists(dbPath)) return;

                using var conn = new SqliteConnection($"Data Source={dbPath}");
                conn.Open();

                var colunasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(Configuracoes)";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        colunasExistentes.Add(reader.GetString(1));
                }

                if (colunasExistentes.Count == 0) return;

                // Mapa: nome da coluna → valor padrão SQL
                var colunasEsperadas = new Dictionary<string, string>
                {
                    { "QuantidadeRevisoes", "10" },
                    { "UltimaDisciplinaId", "0"  },
                    // Intervalos padrão 30 dias (lógica encadeada: cada revisão é 30d após a anterior)
                    { "Intervalo1",  "30" }, { "Intervalo2",  "30" }, { "Intervalo3",  "30" },
                    { "Intervalo4",  "30" }, { "Intervalo5",  "30" }, { "Intervalo6",  "30" },
                    { "Intervalo7",  "30" }, { "Intervalo8",  "30" }, { "Intervalo9",  "30" },
                    { "Intervalo10", "30" }, { "Intervalo11", "30" }, { "Intervalo12", "30" },
                    { "Intervalo13", "30" }, { "Intervalo14", "30" }, { "Intervalo15", "30" },
                    { "Intervalo16", "30" }, { "Intervalo17", "30" }, { "Intervalo18", "30" },
                    { "Intervalo19", "30" }, { "Intervalo20", "30" }, { "Intervalo21", "30" },
                    { "Intervalo22", "30" }, { "Intervalo23", "30" }, { "Intervalo24", "30" },
                    { "Intervalo25", "30" }, { "Intervalo26", "30" }, { "Intervalo27", "30" },
                    { "Intervalo28", "30" }, { "Intervalo29", "30" }, { "Intervalo30", "30" },
                };

                foreach (var kv in colunasEsperadas)
                {
                    if (!colunasExistentes.Contains(kv.Key))
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = $"ALTER TABLE Configuracoes ADD COLUMN \"{kv.Key}\" INTEGER NOT NULL DEFAULT {kv.Value}";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro em MigrarConfiguracoes: " + ex.Message);
            }
        }

        /// <summary>
        /// Garante que a tabela Disciplinas tenha as novas colunas de
        /// QuantidadeRevisoes e Intervalo1..30 (todas nullable).
        /// Deve ser chamado após MigrarConfiguracoes().
        /// </summary>
        public static void MigrarConfiguracoesDisciplinas()
        {
            try
            {
                string dbPath = GetDbPath();
                if (!File.Exists(dbPath)) return;

                using var conn = new SqliteConnection($"Data Source={dbPath}");
                conn.Open();

                var colunasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(Disciplinas)";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        colunasExistentes.Add(reader.GetString(1));
                }

                if (colunasExistentes.Count == 0) return;

                // Todas nullable — ausência = usa global
                var colunasDisciplina = new List<string> { "QuantidadeRevisoes" };
                for (int i = 1; i <= 30; i++) colunasDisciplina.Add($"Intervalo{i}");

                foreach (var col in colunasDisciplina)
                {
                    if (!colunasExistentes.Contains(col))
                    {
                        using var cmd = conn.CreateCommand();
                        // NULL default = "não configurado, usa global"
                        cmd.CommandText = $"ALTER TABLE Disciplinas ADD COLUMN \"{col}\" INTEGER NULL DEFAULT NULL";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro em MigrarConfiguracoesDisciplinas: " + ex.Message);
            }
        }

        // ── Quantidade de revisões ────────────────────────────────────────────────

        public static int GetQuantidadeRevisoes()
        {
            try
            {
                MigrarConfiguracoes();
                using var db = new EstudoDbContext();
                var config = db.Configuracoes.FirstOrDefault();
                return config?.QuantidadeRevisoes ?? 10;
            }
            catch { return 10; }
        }

        /// <summary>
        /// Retorna a quantidade de revisões efetiva para uma disciplina:
        /// usa o valor próprio da disciplina, ou cai para o global.
        /// </summary>
        public static int GetQuantidadeRevisoesDisciplina(Disciplina d, Configuracao config)
            => d?.QuantidadeRevisoes ?? config?.QuantidadeRevisoes ?? 10;

        /// <summary>
        /// Retorna o intervalo efetivo para a revisão N de uma disciplina:
        /// usa o valor próprio da disciplina, ou cai para o global.
        /// </summary>
        public static int GetIntervaloEfetivo(Disciplina d, Configuracao config, int n)
            => d?.GetIntervalo(n) ?? config?.GetIntervalo(n) ?? 30;

        // ── Sincronização do calendário ───────────────────────────────────────────

        public static void SincronizarCalendarioGlobal()
        {
            try
            {
                using var db = new EstudoDbContext();

                // Remove apenas as notas de revisão automática (preserva manuais)
                var notasRevisao = db.NotasCalendario.Where(n => n.AssuntoId == -1).ToList();
                db.NotasCalendario.RemoveRange(notasRevisao);
                db.SaveChanges();

                var config = db.Configuracoes.FirstOrDefault();
                int qtdRevisoesGlobal = config?.QuantidadeRevisoes ?? 10;

                var todasDisciplinas = db.Disciplinas.Include(d => d.Assuntos).ToList();
                var agrupamento = new Dictionary<DateTime, List<string>>();

                foreach (var disciplina in todasDisciplinas)
                {
                    int qtdRev = TemaManager.GetQuantidadeRevisoesDisciplina(disciplina, config);
                    string nomeDisc = disciplina.Nome;

                    foreach (var assunto in disciplina.Assuntos)
                    {
                        for (int i = 1; i <= qtdRev; i++)
                        {
                            DateTime dataRev = assunto.GetDataRev(i).Date;
                            string texto = $"Estudar Disciplina: {nomeDisc.ToUpper()} - Assunto: {assunto.Titulo} - {i}ª Revisão";

                            if (!agrupamento.ContainsKey(dataRev))
                                agrupamento[dataRev] = new List<string>();

                            agrupamento[dataRev].Add(texto);
                        }
                    }
                }

                foreach (var item in agrupamento)
                {
                    db.NotasCalendario.Add(new NotaCalendario
                    {
                        Data = item.Key,
                        Conteudo = string.Join("\n", item.Value),
                        AssuntoId = -1
                    });
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro na Sincronização: " + ex.Message);
            }
        }

        // ── Seed ─────────────────────────────────────────────────────────────────

        public static void SeedDatabase(EstudoDbContext db)
        {
            try
            {
                if (TemaAtual == "Padrao")
                {
                    System.Diagnostics.Debug.WriteLine("Seed ignorado: O tema 'Padrao' deve permanecer vazio.");
                    return;
                }

                db.Database.EnsureCreated();

                if (db.Disciplinas.Any()) return;

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.json");
                if (!File.Exists(filePath)) return;

                string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var root = JsonSerializer.Deserialize<SeedRoot>(jsonString, options);

                if (root?.NomeTema != null && root.NomeTema.Trim().Equals(TemaAtual.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Iniciando carga de dados para o tema: {root.NomeTema}");

                    foreach (var d in root.Disciplinas)
                    {
                        var novaDisc = new Disciplina { Nome = d.Nome };
                        db.Disciplinas.Add(novaDisc);
                        db.SaveChanges();

                        foreach (var a in d.Assuntos)
                        {
                            // Padrão encadeado: 30d cada
                            db.Assuntos.Add(new Assunto
                            {
                                Titulo = a.Titulo,
                                DataInicio = a.DataInicio,
                                DisciplinaId = novaDisc.Id,
                                Int1 = 30,
                                Int2 = 30,
                                Int3 = 30,
                                Int4 = 30,
                                Int5 = 30,
                                Int6 = 30,
                                Int7 = 30,
                                Int8 = 30,
                                Int9 = 30,
                                Int10 = 30
                            });
                        }
                    }
                    db.SaveChanges();
                    SincronizarCalendarioGlobal();
                    System.Diagnostics.Debug.WriteLine("Seed concluído com sucesso.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Seed ignorado: nome no JSON não coincide com tema atual.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao processar seed.json: " + ex.Message);
            }
        }
    }

    public class SeedRoot { public string NomeTema { get; set; } public List<DisciplinaSeed> Disciplinas { get; set; } }
    public class DisciplinaSeed { public string Nome { get; set; } public List<AssuntoSeed> Assuntos { get; set; } }
    public class AssuntoSeed
    {
        public string Titulo { get; set; }
        public DateTime DataInicio { get; set; }
        public int Int1 { get; set; } = 30; public int Int2 { get; set; } = 30;
        public int Int3 { get; set; } = 30; public int Int4 { get; set; } = 30;
        public int Int5 { get; set; } = 30; public int Int6 { get; set; } = 30;
        public int Int7 { get; set; } = 30; public int Int8 { get; set; } = 30;
        public int Int9 { get; set; } = 30; public int Int10 { get; set; } = 30;
    }
}
