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
        public AssuntosPage()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            using (var db = new EstudoDbContext())
            {
                var config = db.Configuracoes.FirstOrDefault();
                if (config == null)
                {
                    config = new Configuracao { Intervalo1 = 30, Intervalo2 = 60, Intervalo3 = 90, Intervalo4 = 120, Intervalo5 = 150 };
                    db.Configuracoes.Add(config);
                    db.SaveChanges();
                }

                txtR1.Text = config.Intervalo1.ToString();
                txtR2.Text = config.Intervalo2.ToString();
                txtR3.Text = config.Intervalo3.ToString();
                txtR4.Text = config.Intervalo4.ToString();
                txtR5.Text = config.Intervalo5.ToString();

                cbFiltroDisciplina.ItemsSource = db.Disciplinas.OrderBy(d => d.Nome).ToList();

                var lista = db.Assuntos.Include(a => a.Disciplina).ToList();
                dgAssuntos.ItemsSource = lista;
            }
        }

        // NOVA LÓGICA: Clique Duplo para cor ou edição
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Assunto assunto)
            {
                var elementoClicado = e.OriginalSource as FrameworkElement;
                var cell = FindParent<DataGridCell>(elementoClicado);

                if (cell != null)
                {
                    string header = cell.Column.Header.ToString();

                    // Se clicar em Disciplina ou Assunto, alterna a cor
                    if (header == "Disciplina" || header == "Assunto")
                    {
                        using (var db = new EstudoDbContext())
                        {
                            db.Assuntos.Attach(assunto);
                            assunto.IsDestacado = !assunto.IsDestacado;
                            db.SaveChanges();
                        }
                        dgAssuntos.Items.Refresh();
                        e.Handled = true; // Impede comportamento extra
                    }
                    // Se clicar em "Início", não faz nada aqui (deixa o WPF abrir a edição)
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
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

        private void MarcarRevisao_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Assunto assunto)
            {
                int r = int.Parse(btn.Tag.ToString());
                using (var db = new EstudoDbContext())
                {
                    db.Assuntos.Attach(assunto);
                    if (r == 1) assunto.Rev1Concluida = !assunto.Rev1Concluida;
                    else if (r == 2) assunto.Rev2Concluida = !assunto.Rev2Concluida;
                    else if (r == 3) assunto.Rev3Concluida = !assunto.Rev3Concluida;
                    else if (r == 4) assunto.Rev4Concluida = !assunto.Rev4Concluida;
                    else if (r == 5) assunto.Rev5Concluida = !assunto.Rev5Concluida;

                    db.SaveChanges();
                }
                dgAssuntos.Items.Refresh();
            }
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
        {
            ProcessarMudancaIntervalo(sender as TextBox);
        }

        private void ProcessarMudancaIntervalo(TextBox tb)
        {
            if (tb != null && int.TryParse(tb.Text, out int novoValor))
            {
                int r = int.Parse(tb.Tag.ToString());
                using (var db = new EstudoDbContext())
                {
                    var config = db.Configuracoes.First();
                    if (r == 1) config.Intervalo1 = novoValor;
                    else if (r == 2) config.Intervalo2 = novoValor;
                    else if (r == 3) config.Intervalo3 = novoValor;
                    else if (r == 4) config.Intervalo4 = novoValor;
                    else if (r == 5) config.Intervalo5 = novoValor;

                    var lista = db.Assuntos.ToList();
                    foreach (var a in lista)
                    {
                        if (r == 1) a.Int1 = novoValor;
                        else if (r == 2) a.Int2 = novoValor;
                        else if (r == 3) a.Int3 = novoValor;
                        else if (r == 4) a.Int4 = novoValor;
                        else if (r == 5) a.Int5 = novoValor;
                    }
                    db.SaveChanges();
                }
                CarregarDados();
            }
        }

        private void dgAssuntos_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is Assunto assunto)
            {
                // Delay para garantir que o binding da data seja atualizado antes de salvar
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        using (var db = new EstudoDbContext())
                        {
                            db.Entry(assunto).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        dgAssuntos.Items.Refresh();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao salvar data: {ex.Message}");
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void btnApagarAssunto_Click(object sender, RoutedEventArgs e)
        {
            if (dgAssuntos.SelectedItem is Assunto selecionado)
            {
                if (MessageBox.Show($"Deseja excluir '{selecionado.Titulo}'?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var db = new EstudoDbContext())
                    {
                        db.Assuntos.Remove(selecionado);
                        db.SaveChanges();
                    }
                    CarregarDados();
                }
            }
        }
    }
}