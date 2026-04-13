// AssuntosPage.xaml.cs
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
        private int _qtdRevisoes = 10;

        public AssuntosPage()
        {
            InitializeComponent();
            CarregarDados();
            TemaManager.SincronizarCalendarioGlobal();
        }

        private void CarregarDados()
        {
            using (var db = new EstudoDbContext())
            {
                // Garante que a configuração existe
                var config = db.Configuracoes.FirstOrDefault();
                if (config == null)
                {
                    config = new Configuracao { QuantidadeRevisoes = 10 };
                    // Define intervalos padrão 30 em 30
                    for (int i = 1; i <= 30; i++) config.SetIntervalo(i, i * 30);
                    db.Configuracoes.Add(config);
                    db.SaveChanges();
                }
                // Garante que QuantidadeRevisoes existe (migração de banco antigo)
                if (config.QuantidadeRevisoes < 1) config.QuantidadeRevisoes = 10;

                _qtdRevisoes = config.QuantidadeRevisoes;
                txtQtdRevisoes.Text = _qtdRevisoes.ToString();

                // Atualiza as colunas dinâmicas
                ReconstruirColunasDinamicas(config);

                cbFiltroDisciplina.ItemsSource = db.Disciplinas.OrderBy(d => d.Nome).ToList();
                dgAssuntos.ItemsSource = db.Assuntos.Include(a => a.Disciplina).ToList();
            }
        }

        /// <summary>
        /// Remove todas as colunas de revisão e adiciona novamente de acordo com _qtdRevisoes.
        /// As 3 primeiras colunas (Disciplina, Assunto, Início) são fixas — mantemos apenas elas.
        /// </summary>
        private void ReconstruirColunasDinamicas(Configuracao config)
        {
            // Remove colunas antigas de revisão (índices >= 3 até penúltima — o botão lixo é o último)
            while (dgAssuntos.Columns.Count > 3)
                dgAssuntos.Columns.RemoveAt(dgAssuntos.Columns.Count - 1);

            for (int i = 1; i <= _qtdRevisoes; i++)
            {
                int capturedI = i;
                int intervalo = config.GetIntervalo(i);

                // Cabeçalho com TextBox para editar intervalo
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var lblPre = new TextBlock { Text = $"R{i} (", VerticalAlignment = VerticalAlignment.Center };
                var txtIntervalo = new TextBox
                {
                    Text = intervalo.ToString(),
                    Tag = capturedI,
                    Width = 28,
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Background = Brushes.Transparent
                };
                txtIntervalo.KeyDown += Intervalo_KeyDown;
                txtIntervalo.LostFocus += Intervalo_LostFocus;
                var lblPost = new TextBlock { Text = "d)", VerticalAlignment = VerticalAlignment.Center };
                headerPanel.Children.Add(lblPre);
                headerPanel.Children.Add(txtIntervalo);
                headerPanel.Children.Add(lblPost);

                // Template da célula com botão colorido
                var cellTemplate = new DataTemplate();
                var factory = new FrameworkElementFactory(typeof(Button));
                factory.SetBinding(Button.ContentProperty, new System.Windows.Data.Binding($"DataRev{capturedI}")
                {
                    StringFormat = "{0:dd/MM/yyyy}"
                });
                factory.SetBinding(Button.BackgroundProperty, new System.Windows.Data.Binding($"Rev{capturedI}Concluida")
                {
                    Converter = (System.Windows.Data.IValueConverter)Application.Current.Resources["StatusToColorConverter"]
                });
                factory.SetValue(Button.ForegroundProperty, Brushes.White);
                factory.SetValue(Button.MarginProperty, new Thickness(2));
                factory.SetValue(Button.TagProperty, capturedI);
                factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(MarcarRevisao_Click));
                cellTemplate.VisualTree = factory;

                var col = new DataGridTemplateColumn
                {
                    Header = headerPanel,
                    CellTemplate = cellTemplate,
                    Width = new DataGridLength(105)
                };

                dgAssuntos.Columns.Add(col);
            }

            // Coluna do botão excluir — sempre no final
            var deleteTemplate = new DataTemplate();
            var deleteFactory = new FrameworkElementFactory(typeof(Button));
            deleteFactory.SetValue(Button.ContentProperty, "🗑");
            deleteFactory.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(231, 76, 60)));
            deleteFactory.SetValue(Button.ForegroundProperty, Brushes.White);
            deleteFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            deleteFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(btnApagarAssunto_Click));
            deleteTemplate.VisualTree = deleteFactory;

            dgAssuntos.Columns.Add(new DataGridTemplateColumn
            {
                CellTemplate = deleteTemplate,
                Width = new DataGridLength(40)
            });
        }

        // ── Botões +/- de revisões ──────────────────────────────────────────────

        private void btnMaisRevisoes_Click(object sender, RoutedEventArgs e)
        {
            if (_qtdRevisoes >= 30) return;
            _qtdRevisoes++;
            SalvarQuantidadeRevisoes();
            CarregarDados();
        }

        private void btnMenosRevisoes_Click(object sender, RoutedEventArgs e)
        {
            if (_qtdRevisoes <= 1) return;
            _qtdRevisoes--;
            SalvarQuantidadeRevisoes();
            CarregarDados();
        }

        private void SalvarQuantidadeRevisoes()
        {
            using (var db = new EstudoDbContext())
            {
                var config = db.Configuracoes.First();
                config.QuantidadeRevisoes = _qtdRevisoes;
                db.SaveChanges();
            }
            TemaManager.SincronizarCalendarioGlobal();
        }

        // ── Duplo clique ────────────────────────────────────────────────────────

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Assunto assunto)
            {
                var cell = FindParent<DataGridCell>(e.OriginalSource as FrameworkElement);
                if (cell != null && cell.Column.Header != null)
                {
                    string h = cell.Column.Header.ToString();
                    if (h == "Disciplina" || h == "Assunto")
                    {
                        using (var db = new EstudoDbContext())
                        {
                            db.Assuntos.Attach(assunto);
                            assunto.IsDestacado = !assunto.IsDestacado;
                            db.SaveChanges();
                        }
                        dgAssuntos.Items.Refresh();
                        e.Handled = true;
                    }
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }

        // ── Marcar revisão (toggle verde/vermelho) ──────────────────────────────

        private void MarcarRevisao_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Assunto assunto)
            {
                int r = (int)btn.Tag;
                using (var db = new EstudoDbContext())
                {
                    db.Assuntos.Attach(assunto);
                    assunto.SetRevConcluida(r, !assunto.GetRevConcluida(r));
                    db.SaveChanges();
                }
                dgAssuntos.Items.Refresh();
            }
        }

        // ── Edição de intervalo ─────────────────────────────────────────────────

        private void ProcessarMudancaIntervalo(TextBox tb)
        {
            if (tb != null && int.TryParse(tb.Text, out int novo) && novo > 0)
            {
                int r = (int)tb.Tag;
                using (var db = new EstudoDbContext())
                {
                    var config = db.Configuracoes.First();
                    config.SetIntervalo(r, novo);

                    foreach (var a in db.Assuntos)
                        a.SetIntervalo(r, novo);

                    db.SaveChanges();
                }
                TemaManager.SincronizarCalendarioGlobal();
                CarregarDados();
            }
        }

        private void dgAssuntos_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is Assunto assunto)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    using (var db = new EstudoDbContext())
                    {
                        db.Entry(assunto).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    TemaManager.SincronizarCalendarioGlobal();
                    dgAssuntos.Items.Refresh();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void btnApagarAssunto_Click(object sender, RoutedEventArgs e)
        {
            if (dgAssuntos.SelectedItem is Assunto sel)
            {
                if (MessageBox.Show($"Excluir '{sel.Titulo}'?", "Confirmação", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var db = new EstudoDbContext()) { db.Assuntos.Remove(sel); db.SaveChanges(); }
                    TemaManager.SincronizarCalendarioGlobal();
                    CarregarDados();
                }
            }
        }

        private void Filtro_Changed(object sender, EventArgs e)
        {
            using (var db = new EstudoDbContext())
            {
                var query = db.Assuntos.Include(a => a.Disciplina).AsQueryable();
                if (!string.IsNullOrWhiteSpace(txtBuscaAssunto.Text))
                    query = query.Where(a => a.Titulo.ToLower().Contains(txtBuscaAssunto.Text.ToLower()));
                if (cbFiltroDisciplina.SelectedValue is int id)
                    query = query.Where(a => a.DisciplinaId == id);
                dgAssuntos.ItemsSource = query.ToList();
            }
        }

        private void LimparFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBuscaAssunto.Clear();
            cbFiltroDisciplina.SelectedIndex = -1;
            CarregarDados();
        }

        private void Intervalo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessarMudancaIntervalo(sender as TextBox);
                Keyboard.ClearFocus();
            }
        }

        private void Intervalo_LostFocus(object sender, RoutedEventArgs e)
            => ProcessarMudancaIntervalo(sender as TextBox);
    }
}
