using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using RevisaFacil.Data;
using RevisaFacil.Helpers;
using LiveCharts;
using LiveCharts.Wpf;

namespace RevisaFacil.Views
{
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
                    // Força o carregamento dos dados incluindo a disciplina para o gráfico
                    var todosAssuntos = db.Assuntos.ToList();

                    // 1. Cálculos de Cards (Batendo com os x:Name do seu XAML)
                    int total = todosAssuntos.Count;

                    // Um assunto é concluído se as 5 revisões forem True
                    int concluidos = todosAssuntos.Count(a =>
                        a.Rev1Concluida && a.Rev2Concluida && a.Rev3Concluida &&
                        a.Rev4Concluida && a.Rev5Concluida);

                    int pendentes = total - concluidos;

                    // ATENÇÃO: Nomes ajustados para o seu XAML atual
                    txtTotalAssuntos.Text = total.ToString();
                    txtConcluidos.Text = concluidos.ToString();
                    txtPendentes.Text = pendentes.ToString();

                    // 2. Gráfico de Pizza por Disciplina
                    SeriesCollection = new SeriesCollection();

                    var dadosAgrupados = todosAssuntos
                        .GroupBy(a => a.Disciplina.Nome)
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao carregar MainView: " + ex.Message);
            }
        }
    }
}