// EstatisticasPage.xaml.cs
// CORREÇÃO: NullReferenceException em AplicarFiltros() resolvido com guards de null
// em todos os controles (txtBusca, cbFiltroDisciplina, cbFiltroDesempenho,
// dgEstatisticasAssuntos) antes de qualquer acesso às suas propriedades.
// Também substituído acesso direto a db.Configuracoes por TemaManager.GetQuantidadeRevisoes(),
// que já executa MigrarConfiguracoes() internamente.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using RevisaFacil.Data;
using RevisaFacil.Helpers;
using RevisaFacil.Models;

namespace RevisaFacil.Views
{
    // ── Modelo de dados para a tabela de estatísticas por assunto ─────────────────
    public class EstatisticaAssunto
    {
        public int Id { get; set; }
        public string NomeDisciplina { get; set; }
        public string Titulo { get; set; }
        public DateTime DataInicio { get; set; }
        public bool IsDestacado { get; set; }
        public int QtdRevisoes { get; set; }
        public int RevisoesConcluidas { get; set; }
        public double AtrasoMaxDias { get; set; }

        public double Taxa => QtdRevisoes > 0 ? (RevisoesConcluidas * 100.0) / QtdRevisoes : 0;

        public string TaxaTexto => $"{Taxa:0}%";

        public Brush CorTaxa
        {
            get
            {
                if (Taxa >= 70) return new SolidColorBrush(Color.FromRgb(39, 174, 96));
                if (Taxa >= 30) return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                return new SolidColorBrush(Color.FromRgb(192, 57, 43));
            }
        }

        // Largura da barra de progresso (máx 140 px)
        public double LarguraBarra => Taxa / 100.0 * 140.0;

        public double AtrasoMaxDiasVal { get; set; }
        public string AtrasoMaxTexto => AtrasoMaxDias > 0 ? $"{AtrasoMaxDias:0}" : "Em dia";

        public Brush CorAtraso
        {
            get
            {
                if (AtrasoMaxDias == 0) return new SolidColorBrush(Color.FromRgb(39, 174, 96));
                if (AtrasoMaxDias <= 7) return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                return new SolidColorBrush(Color.FromRgb(192, 57, 43));
            }
        }

        public string IconeStatus
        {
            get
            {
                if (IsDestacado) return "🟢";
                if (Taxa >= 70) return "✅";
                if (Taxa >= 30) return "⚠️";
                return "🔴";
            }
        }
    }

    // ── Page ──────────────────────────────────────────────────────────────────────
    public partial class EstatisticasPage : Page
    {
        private List<EstatisticaAssunto> _todosItens = new List<EstatisticaAssunto>();

        public EstatisticasPage()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                // ✅ GetQuantidadeRevisoes() já executa MigrarConfiguracoes() internamente
                int qtdRev = TemaManager.GetQuantidadeRevisoes();
                var hoje = DateTime.Today;

                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    var assuntos = db.Assuntos.Include(a => a.Disciplina).ToList();

                    _todosItens = assuntos.Select(a =>
                    {
                        int concluidas = 0;
                        double atrasoMax = 0;

                        for (int i = 1; i <= qtdRev; i++)
                        {
                            bool c = a.GetRevConcluida(i);
                            DateTime dr = a.GetDataRev(i).Date;

                            if (c) concluidas++;
                            else if (dr < hoje)
                            {
                                double dias = (hoje - dr).TotalDays;
                                if (dias > atrasoMax) atrasoMax = dias;
                            }
                        }

                        return new EstatisticaAssunto
                        {
                            Id = a.Id,
                            NomeDisciplina = a.Disciplina?.Nome ?? "Sem Disciplina",
                            Titulo = a.Titulo,
                            DataInicio = a.DataInicio,
                            IsDestacado = a.IsDestacado,
                            QtdRevisoes = qtdRev,
                            RevisoesConcluidas = concluidas,
                            AtrasoMaxDias = atrasoMax
                        };
                    }).ToList();

                    // Cards de resumo — acesso seguro com verificação de null
                    if (txtMuitoEstudados != null) txtMuitoEstudados.Text = _todosItens.Count(x => x.Taxa >= 70).ToString();
                    if (txtModerados != null) txtModerados.Text = _todosItens.Count(x => x.Taxa >= 30 && x.Taxa < 70).ToString();
                    if (txtPoucoEstudados != null) txtPoucoEstudados.Text = _todosItens.Count(x => x.Taxa < 30).ToString();
                    if (txtIniciados != null) txtIniciados.Text = _todosItens.Count(x => x.IsDestacado).ToString();

                    // Carrega disciplinas no filtro — acesso seguro
                    if (cbFiltroDisciplina != null)
                        cbFiltroDisciplina.ItemsSource = db.Disciplinas.OrderBy(d => d.Nome).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro em CarregarDados (EstatisticasPage): " + ex.Message);
            }

            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            // ✅ CORREÇÃO PRINCIPAL: guards de null em todos os controles XAML
            // Evita NullReferenceException se AplicarFiltros() for chamado antes
            // de InitializeComponent() vincular os controles, ou se a página não
            // estiver completamente carregada.
            if (dgEstatisticasAssuntos == null) return;

            var filtrado = _todosItens.AsEnumerable();

            // Filtro por disciplina
            if (cbFiltroDisciplina?.SelectedValue is int idDisc)
            {
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    var nomDisc = db.Disciplinas.Find(idDisc)?.Nome;
                    if (nomDisc != null)
                        filtrado = filtrado.Where(x => x.NomeDisciplina == nomDisc);
                }
            }

            // Filtro por desempenho
            if (cbFiltroDesempenho?.SelectedItem is ComboBoxItem cbi)
            {
                string content = cbi.Content?.ToString() ?? "";
                if (content.Contains("Muito revisados")) filtrado = filtrado.Where(x => x.Taxa >= 70);
                else if (content.Contains("Em progresso")) filtrado = filtrado.Where(x => x.Taxa >= 30 && x.Taxa < 70);
                else if (content.Contains("Pouco revisados")) filtrado = filtrado.Where(x => x.Taxa < 30);
                else if (content.Contains("Iniciados")) filtrado = filtrado.Where(x => x.IsDestacado);
            }

            // ✅ Filtro por texto — null-safe com operador ?.
            string buscaTexto = txtBusca?.Text ?? "";
            if (!string.IsNullOrWhiteSpace(buscaTexto))
            {
                string busca = buscaTexto.ToLower();
                filtrado = filtrado.Where(x =>
                    (x.Titulo?.ToLower().Contains(busca) ?? false) ||
                    (x.NomeDisciplina?.ToLower().Contains(busca) ?? false));
            }

            // Ordena: menor taxa primeiro, depois maior atraso
            dgEstatisticasAssuntos.ItemsSource = filtrado
                .OrderBy(x => x.Taxa)
                .ThenByDescending(x => x.AtrasoMaxDias)
                .ToList();
        }

        private void Filtro_Changed(object sender, EventArgs e) => AplicarFiltros();

        private void LimparFiltros_Click(object sender, RoutedEventArgs e)
        {
            if (cbFiltroDisciplina != null) cbFiltroDisciplina.SelectedIndex = -1;
            if (cbFiltroDesempenho != null) cbFiltroDesempenho.SelectedIndex = 0;
            txtBusca?.Clear();
            AplicarFiltros();
        }
    }
}