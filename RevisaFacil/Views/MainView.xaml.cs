// MainView.xaml.cs — v1.3.0
// ALTERAÇÕES:
// 1. ChartDisciplinas_DataClick: usa Window.GetWindow() para chamar
//    MainWindow.NavegaParaRevisoes(), que faz a navegação via MainFrame.
//    Isso resolve o problema de NavigationService ser null numa Page carregada em Frame.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using RevisaFacil.Data;
using RevisaFacil.Helpers;
using LiveCharts;
using LiveCharts.Wpf;

namespace RevisaFacil.Views
{
    // ── Modelo de dados para a tabela de estatísticas por disciplina ──────────────
    public class EstatisticaDisciplina
    {
        public string NomeDisciplina { get; set; }
        public int TotalAssuntos { get; set; }

        public double TaxaConclusao { get; set; }
        public string TaxaConclusaoTexto => $"{TaxaConclusao:0}%";

        public Brush CorTaxa
        {
            get
            {
                if (TaxaConclusao >= 70) return new SolidColorBrush(Color.FromRgb(39, 174, 96));
                if (TaxaConclusao >= 40) return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                return new SolidColorBrush(Color.FromRgb(192, 57, 43));
            }
        }

        public double LarguraBarra => TaxaConclusao / 100.0 * 118.0;

        public double AtrasoMedioDias { get; set; }
        public string AtrasoMedioTexto => AtrasoMedioDias > 0 ? $"{AtrasoMedioDias:0} dias" : "Em dia ✓";

        public Brush CorAtraso
        {
            get
            {
                if (AtrasoMedioDias == 0) return new SolidColorBrush(Color.FromRgb(39, 174, 96));
                if (AtrasoMedioDias <= 7) return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                return new SolidColorBrush(Color.FromRgb(192, 57, 43));
            }
        }

        public string IconeAtraso => AtrasoMedioDias == 0 ? "" : AtrasoMedioDias <= 7 ? "⚠️" : "🔴";
        public int RevisoesAtrasadas { get; set; }
    }

    // ── Page ──────────────────────────────────────────────────────────────────────
    public partial class MainView : Page
    {
        public SeriesCollection SeriesCollection { get; set; }

        public MainView()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                using var db = new EstudoDbContext(TemaManager.GetDbPath());

                int qtdRev = TemaManager.GetQuantidadeRevisoes();
                var todosAssuntos = db.Assuntos.ToList();
                var hoje = DateTime.Today;

                // ── Cards de resumo geral ─────────────────────────────────────────
                int total = todosAssuntos.Count;

                int concluidos = todosAssuntos.Count(a =>
                {
                    for (int i = 1; i <= qtdRev; i++)
                        if (!a.GetRevConcluida(i)) return false;
                    return true;
                });

                txtTotalAssuntos.Text = total.ToString();
                txtConcluidos.Text = concluidos.ToString();
                txtPendentes.Text = (total - concluidos).ToString();
                txtLabelConcluidos.Text = $"Concluídos ({qtdRev} Revs)";

                // ── Card do dia ───────────────────────────────────────────────────
                int revisoesHoje = todosAssuntos.Sum(a =>
                {
                    int c = 0;
                    for (int i = 1; i <= qtdRev; i++)
                        if (a.GetDataRev(i).Date == hoje && !a.GetRevConcluida(i)) c++;
                    return c;
                });

                int atrasadas = todosAssuntos.Sum(a =>
                {
                    int c = 0;
                    for (int i = 1; i <= qtdRev; i++)
                        if (a.GetDataRev(i).Date < hoje && !a.GetRevConcluida(i)) c++;
                    return c;
                });

                int concluidasHoje = todosAssuntos.Sum(a =>
                {
                    int c = 0;
                    for (int i = 1; i <= qtdRev; i++)
                        if (a.GetDataRev(i).Date == hoje && a.GetRevConcluida(i)) c++;
                    return c;
                });

                txtRevisoesHoje.Text = revisoesHoje.ToString();
                txtRevisoesAtrasadas.Text = atrasadas.ToString();
                txtConcluidasHoje.Text = concluidasHoje.ToString();

                // ── Gráfico de pizza ──────────────────────────────────────────────
                SeriesCollection = new SeriesCollection();

                var dadosAgrupados = todosAssuntos
                    .GroupBy(a => a.Disciplina?.Nome ?? "Sem Disciplina")
                    .Select(g => new { Nome = g.Key, Qtd = g.Count() });

                foreach (var item in dadosAgrupados)
                {
                    SeriesCollection.Add(new PieSeries
                    {
                        Title = item.Nome,
                        Values = new ChartValues<int> { item.Qtd },
                        DataLabels = true,
                        LabelPoint = cp => $"{cp.Y} ({cp.Participation:P0})"
                    });
                }

                chartDisciplinas.Series = SeriesCollection;

                // ── Tabela de estatísticas por disciplina ─────────────────────────
                var estatisticas = new List<EstatisticaDisciplina>();

                foreach (var grupo in todosAssuntos.GroupBy(a => a.Disciplina?.Nome ?? "Sem Disciplina").OrderBy(g => g.Key))
                {
                    var assuntos = grupo.ToList();
                    int totalPossivel = assuntos.Count * qtdRev;

                    int concluidsGrupo = assuntos.Sum(a =>
                    {
                        int c = 0;
                        for (int i = 1; i <= qtdRev; i++)
                            if (a.GetRevConcluida(i)) c++;
                        return c;
                    });

                    double taxa = totalPossivel > 0 ? (concluidsGrupo * 100.0) / totalPossivel : 0;

                    var diasAtraso = new List<double>();
                    foreach (var a in assuntos)
                        for (int i = 1; i <= qtdRev; i++)
                            if (a.GetDataRev(i).Date < hoje && !a.GetRevConcluida(i))
                                diasAtraso.Add((hoje - a.GetDataRev(i).Date).TotalDays);

                    estatisticas.Add(new EstatisticaDisciplina
                    {
                        NomeDisciplina = grupo.Key,
                        TotalAssuntos = assuntos.Count,
                        TaxaConclusao = taxa,
                        AtrasoMedioDias = diasAtraso.Count > 0 ? diasAtraso.Average() : 0,
                        RevisoesAtrasadas = diasAtraso.Count
                    });
                }

                dgEstatisticas.ItemsSource = estatisticas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao carregar MainView: " + ex.Message);
            }
        }

        // ── Clique no gráfico de pizza ────────────────────────────────────────────

        /// <summary>
        /// DataClick do LiveCharts: dispara ao clicar em qualquer fatia do gráfico.
        /// Delega a navegação para MainWindow.NavegaParaRevisoes() porque
        /// NavigationService é null quando a Page está dentro de um Frame filho.
        /// </summary>
        private void ChartDisciplinas_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            try
            {
                string nomeDisciplina = chartPoint.SeriesView?.Title;
                if (string.IsNullOrEmpty(nomeDisciplina)) return;

                // Obtém a MainWindow pai e delega a navegação
                if (Window.GetWindow(this) is MainWindow mainWindow)
                    mainWindow.NavegaParaRevisoes(nomeDisciplina);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro no clique do gráfico: " + ex.Message);
            }
        }
    }
}
