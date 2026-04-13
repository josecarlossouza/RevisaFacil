// MainView.xaml.cs
// CORREÇÕES:
//   1. txtLabelConcluidos atualizado dinamicamente com a qtdRev lida do banco
//   2. Cálculo de "Concluídos", "Revisões Hoje", "Atrasadas" e "Concluídas Hoje"
//      agora usa qtdRev em vez de 5 fixo
//   3. Tabela de estatísticas por disciplina também usa qtdRev

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
                    // ── Quantidade de revisões configurada pelo usuário ────────────
                    // ✅ CORREÇÃO PRINCIPAL: lê qtdRev com fallback seguro caso a coluna
                    //    ainda não exista no banco (evita crash durante a migração).
                    int qtdRev = TemaManager.GetQuantidadeRevisoes();

                    var todosAssuntos = db.Assuntos.ToList();
                    var hoje = DateTime.Today;

                    // ── Cards de resumo geral ──────────────────────────────────────
                    int total = todosAssuntos.Count;

                    // Um assunto é considerado "concluído" quando TODAS as qtdRev revisões
                    // estão marcadas como concluídas.
                    int concluidos = todosAssuntos.Count(a =>
                    {
                        for (int i = 1; i <= qtdRev; i++)
                            if (!a.GetRevConcluida(i)) return false;
                        return true;
                    });
                    int pendentes = total - concluidos;

                    txtTotalAssuntos.Text = total.ToString();
                    txtConcluidos.Text = concluidos.ToString();
                    txtPendentes.Text = pendentes.ToString();

                    // ✅ LABEL DINÂMICO: acompanha a quantidade de revisões do usuário
                    txtLabelConcluidos.Text = $"Concluídos ({qtdRev} Revs)";

                    // ── Card de resumo do dia ──────────────────────────────────────
                    int revisoesHoje = todosAssuntos.Sum(a =>
                    {
                        int count = 0;
                        for (int i = 1; i <= qtdRev; i++)
                            if (a.GetDataRev(i).Date == hoje && !a.GetRevConcluida(i)) count++;
                        return count;
                    });

                    int atrasadas = todosAssuntos.Sum(a =>
                    {
                        int count = 0;
                        for (int i = 1; i <= qtdRev; i++)
                            if (a.GetDataRev(i).Date < hoje && !a.GetRevConcluida(i)) count++;
                        return count;
                    });

                    int concluidasHoje = todosAssuntos.Sum(a =>
                    {
                        int count = 0;
                        for (int i = 1; i <= qtdRev; i++)
                            if (a.GetDataRev(i).Date == hoje && a.GetRevConcluida(i)) count++;
                        return count;
                    });

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
                        int totalRevisoesPossiveis = totalAssuntos * qtdRev;

                        int revisoesConcluidasTotal = assuntos.Sum(a =>
                        {
                            int c = 0;
                            for (int i = 1; i <= qtdRev; i++)
                                if (a.GetRevConcluida(i)) c++;
                            return c;
                        });

                        double taxa = totalRevisoesPossiveis > 0
                            ? (revisoesConcluidasTotal * 100.0) / totalRevisoesPossiveis
                            : 0;

                        var diasDeAtraso = new List<double>();
                        foreach (var a in assuntos)
                        {
                            for (int i = 1; i <= qtdRev; i++)
                            {
                                if (a.GetDataRev(i).Date < hoje && !a.GetRevConcluida(i))
                                    diasDeAtraso.Add((hoje - a.GetDataRev(i).Date).TotalDays);
                            }
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
