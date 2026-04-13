// MainWindow.xaml.cs
// CORREÇÃO: InicializarSistemaDeTemas() agora cria apenas o banco "Padrao" vazio
// quando não existe nenhum .db. O seed.json só é processado se o arquivo existir
// com o nome exato "seed.json" — se estiver renomeado (seed_OFF.json), é ignorado.

using Microsoft.Win32;
using RevisaFacil.Data;
using RevisaFacil.Helpers;
using RevisaFacil.Services;
using RevisaFacil.Views;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RevisaFacil
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timerAgenda;
        private bool _alertaHojeEnviado = false;
        private DateTime _ultimoDiaDeEnvio = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            InicializarSistemaDeTemas();

            TemaManager.MigrarConfiguracoes();
            TemaManager.SincronizarCalendarioGlobal();

            MainFrame.Navigate(new MainView());

            Task.Run(async () =>
            {
                await VerificarAlertasTelegram();
                await EnviarResumoCalendarioSeNecessario();
            });

            IniciarTimerAgenda();
        }

        // ── Exportar ──────────────────────────────────────────────────────────────

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nomeTema = TemaManager.TemaAtual;

                var dlg = new SaveFileDialog
                {
                    Title = "Exportar para Excel",
                    FileName = $"{nomeTema}.xlsx",
                    DefaultExt = ".xlsx",
                    Filter = "Planilha Excel (*.xlsx)|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dlg.ShowDialog() != true) return;

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                try
                {
                    ExcelService.Exportar(dlg.FileName);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                MessageBox.Show(
                    $"Arquivo exportado com sucesso!\n\n{dlg.FileName}",
                    "Exportação concluída",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao exportar:\n{ex.Message}",
                    "Erro na exportação",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ── Importar ──────────────────────────────────────────────────────────────

        private void btnImportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Title = "Importar planilha Excel",
                    DefaultExt = ".xlsx",
                    Filter = "Planilha Excel (*.xlsx)|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dlg.ShowDialog() != true) return;

                string nomeTema = Path.GetFileNameWithoutExtension(dlg.FileName);
                bool temaExiste = TemaManager.ListarTemas().Contains(nomeTema);

                string pergunta = temaExiste
                    ? $"O tema '{nomeTema}' já existe.\n\nNovos assuntos serão adicionados e dados diferentes serão atualizados.\n\nContinuar?"
                    : $"Será criado um novo tema chamado '{nomeTema}'.\n\nContinuar?";

                if (MessageBox.Show(pergunta, "Confirmar importação",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                ExcelService.ResultadoImportacao resultado;
                try
                {
                    resultado = ExcelService.Importar(dlg.FileName);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                if (!resultado.Sucesso)
                {
                    MessageBox.Show(resultado.Mensagem, "Importação falhou",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AtualizarComboTemas();
                cbTemas.SelectedItem = nomeTema;

                var sb = new StringBuilder();
                sb.AppendLine(resultado.Mensagem);
                sb.AppendLine();
                if (resultado.TemaNovosCriado) sb.AppendLine("✅ Novo tema criado.");
                if (resultado.DisciplinasNovas > 0) sb.AppendLine($"📚 {resultado.DisciplinasNovas} disciplina(s) nova(s) adicionada(s).");
                if (resultado.AssuntosNovos > 0) sb.AppendLine($"📝 {resultado.AssuntosNovos} assunto(s) novo(s) adicionado(s).");
                if (resultado.AssuntosAtualizados > 0) sb.AppendLine($"🔄 {resultado.AssuntosAtualizados} assunto(s) atualizado(s).");
                if (resultado.DisciplinasNovas == 0 && resultado.AssuntosNovos == 0 && resultado.AssuntosAtualizados == 0)
                    sb.AppendLine("ℹ️ Nenhuma alteração necessária — todos os dados já estavam atualizados.");

                MessageBox.Show(sb.ToString().Trim(), "Importação concluída",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MainFrame.Navigate(new MainView());
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show(
                    $"Erro ao importar:\n{ex.Message}",
                    "Erro na importação",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ── Timer para envio às 8h ────────────────────────────────────────────────

        private void IniciarTimerAgenda()
        {
            _timerAgenda = new DispatcherTimer();
            _timerAgenda.Interval = TimeSpan.FromMinutes(1);
            _timerAgenda.Tick += TimerAgenda_Tick;
            _timerAgenda.Start();
        }

        private async void TimerAgenda_Tick(object sender, EventArgs e)
        {
            var agora = DateTime.Now;
            if (agora.Date != _ultimoDiaDeEnvio) _alertaHojeEnviado = false;

            if (!_alertaHojeEnviado && agora.Hour == 8 && agora.Minute == 0)
            {
                _alertaHojeEnviado = true;
                _ultimoDiaDeEnvio = agora.Date;
                await Task.Run(async () => await EnviarResumoCalendarioSeNecessario());
            }
        }

        // ── Envio do resumo do calendário ─────────────────────────────────────────

        private async Task EnviarResumoCalendarioSeNecessario()
        {
            try
            {
                var telegram = new TelegramService();
                await telegram.EnviarResumoCalendarioHoje();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao enviar resumo calendário: " + ex.Message);
            }
        }

        // ── Alerta de revisões pendentes/atrasadas ────────────────────────────────

        private async Task VerificarAlertasTelegram()
        {
            try
            {
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    var hoje = DateTime.Today;
                    int qtdRev = TemaManager.GetQuantidadeRevisoes();

                    var pendentes = db.Assuntos.ToList().Where(a =>
                    {
                        for (int i = 1; i <= qtdRev; i++)
                            if (a.GetDataRev(i) <= hoje && !a.GetRevConcluida(i)) return true;
                        return false;
                    }).ToList();

                    if (pendentes.Any())
                    {
                        var telegram = new TelegramService();
                        var sb = new StringBuilder();
                        sb.AppendLine("📢 *REVISAFACIL: Revisões Pendentes*");
                        sb.AppendLine($"📅 Data: {hoje:dd/MM/yyyy}");
                        sb.AppendLine();

                        foreach (var p in pendentes.Take(10))
                        {
                            sb.AppendLine($"📌 *{p.Disciplina?.Nome ?? "Geral"}*");
                            sb.AppendLine($"└ {p.Titulo}");
                            sb.AppendLine();
                        }

                        if (pendentes.Count > 10)
                            sb.AppendLine($"... e mais {pendentes.Count - 10} assuntos atrasados.");

                        await telegram.EnviarAlerta(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro Telegram: " + ex.Message);
            }
        }

        // ── Sistema de Temas ──────────────────────────────────────────────────────

        private void InicializarSistemaDeTemas()
        {
            // ─────────────────────────────────────────────────────────────────────
            // PASSO 1: Garante que o tema "Padrao" (banco vazio) exista.
            // Só cria o arquivo se ele ainda não existir no disco.
            // ─────────────────────────────────────────────────────────────────────
            string dbPadrao = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Padrao.db");
            if (!File.Exists(dbPadrao))
            {
                TemaManager.TemaAtual = "Padrao";
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                    db.Database.EnsureCreated();
                TemaManager.MigrarConfiguracoes();
            }

            // ─────────────────────────────────────────────────────────────────────
            // PASSO 2: Verifica se existe "seed.json" com o nome EXATO.
            // Se o arquivo estiver renomeado (ex: seed_OFF.json), ele é ignorado.
            // O seed só roda UMA VEZ: somente se o tema do json ainda não existe
            // como arquivo .db no disco.
            // ─────────────────────────────────────────────────────────────────────
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.json");
            string temaDoJson = null;

            if (File.Exists(filePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
                    var root = JsonSerializer.Deserialize<SeedRoot>(jsonString);

                    if (!string.IsNullOrEmpty(root?.NomeTema))
                    {
                        temaDoJson = root.NomeTema.Trim();

                        // Só processa o seed se o banco desse tema NÃO existe ainda
                        string dbDoTema = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory, $"{temaDoJson}.db");

                        if (!File.Exists(dbDoTema))
                        {
                            TemaManager.TemaAtual = temaDoJson;
                            using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                            {
                                db.Database.EnsureCreated();
                                TemaManager.MigrarConfiguracoes();
                                TemaManager.SeedDatabase(db);
                            }
                        }
                        // Se o banco já existe, apenas usa o tema — sem recriar nada
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao ler seed.json: " + ex.Message);
                    temaDoJson = null;
                }
            }

            // ─────────────────────────────────────────────────────────────────────
            // PASSO 3: Define o tema ativo.
            // Prioridade: tema do seed (se lido com sucesso) → "Padrao"
            // ─────────────────────────────────────────────────────────────────────
            TemaManager.TemaAtual = temaDoJson ?? "Padrao";
            AtualizarComboTemas();
        }

        private void AtualizarComboTemas()
        {
            cbTemas.ItemsSource = TemaManager.ListarTemas();
            cbTemas.SelectedItem = TemaManager.TemaAtual;
        }

        private void cbTemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTemas.SelectedItem is string tema)
            {
                TemaManager.TemaAtual = tema;
                TemaManager.MigrarConfiguracoes();
                TemaManager.SincronizarCalendarioGlobal();
                MainFrame.Navigate(new MainView());
            }
        }

        private void btnNovoTema_Click(object sender, RoutedEventArgs e)
        {
            string novoNome = Microsoft.VisualBasic.Interaction.InputBox("Nome do novo Tema:", "Novo Tema");
            if (!string.IsNullOrWhiteSpace(novoNome))
            {
                novoNome = novoNome.Trim();
                if (TemaManager.ListarTemas().Contains(novoNome)) return;
                TemaManager.TemaAtual = novoNome;
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                    db.Database.EnsureCreated();
                TemaManager.MigrarConfiguracoes();
                AtualizarComboTemas();
                cbTemas.SelectedItem = novoNome;
            }
        }

        private void btnExcluirTema_Click(object sender, RoutedEventArgs e)
        {
            var tema = cbTemas.SelectedItem as string;
            if (string.IsNullOrEmpty(tema) || tema == "Padrao") return;

            if (MessageBox.Show($"Deseja excluir permanentemente o tema '{tema}'?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // CHAMA O NOVO MÉTODO QUE LIBERA O BANCO ANTES DE DELETAR
                    TemaManager.DeletarBanco(tema);

                    TemaManager.TemaAtual = "Padrao";
                    AtualizarComboTemas();
                    MainFrame.Navigate(new MainView());

                    MessageBox.Show("Tema excluído com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao excluir: " + ex.Message);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timerAgenda?.Stop();
            base.OnClosed(e);
        }

        // ── Navegação ─────────────────────────────────────────────────────────────

        private void NavCalendario_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CalendarioPage());
        private void btnPainel_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MainView());
        private void btnAssuntos_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AssuntosPage());
        private void btnNovoAssunto_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NovoAssuntoPage());
        private void btnNovaDisciplina_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NovaDisciplinaPage());
        private void btnEstatisticas_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new EstatisticasPage());
    }
}