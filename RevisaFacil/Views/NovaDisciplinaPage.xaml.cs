using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RevisaFacil.Data;
using RevisaFacil.Models;
using RevisaFacil.Helpers;

namespace RevisaFacil.Views
{
    public partial class NovaDisciplinaPage : Page
    {
        public NovaDisciplinaPage()
        {
            InitializeComponent();
            CarregarDisciplinas();
        }

        /// <summary>
        /// Carrega ou atualiza a lista de disciplinas no DataGrid
        /// </summary>
        private void CarregarDisciplinas()
        {
            using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
            {
                // Busca as disciplinas ordenadas por nome e joga no DataGrid
                dgDisciplinas.ItemsSource = db.Disciplinas.OrderBy(d => d.Nome).ToList();
            }
        }

        /// <summary>
        /// Lógica de salvamento (Disparada pelo clique ou pela tecla ENTER)
        /// </summary>
        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            string nomeNova = txtNomeDisciplina.Text.Trim();

            if (string.IsNullOrWhiteSpace(nomeNova))
            {
                MessageBox.Show("Por favor, digite o nome da disciplina.", "Campo Vazio", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
            {
                // 1. Verificação de Duplicidade (Case-insensitive)
                bool existe = db.Disciplinas.Any(d => d.Nome.ToLower() == nomeNova.ToLower());

                if (existe)
                {
                    MessageBox.Show("❌ Esta disciplina já está cadastrada!", "Disciplina Repetida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Cadastro no Banco
                db.Disciplinas.Add(new Disciplina { Nome = nomeNova });
                db.SaveChanges();

                MessageBox.Show("✅ Disciplina cadastrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                // 3. Limpeza e Atualização da UI
                txtNomeDisciplina.Clear();
                CarregarDisciplinas();
            }
        }

        /// <summary>
        /// Lógica de exclusão de disciplina
        /// </summary>
        private void btnApagarDisciplina_Click(object sender, RoutedEventArgs e)
        {
            // Captura a disciplina da linha onde o botão foi clicado
            if (sender is Button btn && btn.DataContext is Disciplina disciplinaSelecionada)
            {
                var confirmacao = MessageBox.Show(
                    $"ATENÇÃO: Deseja realmente excluir a disciplina '{disciplinaSelecionada.Nome}'?\n\n" +
                    "Isso apagará permanentemente todos os assuntos e revisões vinculados a ela!",
                    "Confirmar Exclusão Crítica",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (confirmacao == MessageBoxResult.Yes)
                {
                    using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
                    {
                        var dbDisc = db.Disciplinas.Find(disciplinaSelecionada.Id);
                        if (dbDisc != null)
                        {
                            db.Disciplinas.Remove(dbDisc);
                            db.SaveChanges();

                            // Atualiza a lista para refletir a remoção
                            CarregarDisciplinas();
                        }
                    }
                }
            }
        }
    }
}