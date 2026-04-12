using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using RevisaFacil.Data;
using RevisaFacil.Helpers;
using LiveCharts;
using LiveCharts.Wpf;

namespace RevisaFacil.Views
{
    // Classe de dados para cada linha da tabela de estatísticas
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
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    var todosAssuntos = db.Assuntos.ToList();
                    var hoje = DateTime.Today;

                    // ── Cards de resumo geral ──────────────────────────────────────
                    int total = todosAssuntos.Count;
                    int concluidos = todosAssuntos.Count(a =>
                        a.Rev1Concluida && a.Rev2Concluida && a.Rev3Concluida &&
                        a.Rev4Concluida && a.Rev5Concluida);
                    int pendentes = total - concluidos;

                    txtTotalAssuntos.Text = total.ToString();
                    txtConcluidos.Text = concluidos.ToString();
                    txtPendentes.Text = pendentes.ToString();

                    // ── Card de resumo do dia ──────────────────────────────────────
                    int revisoesHoje = todosAssuntos.Sum(a =>
                        (a.DataRev1.Date == hoje && !a.Rev1Concluida ? 1 : 0) +
                        (a.DataRev2.Date == hoje && !a.Rev2Concluida ? 1 : 0) +
                        (a.DataRev3.Date == hoje && !a.Rev3Concluida ? 1 : 0) +
                        (a.DataRev4.Date == hoje && !a.Rev4Concluida ? 1 : 0) +
                        (a.DataRev5.Date == hoje && !a.Rev5Concluida ? 1 : 0));

                    int atrasadas = todosAssuntos.Sum(a =>
                        (a.DataRev1.Date < hoje && !a.Rev1Concluida ? 1 : 0) +
                        (a.DataRev2.Date < hoje && !a.Rev2Concluida ? 1 : 0) +
                        (a.DataRev3.Date < hoje && !a.Rev3Concluida ? 1 : 0) +
                        (a.DataRev4.Date < hoje && !a.Rev4Concluida ? 1 : 0) +
                        (a.DataRev5.Date < hoje && !a.Rev5Concluida ? 1 : 0));

                    int concluidasHoje = todosAssuntos.Sum(a =>
                        (a.DataRev1.Date == hoje && a.Rev1Concluida ? 1 : 0) +
                        (a.DataRev2.Date == hoje && a.Rev2Concluida ? 1 : 0) +
                        (a.DataRev3.Date == hoje && a.Rev3Concluida ? 1 : 0) +
                        (a.DataRev4.Date == hoje && a.Rev4Concluida ? 1 : 0) +
                        (a.DataRev5.Date == hoje && a.Rev5Concluida ? 1 : 0));

                    txtRevisoesHoje.Text = revisoesHoje.ToString();
                    txtRevisoesAtrasadas.Text = atrasadas.ToString();
                    txtConcluidasHoje.Text = concluidasHoje.ToString();

                    // ── Gráfico de Pizza por Disciplina ───────────────────────────
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
                            LabelPoint = chartPoint => $"{chartPoint.Y} ({chartPoint.Participation:P0})"
                        });
                    }
                    chartDisciplinas.Series = SeriesCollection;

                    // ── Tabela de estatísticas por disciplina ──────────────────────
                    var estatisticas = new List<EstatisticaDisciplina>();

                    var porDisciplina = todosAssuntos
                        .GroupBy(a => a.Disciplina?.Nome ?? "Sem Disciplina");

                    foreach (var grupo in porDisciplina.OrderBy(g => g.Key))
                    {
                        var assuntos = grupo.ToList();
                        int totalAssuntos = assuntos.Count;
                        int totalRevisoesPossiveis = totalAssuntos * 5;

                        int revisoesConcluidasTotal =
                            assuntos.Count(a => a.Rev1Concluida) +
                            assuntos.Count(a => a.Rev2Concluida) +
                            assuntos.Count(a => a.Rev3Concluida) +
                            assuntos.Count(a => a.Rev4Concluida) +
                            assuntos.Count(a => a.Rev5Concluida);

                        double taxa = totalRevisoesPossiveis > 0
                            ? (revisoesConcluidasTotal * 100.0) / totalRevisoesPossiveis
                            : 0;

                        var diasDeAtraso = new List<double>();
                        foreach (var a in assuntos)
                        {
                            if (a.DataRev1.Date < hoje && !a.Rev1Concluida) diasDeAtraso.Add((hoje - a.DataRev1.Date).TotalDays);
                            if (a.DataRev2.Date < hoje && !a.Rev2Concluida) diasDeAtraso.Add((hoje - a.DataRev2.Date).TotalDays);
                            if (a.DataRev3.Date < hoje && !a.Rev3Concluida) diasDeAtraso.Add((hoje - a.DataRev3.Date).TotalDays);
                            if (a.DataRev4.Date < hoje && !a.Rev4Concluida) diasDeAtraso.Add((hoje - a.DataRev4.Date).TotalDays);
                            if (a.DataRev5.Date < hoje && !a.Rev5Concluida) diasDeAtraso.Add((hoje - a.DataRev5.Date).TotalDays);
                        }

                        double atrasoMedio = diasDeAtraso.Count > 0 ? diasDeAtraso.Average() : 0;

                        estatisticas.Add(new EstatisticaDisciplina
                        {
                            NomeDisciplina = grupo.Key,
                            TotalAssuntos = totalAssuntos,
                            TaxaConclusao = taxa,
                            AtrasoMedioDias = atrasoMedio,
                            RevisoesAtrasadas = diasDeAtraso.Count
                        });
                    }

                    dgEstatisticas.ItemsSource = estatisticas;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao carregar MainView: " + ex.Message);
            }
        }
    }
}