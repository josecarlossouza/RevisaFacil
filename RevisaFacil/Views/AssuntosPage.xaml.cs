// AssuntosPage.xaml.cs
// MODIFICAÇÕES PRINCIPAIS:
// 1. Exibe UMA disciplina por vez, escolhida pelo ComboBox cbDisciplina.
// 2. Lembra a última disciplina visualizada (salva em Configuracao.UltimaDisciplinaId).
// 3. Botões +/- e cabeçalhos de intervalo afetam SOMENTE a disciplina atual.
// 4. FiltrarPorDisciplina() permite navegação vinda do gráfico de pizza do MainView.

using RevisaFacil.Data;
using RevisaFacil.Helpers;
using RevisaFacil.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RevisaFacil.Views
{
    public partial class AssuntosPage : Page
    {
        // ── Estado ────────────────────────────────────────────────────────────────
        private Disciplina _disciplinaAtual;
        private int _qtdRevisoesAtual = 10;
        private bool _carregando = false; // evita loops em SelectionChanged

        public AssuntosPage()
        {
            InitializeComponent();
            CarregarComboEDisciplinaInicial();
            TemaManager.SincronizarCalendarioGlobal();
        }

        // ── Inicialização ─────────────────────────────────────────────────────────

        /// <summary>
        /// Popula o ComboBox de disciplinas e seleciona a última usada (ou a primeira).
        /// </summary>
        private void CarregarComboEDisciplinaInicial()
        {
            _carregando = true;
            try
            {
                using var db = new EstudoDbContext(TemaManager.GetDbPath());
                TemaManager.MigrarConfiguracoes();
                TemaManager.MigrarConfiguracoesDisciplinas();

                var disciplinas = db.Disciplinas.OrderBy(d => d.Nome).ToList();
                cbDisciplina.ItemsSource = disciplinas;

                if (!disciplinas.Any()) return;

                // Tenta carregar a última disciplina visualizada
                var config = db.Configuracoes.FirstOrDefault();
                int ultimaId = config?.UltimaDisciplinaId ?? 0;

                var selecionada = disciplinas.FirstOrDefault(d => d.Id == ultimaId)
                                  ?? disciplinas.First();

                cbDisciplina.SelectedItem = selecionada;
                // SelectionChanged não dispara ainda pois _carregando = true
                CarregarDisciplina(selecionada);
            }
            finally
            {
                _carregando = false;
            }
        }

        // ── Carregar disciplina ───────────────────────────────────────────────────

        private void CarregarDisciplina(Disciplina disciplina)
        {
            if (disciplina == null) return;

            using var db = new EstudoDbContext(TemaManager.GetDbPath());

            // Recarrega do banco para ter os intervalos atualizados
            _disciplinaAtual = db.Disciplinas
                .Include(d => d.Assuntos)
                .FirstOrDefault(d => d.Id == disciplina.Id);

            if (_disciplinaAtual == null) return;

            var config = db.Configuracoes.FirstOrDefault();

            // Quantidade de revisões: específica da disciplina, ou global
            _qtdRevisoesAtual = TemaManager.GetQuantidadeRevisoesDisciplina(_disciplinaAtual, config);
            txtQtdRevisoes.Text = _qtdRevisoesAtual.ToString();
            txtTituloDisciplina.Text = _disciplinaAtual.Nome;

            // Salva como última disciplina visualizada
            if (config != null)
            {
                config.UltimaDisciplinaId = _disciplinaAtual.Id;
                db.SaveChanges();
            }

            // Reconstrói colunas dinâmicas com os intervalos da disciplina
            ReconstruirColunasDinamicas(_disciplinaAtual, config);

            // Carrega assuntos desta disciplina
            AplicarFiltro(db);
        }

        private void AplicarFiltro(EstudoDbContext db = null)
        {
            bool criarDb = db == null;
            if (criarDb) db = new EstudoDbContext(TemaManager.GetDbPath());

            try
            {
                if (_disciplinaAtual == null) return;

                var query = db.Assuntos
                    .Include(a => a.Disciplina)
                    .Where(a => a.DisciplinaId == _disciplinaAtual.Id)
                    .AsQueryable();

                string busca = txtBuscaAssunto?.Text?.Trim() ?? "";
                if (!string.IsNullOrEmpty(busca))
                    query = query.Where(a => a.Titulo.ToLower().Contains(busca.ToLower()));

                dgAssuntos.ItemsSource = query.OrderBy(a => a.Titulo).ToList();
            }
            finally
            {
                if (criarDb) db?.Dispose();
            }
        }

        // ── Reconstrução das colunas dinâmicas ────────────────────────────────────

        private void ReconstruirColunasDinamicas(Disciplina disciplina, Configuracao config)
        {
            // Remove colunas antigas (mantém as 2 fixas: Assunto e Início)
            while (dgAssuntos.Columns.Count > 2)
                dgAssuntos.Columns.RemoveAt(dgAssuntos.Columns.Count - 1);

            for (int i = 1; i <= _qtdRevisoesAtual; i++)
            {
                int capturedI = i;
                int intervalo = TemaManager.GetIntervaloEfetivo(disciplina, config, i);

                // Cabeçalho editável com o intervalo de dias
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var lblPre = new TextBlock { Text = $"R{i} (", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
                var txtIntervalo = new TextBox
                {
                    Text = intervalo.ToString(),
                    Tag = capturedI,
                    Width = 32,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Background = Brushes.Transparent,
                    FontSize = 12,
                    ToolTip = i == 1
                        ? "Dias após a data de início"
                        : "Dias após a revisão anterior"
                };
                txtIntervalo.KeyDown += Intervalo_KeyDown;
                txtIntervalo.LostFocus += Intervalo_LostFocus;
                var lblPost = new TextBlock { Text = "d)", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
                headerPanel.Children.Add(lblPre);
                headerPanel.Children.Add(txtIntervalo);
                headerPanel.Children.Add(lblPost);

                // Célula: botão colorido com a data calculada
                var cellTemplate = new DataTemplate();
                var factory = new FrameworkElementFactory(typeof(Button));
                factory.SetBinding(Button.ContentProperty, new System.Windows.Data.Binding($"DataRev{capturedI}") { StringFormat = "{0:dd/MM/yyyy}" });
                factory.SetBinding(Button.BackgroundProperty, new System.Windows.Data.Binding($"Rev{capturedI}Concluida") { Converter = (System.Windows.Data.IValueConverter)Application.Current.Resources["StatusToColorConverter"] });
                factory.SetValue(Button.ForegroundProperty, Brushes.White);
                factory.SetValue(Button.MarginProperty, new Thickness(2));
                factory.SetValue(Button.TagProperty, capturedI);
                factory.SetValue(Button.FontSizeProperty, 12.0);
                factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(MarcarRevisao_Click));
                cellTemplate.VisualTree = factory;

                dgAssuntos.Columns.Add(new DataGridTemplateColumn
                {
                    Header = headerPanel,
                    CellTemplate = cellTemplate,
                    Width = new DataGridLength(110)
                });
            }

            // Coluna lixeira — sempre no final
            var deleteTemplate = new DataTemplate();
            var deleteFactory = new FrameworkElementFactory(typeof(Button));
            deleteFactory.SetValue(Button.ContentProperty, "🗑");
            deleteFactory.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(231, 76, 60)));
            deleteFactory.SetValue(Button.ForegroundProperty, Brushes.White);
            deleteFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            deleteFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnApagarAssunto_Click));
            deleteTemplate.VisualTree = deleteFactory;

            dgAssuntos.Columns.Add(new DataGridTemplateColumn
            {
                CellTemplate = deleteTemplate,
                Width = new DataGridLength(40)
            });
        }

        // ── Eventos de seleção de disciplina ─────────────────────────────────────

        private void CbDisciplina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_carregando) return;
            if (cbDisciplina.SelectedItem is Disciplina selecionada)
                CarregarDisciplina(selecionada);
        }

        /// <summary>
        /// Permite que o MainView navegue para cá filtrando por disciplina (clique no gráfico de pizza).
        /// </summary>
        public void FiltrarPorDisciplina(string nomeDisciplina)
        {
            _carregando = true;
            try
            {
                var disciplina = (cbDisciplina.ItemsSource as IEnumerable<Disciplina>)?
                    .FirstOrDefault(d => d.Nome.Equals(nomeDisciplina, StringComparison.OrdinalIgnoreCase));

                if (disciplina != null)
                {
                    cbDisciplina.SelectedItem = disciplina;
                    CarregarDisciplina(disciplina);
                }
            }
            finally
            {
                _carregando = false;
            }
        }

        // ── Botões +/- de revisões (somente para a disciplina atual) ─────────────

        private void BtnMaisRevisoes_Click(object sender, RoutedEventArgs e)
        {
            if (_qtdRevisoesAtual >= 30) return;
            _qtdRevisoesAtual++;
            SalvarQtdRevisoesDisciplina();
        }

        private void BtnMenosRevisoes_Click(object sender, RoutedEventArgs e)
        {
            if (_qtdRevisoesAtual <= 1) return;
            _qtdRevisoesAtual--;
            SalvarQtdRevisoesDisciplina();
        }

        private void SalvarQtdRevisoesDisciplina()
        {
            if (_disciplinaAtual == null) return;

            using var db = new EstudoDbContext(TemaManager.GetDbPath());
            var disc = db.Disciplinas.Find(_disciplinaAtual.Id);
            if (disc == null) return;

            disc.QuantidadeRevisoes = _qtdRevisoesAtual;
            db.SaveChanges();

            TemaManager.SincronizarCalendarioGlobal();
            CarregarDisciplina(_disciplinaAtual);
        }

        // ── Edição de intervalo no cabeçalho (somente para a disciplina atual) ───

        private void ProcessarMudancaIntervalo(TextBox tb)
        {
            if (tb == null || _disciplinaAtual == null) return;
            if (!int.TryParse(tb.Text, out int novoValor) || novoValor <= 0) return;

            int r = (int)tb.Tag;

            using var db = new EstudoDbContext(TemaManager.GetDbPath());

            // Atualiza o intervalo na disciplina
            var disc = db.Disciplinas.Find(_disciplinaAtual.Id);
            if (disc == null) return;
            disc.SetIntervalo(r, novoValor);

            // Aplica o intervalo em todos os assuntos desta disciplina
            var assuntos = db.Assuntos.Where(a => a.DisciplinaId == disc.Id).ToList();
            foreach (var a in assuntos)
                a.SetIntervalo(r, novoValor);

            db.SaveChanges();
            TemaManager.SincronizarCalendarioGlobal();
            CarregarDisciplina(_disciplinaAtual);
        }

        private void Intervalo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessarMudancaIntervalo(sender as TextBox);
                Keyboard.ClearFocus();
                e.Handled = true;
            }
        }

        private void Intervalo_LostFocus(object sender, RoutedEventArgs e)
            => ProcessarMudancaIntervalo(sender as TextBox);

        // ── Duplo clique na linha ─────────────────────────────────────────────────

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Assunto assunto)
            {
                var cell = FindParent<DataGridCell>(e.OriginalSource as FrameworkElement);
                if (cell?.Column?.Header?.ToString() is string h &&
                    (h == "Disciplina" || h == "Assunto"))
                {
                    using var db = new EstudoDbContext(TemaManager.GetDbPath());
                    db.Assuntos.Attach(assunto);
                    assunto.IsDestacado = !assunto.IsDestacado;
                    db.SaveChanges();
                    dgAssuntos.Items.Refresh();
                    e.Handled = true;
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T t) return t;
            return FindParent<T>(parent);
        }

        // ── Marcar revisão (toggle verde/vermelho) ────────────────────────────────

        private void MarcarRevisao_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Assunto assunto)
            {
                int r = (int)btn.Tag;
                using var db = new EstudoDbContext(TemaManager.GetDbPath());
                db.Assuntos.Attach(assunto);
                assunto.SetRevConcluida(r, !assunto.GetRevConcluida(r));
                db.SaveChanges();
                dgAssuntos.Items.Refresh();
            }
        }

        // ── Edição inline da data de início ──────────────────────────────────────

        private void DgAssuntos_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is Assunto assunto)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    using var db = new EstudoDbContext(TemaManager.GetDbPath());
                    db.Entry(assunto).State = EntityState.Modified;
                    db.SaveChanges();
                    TemaManager.SincronizarCalendarioGlobal();
                    dgAssuntos.Items.Refresh();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        // ── Excluir assunto ───────────────────────────────────────────────────────

        private void BtnApagarAssunto_Click(object sender, RoutedEventArgs e)
        {
            if (dgAssuntos.SelectedItem is Assunto sel)
            {
                if (MessageBox.Show($"Excluir '{sel.Titulo}'?", "Confirmação", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using var db = new EstudoDbContext(TemaManager.GetDbPath());
                    db.Assuntos.Remove(sel);
                    db.SaveChanges();
                    TemaManager.SincronizarCalendarioGlobal();
                    AplicarFiltro();
                }
            }
        }

        // ── Filtro de busca por texto ─────────────────────────────────────────────

        private void BuscaAssunto_Changed(object sender, TextChangedEventArgs e)
            => AplicarFiltro();

        private void LimparBusca_Click(object sender, RoutedEventArgs e)
        {
            txtBuscaAssunto.Clear();
            AplicarFiltro();
        }
    }
}
