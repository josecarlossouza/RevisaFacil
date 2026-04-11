using Microsoft.EntityFrameworkCore;
using RevisaFacil.Models;
using RevisaFacil.Helpers;

namespace RevisaFacil.Data
{
    public class EstudoDbContext : DbContext
    {
        private readonly string _dbPath;

        public EstudoDbContext(string dbPath = null)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                _dbPath = TemaManager.GetDbPath();
            }
            else
            {
                _dbPath = dbPath.EndsWith(".db") ? dbPath : $"{dbPath}.db";
            }
        }

        public DbSet<Assunto> Assuntos { get; set; }
        public DbSet<Disciplina> Disciplinas { get; set; }

        public DbSet<Configuracao> Configuracoes { get; set; }

        public DbSet<NotaCalendario> NotasCalendario { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Assunto>()
                .HasOne(a => a.Disciplina)
                .WithMany(d => d.Assuntos)
                .HasForeignKey(a => a.DisciplinaId);
        }
    }
}