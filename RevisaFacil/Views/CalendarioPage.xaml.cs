// CalendarioPage.xaml.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RevisaFacil.Data;
using RevisaFacil.Helpers;

namespace RevisaFacil.Views
{
    /// <summary>Item de dados para cada card do calendário.</summary>
    public class DiaCalendarioItem
    {
        public DateTime? Data { get; set; }

        /// <summary>True quando o card é um espaço vazio antes do dia 1 do mês.</summary>
        public bool EhVazio => !Data.HasValue;

        /// <summary>Exibe apenas o número do dia.</summary>
        public string DiaDaString => Data.HasValue ? Data.Value.Day.ToString() : "";

        /// <summary>Texto resumido exibido dentro do card.</summary>
        public string PreviewTexto { get; set; } = "";

        /// <summary>Texto completo para o ToolTip.</summary>
        public string TooltipTexto { get; set; } = "";

        /// <summary>Indica se este dia possui alguma nota salva no banco.</summary>
        public bool TemNota { get; set; } = false;
    }

    public enum ModoVisualizacao { Mes, Semana, Dia }

    public partial class CalendarioPage : Page
    {
        private DateTime _dataReferencia;
        private ModoVisualizacao _modo = ModoVisualizacao.Mes;

        /// <summary>
        /// HashSet usado pelo MultiBinding do XAML para colorir os cards que têm notas.
        /// </summary>
        public HashSet<DateTime> DatasComNotas { get; set; } = new HashSet<DateTime>();

        private Dictionary<DateTime, string> _notasPorData = new Dictionary<DateTime, string>();

        public CalendarioPage()
        {
            InitializeComponent();
            _dataReferencia = DateTime.Today;
            this.DataContext = this;
            AtualizarBotoesVisualizacao();
            GerarGrade();
        }

        // ── Seleção de modo de visualização ───────────────────────────────────────

        private void btnViewMes_Click(object sender, RoutedEventArgs e)
        {
            _modo = ModoVisualizacao.Mes;
            _dataReferencia = DateTime.Today;
            AtualizarBotoesVisualizacao();
            GerarGrade();
        }

        private void btnViewSemana_Click(object sender, RoutedEventArgs e)
        {
            _modo = ModoVisualizacao.Semana;
            _dataReferencia = DateTime.Today;
            AtualizarBotoesVisualizacao();
            GerarGrade();
        }

        private void btnViewDia_Click(object sender, RoutedEventArgs e)
        {
            _modo = ModoVisualizacao.Dia;
            _dataReferencia = DateTime.Today;
            AtualizarBotoesVisualizacao();
            GerarGrade();
        }

        private void AtualizarBotoesVisualizacao()
        {
            var ativo = new SolidColorBrush(Color.FromRgb(52, 73, 94));
            var normal = Brushes.White;
            var ativoFg = Brushes.White;
            var normalFg = new SolidColorBrush(Color.FromRgb(52, 73, 94));

            btnViewMes.Background = _modo == ModoVisualizacao.Mes ? ativo : normal;
            btnViewMes.Foreground = _modo == ModoVisualizacao.Mes ? ativoFg : normalFg;
            btnViewSemana.Background = _modo == ModoVisualizacao.Semana ? ativo : normal;
            btnViewSemana.Foreground = _modo == ModoVisualizacao.Semana ? ativoFg : normalFg;
            btnViewDia.Background = _modo == ModoVisualizacao.Dia ? ativo : normal;
            btnViewDia.Foreground = _modo == ModoVisualizacao.Dia ? ativoFg : normalFg;
        }

        // ── Navegação Anterior / Próximo ───────────────────────────────────────────

        private void btnAnterior_Click(object sender, RoutedEventArgs e)
        {
            _dataReferencia = _modo switch
            {
                ModoVisualizacao.Mes => _dataReferencia.AddMonths(-1),
                ModoVisualizacao.Semana => _dataReferencia.AddDays(-7),
                ModoVisualizacao.Dia => _dataReferencia.AddDays(-1),
                _ => _dataReferencia.AddMonths(-1)
            };
            GerarGrade();
        }

        private void btnProximo_Click(object sender, RoutedEventArgs e)
        {
            _dataReferencia = _modo switch
            {
                ModoVisualizacao.Mes => _dataReferencia.AddMonths(1),
                ModoVisualizacao.Semana => _dataReferencia.AddDays(7),
                ModoVisualizacao.Dia => _dataReferencia.AddDays(1),
                _ => _dataReferencia.AddMonths(1)
            };
            GerarGrade();
        }

        // ── Construção da grade ────────────────────────────────────────────────────

        private void GerarGrade()
        {
            CarregarNotas();
            AtualizarTitulo();

            // Mostra/oculta o cabeçalho dos dias da semana
            gridDiasSemana.Visibility = _modo == ModoVisualizacao.Dia
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Monta itens conforme o modo
            var itens = _modo switch
            {
                ModoVisualizacao.Mes => MontarItensMes(),
                ModoVisualizacao.Semana => MontarItensSemana(),
                ModoVisualizacao.Dia => MontarItensDia(),
                _ => MontarItensMes()
            };

            // Ajusta o número de colunas do UniformGrid interno
            int colunas = _modo == ModoVisualizacao.Dia ? 1 : 7;
            icDiasCalendario.UpdateLayout();
            var painel = FindVisualChild<UniformGrid>(icDiasCalendario);
            if (painel != null) painel.Columns = colunas;

            // Força refresh
            icDiasCalendario.ItemsSource = null;
            icDiasCalendario.ItemsSource = itens;
        }

        private void AtualizarTitulo()
        {
            var pt = new CultureInfo("pt-BR");
            txtMesAno.Text = _modo switch
            {
                ModoVisualizacao.Mes =>
                    pt.DateTimeFormat.GetMonthName(_dataReferencia.Month).ToUpper()
                    + $" DE {_dataReferencia.Year}",

                ModoVisualizacao.Semana =>
                    $"SEMANA DE {InicioSemana(_dataReferencia):dd/MM} A {FimSemana(_dataReferencia):dd/MM/yyyy}",

                ModoVisualizacao.Dia =>
                    _dataReferencia.ToString("dddd, dd 'de' MMMM 'de' yyyy", pt).ToUpper(),

                _ => ""
            };
        }

        private void CarregarNotas()
        {
            try
            {
                using (var db = new EstudoDbContext())
                {
                    DatasComNotas = new HashSet<DateTime>(
                        db.NotasCalendario.Select(n => n.Data.Date).ToList()
                    );

                    _notasPorData = db.NotasCalendario
                        .GroupBy(n => n.Data.Date)
                        .ToDictionary(
                            g => g.Key,
                            g => string.Join("\n", g
                                .Select(n => n.Conteudo)
                                .Where(c => !string.IsNullOrWhiteSpace(c)))
                        );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao carregar notas: " + ex.Message);
            }
        }

        // ── Montagem de itens ──────────────────────────────────────────────────────

        private DiaCalendarioItem CriarItem(DateTime? data)
        {
            if (!data.HasValue)
                return new DiaCalendarioItem { Data = null };

            bool temNota = DatasComNotas.Contains(data.Value.Date);
            string textoCompleto = _notasPorData.TryGetValue(data.Value.Date, out var t) ? t : "";

            return new DiaCalendarioItem
            {
                Data = data,
                TemNota = temNota,
                PreviewTexto = GerarPreview(textoCompleto),
                TooltipTexto = string.IsNullOrWhiteSpace(textoCompleto)
                    ? $"{data.Value:dd/MM/yyyy} — Sem anotações"
                    : $"📅 {data.Value:dd/MM/yyyy}\n\n{textoCompleto}"
            };
        }

        private static string GerarPreview(string textoCompleto)
        {
            if (string.IsNullOrWhiteSpace(textoCompleto)) return "";

            var linhas = textoCompleto.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var previews = linhas.Select(l =>
            {
                // Extrai apenas o nome do assunto para uma prévia curta
                var partes = l.Split(new[] { "- Assunto: " }, StringSplitOptions.None);
                if (partes.Length > 1)
                {
                    var assunto = partes[1].Split(new[] { " - " }, StringSplitOptions.None)[0];
                    return "* " + (assunto.Length > 22 ? assunto.Substring(0, 22) + "…" : assunto);
                }
                return "* " + (l.Length > 25 ? l.Substring(0, 25) + "…" : l);
            });

            return string.Join("\n", previews.Take(4));
        }

        private List<DiaCalendarioItem> MontarItensMes()
        {
            var lista = new List<DiaCalendarioItem>();
            var primeiro = new DateTime(_dataReferencia.Year, _dataReferencia.Month, 1);

            // Espaços vazios até o dia da semana em que o mês começa
            for (int i = 0; i < (int)primeiro.DayOfWeek; i++)
                lista.Add(CriarItem(null));

            int dias = DateTime.DaysInMonth(_dataReferencia.Year, _dataReferencia.Month);
            for (int d = 1; d <= dias; d++)
                lista.Add(CriarItem(new DateTime(_dataReferencia.Year, _dataReferencia.Month, d)));

            return lista;
        }

        private List<DiaCalendarioItem> MontarItensSemana()
        {
            var lista = new List<DiaCalendarioItem>();
            var inicio = InicioSemana(_dataReferencia);
            for (int i = 0; i < 7; i++)
                lista.Add(CriarItem(inicio.AddDays(i)));
            return lista;
        }

        private List<DiaCalendarioItem> MontarItensDia()
            => new List<DiaCalendarioItem> { CriarItem(_dataReferencia) };

        private static DateTime InicioSemana(DateTime data)
        {
            int diff = (7 + (data.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return data.AddDays(-diff).Date;
        }

        private static DateTime FimSemana(DateTime data) => InicioSemana(data).AddDays(6);

        // ── Clique duplo no card → abre PopUp ─────────────────────────────────────

        private void BorderDia_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2 || e.ChangedButton != MouseButton.Left) return;

            if (sender is Border card && card.Tag is DiaCalendarioItem item && item.Data.HasValue)
            {
                var popUp = new PopUpNotaWindow(item.Data.Value);
                popUp.Owner = Window.GetWindow(this);
                if (popUp.ShowDialog() == true)
                    GerarGrade();
            }
        }

        // ── Utilitário: encontrar filho visual ─────────────────────────────────────

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result) return result;
                var found = FindVisualChild<T>(child);
                if (found != null) return found;
            }
            return null;
        }
    }
}
