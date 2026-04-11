using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RevisaFacil.Data;
using RevisaFacil.Models;
using RevisaFacil.Helpers;

namespace RevisaFacil.Views
{
    public partial class NovoAssuntoPage : Page
    {
        public NovoAssuntoPage()
        {
            InitializeComponent();
            CarregarDisciplinas();
        }

        private void CarregarDisciplinas()
        {
            using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
            {
                cbDisciplina.ItemsSource = db.Disciplinas.OrderBy(d => d.Nome).ToList();
                cbDisciplina.DisplayMemberPath = "Nome";
                cbDisciplina.SelectedValuePath = "Id";
            }
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            string tituloNovo = txtTitulo.Text.Trim();

            if (cbDisciplina.SelectedValue == null || string.IsNullOrWhiteSpace(tituloNovo))
            {
                MessageBox.Show("Preencha todos os campos!");
                return;
            }

            int idSelecionado = (int)cbDisciplina.SelectedValue;

            using (var db = new EstudoDbContext(TemaManager.GetDbPath()))
            {
                // Verifica se já existe esse assunto cadastrado PARA ESTA disciplina
                bool existe = db.Assuntos.Any(a => a.Titulo.ToLower() == tituloNovo.ToLower()
                                                && a.DisciplinaId == idSelecionado);

                if (existe)
                {
                    MessageBox.Show("❌ Erro: Este assunto já existe nesta disciplina!", "Assunto Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var novo = new Assunto
                {
                    Titulo = tituloNovo,
                    DisciplinaId = idSelecionado,
                    DataInicio = DateTime.Today,
                    Int1 = 30,
                    Int2 = 60,
                    Int3 = 90,
                    Int4 = 120,
                    Int5 = 150
                };

                db.Assuntos.Add(novo);
                db.SaveChanges();

                MessageBox.Show("✅ Assunto salvo com sucesso!");
                txtTitulo.Clear();
            }
        }
    }
}