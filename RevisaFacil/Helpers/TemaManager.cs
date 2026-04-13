// TemaManager.cs (Helpers)
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
        public static string TemaAtual { get; set; } = "Padrao";

        public static List<string> ListarTemas()
        {
            var pasta = AppDomain.CurrentDomain.BaseDirectory;
            var arquivos = Directory.GetFiles(pasta, "*.db");
            return arquivos.Select(Path.GetFileNameWithoutExtension).ToList();
        }

        public static string GetDbPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{TemaAtual}.db");

        public static void DeletarBanco(string nomeTema)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nomeTema}.db");

            if (File.Exists(dbPath))
            {
                // Libera o pool de conexões do SQLite para evitar o erro de "processo em uso"
                SqliteConnection.ClearAllPools();

                // Força o Garbage Collector a liberar handles de arquivos
                GC.Collect();
                GC.WaitForPendingFinalizers();

                try { File.Delete(dbPath); }
                catch (IOException)
                {
                    // Se ainda assim falhar, tenta novamente após um breve delay
                    System.Threading.Thread.Sleep(100);
                    File.Delete(dbPath);
                }
            }
        }

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

                var colunasEsperadas = new Dictionary<string, string>
                {
                    { "QuantidadeRevisoes", "10" },
                    { "Intervalo1",  "30"  }, { "Intervalo2",  "60"  }, { "Intervalo3",  "90"  },
                    { "Intervalo4",  "120" }, { "Intervalo5",  "150" }, { "Intervalo6",  "180" },
                    { "Intervalo7",  "210" }, { "Intervalo8",  "240" }, { "Intervalo9",  "270" },
                    { "Intervalo10", "300" }, { "Intervalo11", "330" }, { "Intervalo12", "360" },
                    { "Intervalo13", "390" }, { "Intervalo14", "420" }, { "Intervalo15", "450" },
                    { "Intervalo16", "480" }, { "Intervalo17", "510" }, { "Intervalo18", "540" },
                    { "Intervalo19", "570" }, { "Intervalo20", "600" }, { "Intervalo21", "630" },
                    { "Intervalo22", "660" }, { "Intervalo23", "690" }, { "Intervalo24", "720" },
                    { "Intervalo25", "750" }, { "Intervalo26", "780" }, { "Intervalo27", "810" },
                    { "Intervalo28", "840" }, { "Intervalo29", "870" }, { "Intervalo30", "900" },
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

        public static void SincronizarCalendarioGlobal()
        {
            try
            {
                using var db = new EstudoDbContext();

                var notasRevisao = db.NotasCalendario.Where(n => n.AssuntoId == -1).ToList();
                db.NotasCalendario.RemoveRange(notasRevisao);
                db.SaveChanges();

                var config = db.Configuracoes.FirstOrDefault();
                int qtdRevisoes = config?.QuantidadeRevisoes ?? 10;

                var todosAssuntos = db.Assuntos.Include(a => a.Disciplina).ToList();
                var agrupamento = new Dictionary<DateTime, List<string>>();

                foreach (var assunto in todosAssuntos)
                {
                    string nomeDisc = assunto.Disciplina?.Nome ?? "Sem Disciplina";
                    for (int i = 1; i <= qtdRevisoes; i++)
                    {
                        DateTime dataRev = assunto.GetDataRev(i).Date;
                        string texto = $"Estudar Disciplina: {nomeDisc.ToUpper()} - Assunto: {assunto.Titulo} - {i}ª Revisão";

                        if (!agrupamento.ContainsKey(dataRev))
                            agrupamento[dataRev] = new List<string>();

                        agrupamento[dataRev].Add(texto);
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
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Erro na Sincronização: " + ex.Message); }
        }

        public static void SeedDatabase(EstudoDbContext db)
        {
            try
            {
                // 1. Bloqueio de segurança para o banco padrão
                if (TemaAtual == "Padrao")
                {
                    System.Diagnostics.Debug.WriteLine("Seed ignorado: O tema 'Padrao' deve permanecer vazio.");
                    return;
                }

                // Garante a criação do arquivo físico .db
                db.Database.EnsureCreated();

                // 2. Se já houver dados, não faz nada para não duplicar
                if (db.Disciplinas.Any()) return;

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.json");
                if (!File.Exists(filePath)) return;

                string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var root = JsonSerializer.Deserialize<SeedRoot>(jsonString, options);

                // 3. Validação por Nome do Tema
                // Só executa o seed se o nome do arquivo .db criado for igual ao "NomeTema" do JSON
                if (root?.NomeTema != null && root.NomeTema.Trim().Equals(TemaAtual.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Iniciando carga de dados para o tema: {root.NomeTema}");

                    foreach (var d in root.Disciplinas)
                    {
                        var novaDisc = new Disciplina { Nome = d.Nome };
                        db.Disciplinas.Add(novaDisc);
                        db.SaveChanges(); // Salva para gerar o ID da disciplina

                        foreach (var a in d.Assuntos)
                        {
                            db.Assuntos.Add(new Assunto
                            {
                                Titulo = a.Titulo,
                                DataInicio = a.DataInicio,
                                DisciplinaId = novaDisc.Id,
                                // Usa os valores padrão (30, 60...) se não houver no JSON
                                Int1 = 30,
                                Int2 = 60,
                                Int3 = 90,
                                Int4 = 120,
                                Int5 = 150,
                                Int6 = 180,
                                Int7 = 210,
                                Int8 = 240,
                                Int9 = 270,
                                Int10 = 300
                            });
                        }
                    }
                    db.SaveChanges();

                    // Sincroniza o calendário para que as datas das revisões apareçam imediatamente
                    SincronizarCalendarioGlobal();

                    System.Diagnostics.Debug.WriteLine("Seed concluído com sucesso.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Seed ignorado: O nome no JSON '{root?.NomeTema}' não coincide com o tema atual '{TemaAtual}'.");
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
        public int Int1 { get; set; } = 30; public int Int2 { get; set; } = 60;
        public int Int3 { get; set; } = 90; public int Int4 { get; set; } = 120;
        public int Int5 { get; set; } = 150; public int Int6 { get; set; } = 180;
        public int Int7 { get; set; } = 210; public int Int8 { get; set; } = 240;
        public int Int9 { get; set; } = 270; public int Int10 { get; set; } = 300;
    }
}