using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using RevisaFacil.Views;
using RevisaFacil.Helpers;
using RevisaFacil.Data;
using RevisaFacil.Services; // Adicionado para o Telegram

namespace RevisaFacil
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InicializarSistemaDeTemas();
            MainFrame.Navigate(new MainView());

            // Dispara verificação do Telegram em segundo plano para não travar a abertura
            Task.Run(async () => await VerificarAlertasTelegram());
        }

        private void InicializarSistemaDeTemas()
        {
            var temasExistentes = TemaManager.ListarTemas();
            if (!temasExistentes.Contains("Padrao"))
            {
                TemaManager.TemaAtual = "Padrao";
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    db.Database.EnsureCreated();
                }
            }

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
                        if (!TemaManager.ListarTemas().Contains(temaDoJson))
                        {
                            TemaManager.TemaAtual = temaDoJson;
                            using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                            {
                                db.Database.EnsureCreated();
                                TemaManager.SeedDatabase(db);
                            }
                        }
                    }
                }
                catch { }
            }

            TemaManager.TemaAtual = temaDoJson ?? "Padrao";
            AtualizarComboTemas();
        }

        private async Task VerificarAlertasTelegram()
        {
            try
            {
                using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                {
                    var hoje = DateTime.Today;
                    // Filtra assuntos com revisões vencidas ou para hoje e não concluídas
                    var pendentes = db.Assuntos.ToList().Where(a =>
                        (a.DataRev1 <= hoje && !a.Rev1Concluida) ||
                        (a.DataRev2 <= hoje && !a.Rev2Concluida) ||
                        (a.DataRev3 <= hoje && !a.Rev3Concluida) ||
                        (a.DataRev4 <= hoje && !a.Rev4Concluida) ||
                        (a.DataRev5 <= hoje && !a.Rev5Concluida)
                    ).ToList();

                    if (pendentes.Any())
                    {
                        var telegram = new TelegramService();
                        var sb = new StringBuilder();
                        sb.AppendLine("📢 *ESTUDO CARLOS: Revisões Pendentes*");
                        sb.AppendLine($"📅 Data: {hoje:dd/MM/yyyy}");
                        sb.AppendLine("");

                        foreach (var p in pendentes.Take(10)) // Mostra os 10 primeiros
                        {
                            sb.AppendLine($"📌 *{p.Disciplina.Nome}*");
                            sb.AppendLine($"└ {p.Titulo}");
                            sb.AppendLine("");
                        }

                        if (pendentes.Count > 10)
                            sb.AppendLine($"... e mais {pendentes.Count - 10} assuntos atrasados.");

                        await telegram.EnviarAlerta(sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro Alerta Telegram: " + ex.Message);
            }
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
                {
                    db.Database.EnsureCreated();
                }
                AtualizarComboTemas();
                cbTemas.SelectedItem = novoNome;
            }
        }

        private void btnExcluirTema_Click(object sender, RoutedEventArgs e)
        {
            if (cbTemas.SelectedItem is string tema && tema != "Padrao")
            {
                if (MessageBox.Show($"Excluir tema '{tema}'?", "Aviso", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    File.Delete(TemaManager.GetDbPath());
                    TemaManager.TemaAtual = "Padrao";
                    AtualizarComboTemas();
                }
            }
        }

        private void NavCalendario_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CalendarioPage());
        }

        private void btnPainel_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MainView());
        private void btnAssuntos_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AssuntosPage());
        private void btnNovoAssunto_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NovoAssuntoPage());
        private void btnNovaDisciplina_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new NovaDisciplinaPage());
    }
}