using RevisaFacil.Data;
using RevisaFacil.Models;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace RevisaFacil.Views
{
    public partial class PopUpNotaWindow : Window
    {
        private DateTime _dataNota;

        public PopUpNotaWindow(DateTime data)
        {
            InitializeComponent();
            _dataNota = data.Date;
            txtTituloData.Text = $"📝 Anotações: {data:dd/MM/yyyy}";
            CarregarNota();
            txtNotaPopUp.Focus();
        }

        private void CarregarNota()
        {
            using (var db = new EstudoDbContext())
            {
                var sb = new StringBuilder();

                // 1. Carrega a nota de revisão automática do dia (AssuntoId = -1), se existir
                var notaRevisao = db.NotasCalendario
                    .FirstOrDefault(n => n.Data.Date == _dataNota && n.AssuntoId == -1);

                if (notaRevisao != null && !string.IsNullOrWhiteSpace(notaRevisao.Conteudo))
                {
                    sb.AppendLine("--- REVISÕES DO DIA ---");
                    sb.AppendLine(notaRevisao.Conteudo);
                    sb.AppendLine("--- FIM DAS REVISÕES ---");
                    sb.AppendLine(); // linha em branco separadora
                }

                // 2. Carrega a nota manual do usuário (AssuntoId = -2), se existir
                var notaManual = db.NotasCalendario
                    .FirstOrDefault(n => n.Data.Date == _dataNota && n.AssuntoId == -2);

                if (notaManual != null && !string.IsNullOrWhiteSpace(notaManual.Conteudo))
                {
                    sb.Append(notaManual.Conteudo);
                }

                // Exibe tudo na caixa de texto
                txtNotaPopUp.Text = sb.ToString();

                // Posiciona o cursor no final, para o usuário continuar digitando após as revisões
                txtNotaPopUp.CaretIndex = txtNotaPopUp.Text.Length;
            }
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new EstudoDbContext())
            {
                // Encontra o bloco de texto da nota de revisão automática para REMOVÊ-LO
                // antes de salvar a parte manual do usuário.
                string textoCompleto = txtNotaPopUp.Text;
                string conteudoManual = textoCompleto;

                // Extrai apenas o texto que o usuário digitou (remove o bloco de revisões automáticas)
                var notaRevisao = db.NotasCalendario
                    .FirstOrDefault(n => n.Data.Date == _dataNota && n.AssuntoId == -1);

                if (notaRevisao != null)
                {
                    // Monta o cabeçalho que foi adicionado na exibição para poder removê-lo
                    string cabecalho = $"--- REVISÕES DO DIA ---\n{notaRevisao.Conteudo}\n--- FIM DAS REVISÕES ---\n\n";
                    if (conteudoManual.StartsWith(cabecalho))
                    {
                        conteudoManual = conteudoManual.Substring(cabecalho.Length);
                    }
                    else if (conteudoManual.StartsWith(cabecalho.TrimEnd()))
                    {
                        conteudoManual = conteudoManual.Substring(cabecalho.TrimEnd().Length).TrimStart('\n', '\r');
                    }
                }

                // Salva (ou remove) apenas a nota manual
                var notaManualExistente = db.NotasCalendario
                    .FirstOrDefault(n => n.Data.Date == _dataNota && n.AssuntoId == -2);

                string conteudoFinal = conteudoManual.Trim();

                if (string.IsNullOrEmpty(conteudoFinal))
                {
                    // Se o usuário apagou tudo, remove a nota manual do banco
                    if (notaManualExistente != null)
                        db.NotasCalendario.Remove(notaManualExistente);
                }
                else
                {
                    if (notaManualExistente != null)
                    {
                        notaManualExistente.Conteudo = conteudoFinal;
                    }
                    else
                    {
                        db.NotasCalendario.Add(new NotaCalendario
                        {
                            Data = _dataNota,
                            Conteudo = conteudoFinal,
                            AssuntoId = -2
                        });
                    }
                }

                db.SaveChanges();
            }

            this.DialogResult = true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}