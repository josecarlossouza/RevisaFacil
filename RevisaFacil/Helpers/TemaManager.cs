using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
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

        public static string GetDbPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{TemaAtual}.db");

        /// <summary>
        /// Sincroniza as notas de revisão automática (AssuntoId = -1) no calendário,
        /// preservando SEMPRE as notas manuais do usuário (AssuntoId = -2).
        /// Estratégia: delete apenas as notas de revisão automática e recrie-as
        /// a partir dos dados atuais do banco de Assuntos.
        /// </summary>
        public static void SincronizarCalendarioGlobal()
        {
            try
            {
                using (var db = new EstudoDbContext())
                {
                    // 1. Remove APENAS as notas de revisão automática (AssuntoId = -1)
                    //    As notas manuais (AssuntoId = -2) são PRESERVADAS.
                    var notasRevisao = db.NotasCalendario
                        .Where(n => n.AssuntoId == -1)
                        .ToList();

                    db.NotasCalendario.RemoveRange(notasRevisao);
                    db.SaveChanges();

                    // 2. Carrega todos os assuntos com suas disciplinas
                    var todosAssuntos = db.Assuntos
                        .Include(a => a.Disciplina)
                        .ToList();

                    // 3. Agrupa todas as revisões por data
                    var agrupamento = new Dictionary<DateTime, List<string>>();

                    foreach (var assunto in todosAssuntos)
                    {
                        string nomeDisc = assunto.Disciplina?.Nome ?? "Sem Disciplina";

                        // Cria tuplas (data, número da revisão) para as 5 revisões
                        var revisoes = new[]
                        {
                            (Data: assunto.DataRev1.Date, Numero: 1),
                            (Data: assunto.DataRev2.Date, Numero: 2),
                            (Data: assunto.DataRev3.Date, Numero: 3),
                            (Data: assunto.DataRev4.Date, Numero: 4),
                            (Data: assunto.DataRev5.Date, Numero: 5),
                        };

                        foreach (var rev in revisoes)
                        {
                            string texto = $"Estudar Disciplina: {nomeDisc.ToUpper()} - Assunto: {assunto.Titulo} - {rev.Numero}ª Revisão";

                            if (!agrupamento.ContainsKey(rev.Data))
                                agrupamento[rev.Data] = new List<string>();

                            agrupamento[rev.Data].Add(texto);
                        }
                    }

                    // 4. Salva as revisões agrupadas por data (AssuntoId = -1)
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro na SincronizarCalendarioGlobal: " + ex.Message);
            }
        }

        public static void SeedDatabase(EstudoDbContext db)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.json");
            if (!File.Exists(filePath)) return;
            if (db.Disciplinas.Any()) return;

            try
            {
                string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
                var root = JsonSerializer.Deserialize<SeedRoot>(jsonString);

                if (root?.Disciplinas != null)
                {
                    foreach (var d in root.Disciplinas)
                    {
                        var novaDisc = new Disciplina { Nome = d.Nome };
                        db.Disciplinas.Add(novaDisc);
                        db.SaveChanges();

                        foreach (var a in d.Assuntos)
                        {
                            db.Assuntos.Add(new Assunto
                            {
                                Titulo = a.Titulo,
                                DataInicio = a.DataInicio,
                                DisciplinaId = novaDisc.Id,
                                Int1 = a.Int1,
                                Int2 = a.Int2,
                                Int3 = a.Int3,
                                Int4 = a.Int4,
                                Int5 = a.Int5
                            });
                        }
                    }
                    db.SaveChanges();
                    SincronizarCalendarioGlobal();
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Erro no Seed: " + ex.Message); }
        }
    }

    // Classes necessárias para o SeedDatabase
    public class SeedRoot { public string NomeTema { get; set; } public List<DisciplinaSeed> Disciplinas { get; set; } }
    public class DisciplinaSeed { public string Nome { get; set; } public List<AssuntoSeed> Assuntos { get; set; } }
    public class AssuntoSeed { public string Titulo { get; set; } public DateTime DataInicio { get; set; } public int Int1 { get; set; } = 30; public int Int2 { get; set; } = 60; public int Int3 { get; set; } = 90; public int Int4 { get; set; } = 120; public int Int5 { get; set; } = 150; }
}