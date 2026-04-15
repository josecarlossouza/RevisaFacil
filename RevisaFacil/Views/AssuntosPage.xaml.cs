// AssuntosPage.xaml.cs — v1.3.3
// ─────────────────────────────────────────────────────────────────────────────
// CORREÇÃO DEFINITIVA da edição de data de início:
//
// Problema raiz de todas as tentativas anteriores:
//   O DataGrid WPF tem um sistema interno de edição (AddNew / EditItem) que
//   entra em conflito quando tentamos abrir/fechar o CellEditingTemplate de
//   fora (via BeginEdit/CommitEdit chamados pelo código). Isso causava o crash
//   "Refresh não é permitido durante AddNew/EditItem" e o DatePicker não abria.
//
// Solução definitiva (v1.3.2):
//   O DataGrid inteiro está agora com IsReadOnly="True". A coluna "Início"
//   NUNCA entra em modo de edição nativo. Ao dar duplo clique nessa coluna,
//   o code-behind abre um Popup WPF posicionado no mouse, contendo apenas um
//   DatePicker com o calendário já aberto (IsDropDownOpen="True").
//   Quando o usuário seleciona a data, o evento SelectedDateChanged salva no
//   banco e fecha o Popup. Se o usuário clicar fora sem selecionar, o Popup
//   fecha via StaysOpen="False" sem alterar nada.
//
// Resultado:
//   - Duplo clique em "Início" abre o calendário imediatamente. ✔
//   - Sem conflitos com o sistema de edição do DataGrid. ✔
//   - Sem crash ao restaurar datas. ✔
//   - Duplo clique em "Assunto" ainda faz toggle de IsDestacado. ✔
//
// v1.3.3:
//   - Adicionado método público FiltrarPorDisciplina(string) para compatibilidade
//     com MainWindow.NavegaParaRevisoes — delega para SelecionarDisciplinaPorNome.
// ─────────────────────────────────────────────────────────────────────────────

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
using System.Windows.Threading;

namespace RevisaFacil.Views
{
    public partial class AssuntosPage : Page
    {
        // ── Estado ────────────────────────────────────────────────────────────────
        private Disciplina _disciplinaAtual;
        private int _qtdRevisoesAtual = 10;
        private bool _carregando = false;

        // Assunto sendo editado via popup de data
        private Assunto _assuntoEditandoData = null;

        // Impede que o evento Closed do Popup salve novamente após um save já feito
        private bool _dataSalvaViaPopup = false;

        public AssuntosPage()
        {
            InitializeComponent();
            CarregarComboEDisciplinaInicial();
            TemaManager.SincronizarCalendarioGlobal();
        }

        // ── Inicialização ─────────────────────────────────────────────────────────

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

                var config = db.Configuracoes.FirstOrDefault();
                int ultimaId = config?.UltimaDisciplinaId ?? 0;

                var selecionada = disciplinas.FirstOrDefault(d => d.Id == ultimaId)
                                  ?? disciplinas.First();

                cbDisciplina.SelectedItem = selecionada;
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

            _disciplinaAtual = db.Disciplinas
                .Include(d => d.Assuntos)
                .FirstOrDefault(d => d.Id == disciplina.Id);

            if (_disciplinaAtual == null) return;

            var config = db.Configuracoes.FirstOrDefault();

            _qtdRevisoesAtual = TemaManager.GetQuantidadeRevisoesDisciplina(_disciplinaAtual, config);
            txtQtdRevisoes.Text = _qtdRevisoesAtual.ToString();
            txtTituloDisciplina.Text = _disciplinaAtual.Nome;

            if (config != null)
            {
                config.UltimaDisciplinaId = _disciplinaAtual.Id;
                db.SaveChanges();
            }

            ReconstruirColunasDinamicas(_disciplinaAtual, config);
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
                    IsReadOnly = true,
                    Width = new DataGridLength(110)
                });
            }

            // Coluna lixeira
            var lixeiraTemplate = new DataTemplate();
            var btnFactory = new FrameworkElementFactory(typeof(Button));
            btnFactory.SetValue(Button.ContentProperty, "🗑");
            btnFactory.SetValue(Button.WidthProperty, 30.0);
            btnFactory.SetValue(Button.HeightProperty, 26.0);
            btnFactory.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)));
            btnFactory.SetValue(Button.ForegroundProperty, Brushes.White);
            btnFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            btnFactory.SetValue(Button.CursorProperty, Cursors.Hand);
            btnFactory.SetValue(Button.FontSizeProperty, 14.0);
            btnFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnApagarAssunto_Click));
            lixeiraTemplate.VisualTree = btnFactory;

            dgAssuntos.Columns.Add(new DataGridTemplateColumn
            {
                Header = "",
                CellTemplate = lixeiraTemplate,
                IsReadOnly = true,
                Width = new DataGridLength(40)
            });
        }

        // ── Combo de disciplinas ──────────────────────────────────────────────────

        private void CbDisciplina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_carregando) return;
            if (cbDisciplina.SelectedItem is Disciplina disc)
                CarregarDisciplina(disc);
        }

        /// <summary>Chamado externamente (ex.: MainWindow) para navegar direto a uma disciplina.</summary>
        public void SelecionarDisciplinaPorNome(string nomeDisciplina)
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

        /// <summary>
        /// Alias público para compatibilidade com MainWindow.NavegaParaRevisoes.
        /// Seleciona a disciplina cujo nome corresponde ao parâmetro informado.
        /// </summary>
        public void FiltrarPorDisciplina(string nomeDisciplina)
            => SelecionarDisciplinaPorNome(nomeDisciplina);

        // ── Botões +/- de revisões ────────────────────────────────────────────────

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

        // ── Edição de intervalo no cabeçalho ──────────────────────────────────────

        private void ProcessarMudancaIntervalo(TextBox tb)
        {
            if (tb == null || _disciplinaAtual == null) return;
            if (!int.TryParse(tb.Text, out int novoValor) || novoValor <= 0) return;

            int r = (int)tb.Tag;

            using var db = new EstudoDbContext(TemaManager.GetDbPath());

            var disc = db.Disciplinas.Find(_disciplinaAtual.Id);
            if (disc == null) return;
            disc.SetIntervalo(r, novoValor);

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

        // ── Edição de data de início via Popup ────────────────────────────────────
        //
        // FLUXO (v1.3.2):
        // 1. Usuário dá duplo clique na célula "Início".
        // 2. DataGridRow_MouseDoubleClick detecta a coluna "Início".
        // 3. Guarda o Assunto em _assuntoEditandoData.
        // 4. Define dpPopup.SelectedDate com a data atual do assunto.
        // 5. Abre o Popup (IsOpen = true) — o DatePicker já está com
        //    IsDropDownOpen="True" no XAML, então o calendário abre sozinho.
        // 6. Usuário escolhe uma data → DpPopup_SelectedDateChanged salva e fecha.
        // 7. Se o usuário clicar fora → Popup fecha via StaysOpen="False" sem salvar.

        private void AbrirPopupData(Assunto assunto)
        {
            if (assunto == null) return;

            _assuntoEditandoData = assunto;
            _dataSalvaViaPopup = false;

            txtPopupLabel.Text = $"Nova data de início para:\n\"{assunto.Titulo}\"";

            // Define a data atual SEM disparar SelectedDateChanged prematuramente
            dpPopup.SelectedDateChanged -= DpPopup_SelectedDateChanged;
            dpPopup.SelectedDate = assunto.DataInicio.Date;
            dpPopup.SelectedDateChanged += DpPopup_SelectedDateChanged;

            // Abre o calendário ao exibir o Popup
            dpPopup.IsDropDownOpen = true;
            popupDataInicio.IsOpen = true;
        }

        private void DpPopup_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Disparado quando o usuário clica em uma data no calendário do Popup
            if (_assuntoEditandoData == null) return;
            if (!dpPopup.SelectedDate.HasValue) return;

            DateTime novaData = dpPopup.SelectedDate.Value.Date;

            // Evita salvar se for a mesma data (ex.: re-abrir com a data já correta)
            if (novaData == _assuntoEditandoData.DataInicio.Date)
            {
                _dataSalvaViaPopup = true;
                popupDataInicio.IsOpen = false;
                return;
            }

            SalvarDataInicio(_assuntoEditandoData, novaData);
            _dataSalvaViaPopup = true;
            popupDataInicio.IsOpen = false;
        }

        private void SalvarDataInicio(Assunto assunto, DateTime novaData)
        {
            using var db = new EstudoDbContext(TemaManager.GetDbPath());
            var entity = db.Assuntos.Find(assunto.Id);
            if (entity == null) return;

            entity.DataInicio = novaData;
            db.SaveChanges();

            TemaManager.SincronizarCalendarioGlobal();
            AplicarFiltro();
        }

        private void PopupDataInicio_Closed(object sender, EventArgs e)
        {
            // Popup fechado (seja por seleção ou por clique fora) — limpa estado
            _assuntoEditandoData = null;
        }

        // ── Duplo clique na linha ─────────────────────────────────────────────────
        //
        // Lógica por coluna:
        //   "Assunto" → toggle IsDestacado (linha verde)
        //   "Início"  → abre Popup com DatePicker
        //   outras    → não faz nada

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGridRow row) || !(row.Item is Assunto assunto)) return;

            var cell = FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
            if (cell == null) return;

            // Detecta a coluna pelo Header (string) ou pelo índice
            string header = "";
            if (cell.Column?.Header is string h)
                header = h;

            // Coluna pelo índice como fallback (índice 0 = Assunto, 1 = Início)
            int colIndex = dgAssuntos.Columns.IndexOf(cell.Column);

            bool isColAssunto = header == "Assunto" || colIndex == 0;
            bool isColInicio = header == "Início" || colIndex == 1;

            // ── Coluna "Assunto": toggle IsDestacado ──────────────────────────────
            if (isColAssunto)
            {
                using var db = new EstudoDbContext(TemaManager.GetDbPath());
                var entity = db.Assuntos.Find(assunto.Id);
                if (entity != null)
                {
                    entity.IsDestacado = !entity.IsDestacado;
                    db.SaveChanges();
                }
                AplicarFiltro();
                e.Handled = true;
                return;
            }

            // ── Coluna "Início": abre Popup com DatePicker ────────────────────────
            if (isColInicio)
            {
                // Usar Dispatcher para garantir que o Popup se posicione após o clique
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    AbrirPopupData(assunto);
                }));
                e.Handled = true;
                return;
            }

            // Para todas as outras colunas: não faz nada
        }

        // ── Auxiliar de VisualTree ────────────────────────────────────────────────

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
                var entity = db.Assuntos.Find(assunto.Id);
                if (entity != null)
                {
                    entity.SetRevConcluida(r, !assunto.GetRevConcluida(r));
                    db.SaveChanges();
                }
                AplicarFiltro();
            }
        }

        // ── Excluir assunto ───────────────────────────────────────────────────────

        private void BtnApagarAssunto_Click(object sender, RoutedEventArgs e)
        {
            // O DataContext do botão é o Assunto da linha
            Assunto sel = null;
            if (sender is Button btn && btn.DataContext is Assunto a)
                sel = a;
            else if (dgAssuntos.SelectedItem is Assunto s)
                sel = s;

            if (sel == null) return;

            if (MessageBox.Show($"Excluir '{sel.Titulo}'?", "Confirmação",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using var db = new EstudoDbContext(TemaManager.GetDbPath());
                var entity = db.Assuntos.Find(sel.Id);
                if (entity != null)
                {
                    db.Assuntos.Remove(entity);
                    db.SaveChanges();
                }
                TemaManager.SincronizarCalendarioGlobal();
                AplicarFiltro();
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