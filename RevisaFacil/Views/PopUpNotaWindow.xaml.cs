using RevisaFacil.Data;
using RevisaFacil.Models;
using System;
using System.Linq;
using System.Windows;

namespace RevisaFacil.Views
{
    public partial class PopUpNotaWindow : Window
    {
        private DateTime _dataNota;

        public PopUpNotaWindow(DateTime data)
        {
            InitializeComponent();
            _dataNota = data;
            txtTituloData.Text = $"📝 Anotações: {data:dd/MM/yyyy}";
            CarregarNota();
            txtNotaPopUp.Focus();
        }

        private void CarregarNota()
        {
            using (var db = new EstudoDbContext())
            {
                var nota = db.NotasCalendario.FirstOrDefault(n => n.Data.Date == _dataNota.Date);
                txtNotaPopUp.Text = nota?.Conteudo ?? "";
            }
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new EstudoDbContext())
            {
                var notaExistente = db.NotasCalendario.FirstOrDefault(n => n.Data.Date == _dataNota.Date);
                string conteudoNovo = txtNotaPopUp.Text.Trim();

                if (string.IsNullOrEmpty(conteudoNovo))
                {
                    // Se o usuário apagou tudo, removemos o registro do banco
                    if (notaExistente != null)
                    {
                        db.NotasCalendario.Remove(notaExistente);
                    }
                }
                else
                {
                    // Se tem texto, salvamos ou atualizamos
                    if (notaExistente != null)
                    {
                        notaExistente.Conteudo = conteudoNovo;
                    }
                    else
                    {
                        db.NotasCalendario.Add(new NotaCalendario { Data = _dataNota, Conteudo = conteudoNovo });
                    }
                }

                db.SaveChanges();
            }
            this.DialogResult = true; // Retorna true para o calendário saber que precisa atualizar as cores
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}