// MainWindow.xaml.cs — v1.3.0
// ALTERAÇÕES:
// 1. Handlers do botão Ajuda: ComoUsar, Novidades, Sobre
// 2. MigrarConfiguracoesDisciplinas() chamado em todos os pontos de migração
// 3. NavegaParaRevisoes() — método público chamado pelo MainView ao clicar no gráfico

using Microsoft.Win32;
using RevisaFacil.Data;
using RevisaFacil.Helpers;
using RevisaFacil.Models;
using RevisaFacil.Services;
using RevisaFacil.Views;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;

namespace RevisaFacil
{
    public partial class MainWindow : Window
    {
        public static readonly string Versao = "v1.3.0";

        private DispatcherTimer _timerAgenda;
        private DateTime _ultimoEnvioTelegram = DateTime.MinValue;

        public MainWindow()
        {
            InitializeComponent();
            InicializarSistemaDeTemas();
            MainFrame.Navigate(new MainView());
            IniciarTimerAgenda();
            _ = EnviarResumoDiarioAsync();
        }

        // ── Timer de agenda diária ────────────────────────────────────────────────

        private void IniciarTimerAgenda()
        {
            _timerAgenda = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            _timerAgenda.Tick += async (s, e) =>
            {
                var agora = DateTime.Now;
                if (agora.Hour == 8 && agora.Minute == 0 && _ultimoEnvioTelegram.Date < DateTime.Today)
                    await EnviarResumoDiarioAsync();
            };
            _timerAgenda.Start();
        }

        private async Task EnviarResumoDiarioAsync()
        {
            try
            {
                _ultimoEnvioTelegram = DateTime.Now;
                var telegram = new TelegramService();
                await telegram.EnviarResumoCalendarioHoje();

                var hoje = DateTime.Today;
                using var db = new EstudoDbContext(TemaManager.GetDbPath());
                int qtdRev = TemaManager.GetQuantidadeRevisoes();

                var pendentes = db.Assuntos.Include(a => a.Disciplina).ToList()
                    .Where(a =>
                    {
                        for (int i = 1; i <= qtdRev; i++)
                            if (a.GetDataRev(i).Date <= hoje && !a.GetRevConcluida(i)) return true;
                        return false;
                    }).ToList();

                if (pendentes.Any())
                {
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro Telegram: " + ex.Message);
            }
        }

        // ── Sistema de Temas ──────────────────────────────────────────────────────

        private void InicializarSistemaDeTemas()
        {
            // PASSO 1: Garante banco "Padrao"
            string dbPadrao = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Padrao.db");
            if (!File.Exists(dbPadrao))
            {
                TemaManager.TemaAtual = "Padrao";
                using var db = new EstudoDbContext(TemaManager.GetDbPath());
                db.Database.EnsureCreated();
                TemaManager.MigrarConfiguracoes();
                TemaManager.MigrarConfiguracoesDisciplinas();
            }

            // PASSO 2: Verifica seed.json
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
                        string dbDoTema = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{temaDoJson}.db");
                        if (!File.Exists(dbDoTema))
                        {
                            TemaManager.TemaAtual = temaDoJson;
                            using var db = new EstudoDbContext(TemaManager.GetDbPath());
                            db.Database.EnsureCreated();
                            TemaManager.MigrarConfiguracoes();
                            TemaManager.MigrarConfiguracoesDisciplinas();
                            TemaManager.SeedDatabase(db);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao ler seed.json: " + ex.Message);
                    temaDoJson = null;
                }
            }

            // PASSO 3: Ativa o tema e migra
            TemaManager.TemaAtual = temaDoJson ?? "Padrao";
            TemaManager.MigrarConfiguracoes();
            TemaManager.MigrarConfiguracoesDisciplinas();
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
                TemaManager.MigrarConfiguracoesDisciplinas();
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
                using var db = new EstudoDbContext(TemaManager.GetDbPath());
                db.Database.EnsureCreated();
                TemaManager.MigrarConfiguracoes();
                TemaManager.MigrarConfiguracoesDisciplinas();
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
                    TemaManager.DeletarBanco(tema);
                    TemaManager.TemaAtual = "Padrao";
                    AtualizarComboTemas();
                    MainFrame.Navigate(new MainView());
                    MessageBox.Show("Tema excluído com sucesso!");
                }
                catch (Exception ex) { MessageBox.Show("Erro ao excluir: " + ex.Message); }
            }
        }

        // ── Exportar / Importar Excel ─────────────────────────────────────────────

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                FileName = TemaManager.TemaAtual,
                DefaultExt = ".xlsx",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ExcelService.Exportar(dlg.FileName);
                    MessageBox.Show($"✅ Exportado com sucesso!\n{dlg.FileName}", "Exportação");
                }
                catch (Exception ex) { MessageBox.Show("❌ Erro ao exportar:\n" + ex.Message, "Erro"); }
            }
        }

        private void btnImportar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".xlsx",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var resultado = ExcelService.Importar(dlg.FileName);
                    if (!resultado.Sucesso)
                    {
                        MessageBox.Show("❌ " + resultado.Mensagem, "Erro de importação");
                        return;
                    }
                    AtualizarComboTemas();
                    cbTemas.SelectedItem = TemaManager.TemaAtual;
                    MainFrame.Navigate(new MainView());
                    MessageBox.Show(
                        $"✅ Importação concluída!\n\n" +
                        $"Disciplinas novas: {resultado.DisciplinasNovas}\n" +
                        $"Assuntos novos: {resultado.AssuntosNovos}\n" +
                        $"Assuntos atualizados: {resultado.AssuntosAtualizados}\n" +
                        (resultado.TemaNovosCriado ? "📁 Novo tema criado." : ""),
                        "Importação");
                }
                catch (Exception ex) { MessageBox.Show("❌ Erro ao importar:\n" + ex.Message, "Erro"); }
            }
        }

        // ── Botão Ajuda ───────────────────────────────────────────────────────────

        private void btnAjuda_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void AjudaComoUsar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "📖 COMO USAR O REVISAFÁCIL\n\n" +
                "1. TEMAS\n" +
                "   • Crie temas para separar editais ou concursos diferentes.\n" +
                "   • Use o ComboBox 'Tema Atual' para alternar entre eles.\n\n" +
                "2. DISCIPLINAS E ASSUNTOS\n" +
                "   • Cadastre disciplinas em '➕ Nova Disciplina'.\n" +
                "   • Cadastre assuntos em '➕ Novo Assunto', vinculando a uma disciplina.\n\n" +
                "3. MINHAS REVISÕES\n" +
                "   • Selecione uma disciplina no seletor para ver seus assuntos.\n" +
                "   • Clique nos botões coloridos (Rev1, Rev2...) para marcar revisões.\n" +
                "     Verde = concluída  |  Vermelho = pendente\n" +
                "   • Os intervalos no cabeçalho valem só para a disciplina selecionada.\n" +
                "   • Rev1 conta a partir da data de início.\n" +
                "   • Rev2 em diante conta a partir da revisão anterior.\n\n" +
                "4. CALENDÁRIO\n" +
                "   • Visualize revisões por mês, semana ou dia.\n" +
                "   • Duplo clique em um card para adicionar anotações manuais.\n" +
                "   • Cards azuis = possuem revisões ou anotações.\n\n" +
                "5. EXPORTAR / IMPORTAR\n" +
                "   • Exporte seus dados para Excel (.xlsx) para backup ou compartilhamento.\n" +
                "   • Células verdes no Excel indicam revisões concluídas.\n" +
                "   • Importe planilhas exportadas pelo RevisaFácil para restaurar dados.\n\n" +
                "6. TELEGRAM\n" +
                "   • Configure o arquivo 'telegram_config.ini' com seu token e chat ID.\n" +
                "   • O app envia automaticamente um resumo do dia ao ser aberto.",
                "Como Usar o RevisaFácil",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AjudaNovidades_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"🆕 NOVIDADES — {Versao}\n\n" +
                "✅ REVISÕES POR DISCIPLINA\n" +
                "   • Cada disciplina agora tem sua própria quantidade de revisões.\n" +
                "   • Os intervalos de dias valem apenas para a disciplina selecionada.\n\n" +
                "✅ LÓGICA ENCADEADA DE DATAS\n" +
                "   • Revisão 1 é contada a partir da data de início.\n" +
                "   • Revisões 2+ são contadas a partir da revisão anterior.\n\n" +
                "✅ GRÁFICO DE PIZZA INTERATIVO\n" +
                "   • Clique em uma fatia para navegar à disciplina correspondente.\n" +
                "   • Tooltip ao passar o mouse mostra nome e quantidade de assuntos.\n\n" +
                "✅ EXPORTAÇÃO COM COR VERDE\n" +
                "   • Revisões concluídas são exportadas em verde no Excel.\n" +
                "   • A importação restaura o status de destaque dos campos marcados.\n\n" +
                "✅ INTERFACE ATUALIZADA\n" +
                "   • Menu renomeado: 'Meus Assuntos' → 'Minhas Revisões'.\n" +
                "   • Versão exibida na barra de título da janela.\n" +
                "   • Botão de Ajuda com guia de uso, novidades e informações do app.",
                $"Novidades — RevisaFácil {Versao}",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AjudaSobre_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"ℹ️ SOBRE O REVISAFÁCIL\n\n" +
                $"Versão: {Versao}\n" +
                "Plataforma: Windows (WPF / .NET 8)\n" +
                "Banco de Dados: SQLite via Entity Framework Core 8\n\n" +
                "O RevisaFácil é um gerenciador de estudos para concursos públicos.\n" +
                "Substitui planilhas de Excel por um sistema desktop com:\n" +
                "  • Revisões automáticas espaçadas por disciplina\n" +
                "  • Calendário visual de revisões e anotações\n" +
                "  • Notificações via Telegram\n" +
                "  • Exportação e importação em Excel (.xlsx)\n" +
                "  • Estatísticas de desempenho por assunto e disciplina\n\n" +
                "Tecnologias utilizadas:\n" +
                "  • C# + WPF (.NET 8)\n" +
                "  • Entity Framework Core 8 + SQLite\n" +
                "  • LiveCharts.Wpf\n" +
                "  • ClosedXML\n" +
                "  • Telegram.Bot\n\n" +
                "Repositório: github.com/seu-usuario/RevisaFacil",
                "Sobre o RevisaFácil",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ── Navegação ─────────────────────────────────────────────────────────────

        protected override void OnClosed(EventArgs e)
        {
            _timerAgenda?.Stop();
            base.OnClosed(e);
        }

        /// <summary>
        /// Chamado pelo MainView quando o usuário clica em uma fatia do gráfico de pizza.
        /// Navega para AssuntosPage e filtra pela disciplina clicada.
        /// </summary>
        public void NavegaParaRevisoes(string nomeDisciplina)
        {
            var page = new AssuntosPage();
            MainFrame.Navigate(page);
            // Aplica o filtro após a navegação concluir
            MainFrame.LoadCompleted += (s, ev) =>
            {
                page.FiltrarPorDisciplina(nomeDisciplina);
                // Remove o handler para não acumular
                MainFrame.LoadCompleted -= (s2, ev2) => { };
            };
        }

        private void NavCalendario_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CalendarioPage());
        private void btnPainel_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MainView());
        private void btnAssuntos_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AssuntosPage());
        private void btnNovoAssunto_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NovoAssuntoPage());
        private void btnNovaDisciplina_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NovaDisciplinaPage());
        private void btnEstatisticas_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new EstatisticasPage());
    }
}
