using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
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

        public static void SeedDatabase(EstudoDbContext db)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.json");
            if (!File.Exists(filePath)) return;
            if (db.Disciplinas.Any()) return; // Não duplica se já houver dados

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
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Erro no Seed: " + ex.Message); }
        }
    }

    public class SeedRoot
    {
        public string NomeTema { get; set; }
        public List<DisciplinaSeed> Disciplinas { get; set; }
    }

    public class DisciplinaSeed
    {
        public string Nome { get; set; }
        public List<AssuntoSeed> Assuntos { get; set; }
    }

    public class AssuntoSeed
    {
        public string Titulo { get; set; }
        public DateTime DataInicio { get; set; }
        public int Int1 { get; set; } = 30;
        public int Int2 { get; set; } = 60;
        public int Int3 { get; set; } = 90;
        public int Int4 { get; set; } = 120;
        public int Int5 { get; set; } = 150;
    }
}