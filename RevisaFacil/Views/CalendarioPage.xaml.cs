using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RevisaFacil.Data; // Importante para acessar o EstudoDbContext
using RevisaFacil.Helpers;

namespace RevisaFacil.Views
{
    public partial class CalendarioPage : Page
    {
        private DateTime _dataReferencia;

        // Propriedade que o XAML usará para verificar quais dias colorir
        // Usamos HashSet para que a busca seja instantânea
        public HashSet<DateTime> DatasComNotas { get; set; } = new HashSet<DateTime>();

        public CalendarioPage()
        {
            InitializeComponent();
            _dataReferencia = DateTime.Today;

            // Define o contexto de dados para a própria página para o MultiBinding funcionar
            this.DataContext = this;

            GerarGradeCalendario();
        }

        private void GerarGradeCalendario()
        {
            // 1. Busca no banco de dados todas as datas que possuem anotações salvas
            try
            {
                using (var db = new EstudoDbContext())
                {
                    DatasComNotas = new HashSet<DateTime>(
                        db.NotasCalendario.Select(n => n.Data.Date).ToList()
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao carregar notas: " + ex.Message);
            }

            // 2. Atualiza o título do mês/ano
            CultureInfo culturaPt = new CultureInfo("pt-BR");
            txtMesAno.Text = culturaPt.DateTimeFormat.GetMonthName(_dataReferencia.Month).ToUpper() + $" DE {_dataReferencia.Year}";

            List<DateTime?> listaDias = new List<DateTime?>();

            // 3. Lógica para alinhar o início do mês na grade
            DateTime primeiroDiaDoMes = new DateTime(_dataReferencia.Year, _dataReferencia.Month, 1);
            int diaSemanaComeco = (int)primeiroDiaDoMes.DayOfWeek;

            // Adiciona espaços vazios até o dia da semana em que o mês começa
            for (int i = 0; i < diaSemanaComeco; i++)
            {
                listaDias.Add(null);
            }

            // Adiciona os dias reais do mês
            int diasNoMes = DateTime.DaysInMonth(_dataReferencia.Year, _dataReferencia.Month);
            for (int dia = 1; dia <= diasNoMes; dia++)
            {
                listaDias.Add(new DateTime(_dataReferencia.Year, _dataReferencia.Month, dia));
            }

            // 4. Atualiza a interface (forçamos o ItemsSource a null para garantir o refresh das cores)
            icDiasCalendario.ItemsSource = null;
            icDiasCalendario.ItemsSource = listaDias;
        }

        private void btnMesAnterior_Click(object sender, RoutedEventArgs e)
        {
            _dataReferencia = _dataReferencia.AddMonths(-1);
            GerarGradeCalendario();
        }

        private void btnProximoMes_Click(object sender, RoutedEventArgs e)
        {
            _dataReferencia = _dataReferencia.AddMonths(1);
            GerarGradeCalendario();
        }

        private void BorderDia_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Verifica se foi um clique duplo com o botão esquerdo
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                if (sender is Border card && card.Tag is DateTime dataClicada)
                {
                    // Abre a janela de anotação (PopUp)
                    var popUp = new PopUpNotaWindow(dataClicada);
                    popUp.Owner = Window.GetWindow(this);

                    // Se a janela for fechada após salvar (DialogResult = true)
                    if (popUp.ShowDialog() == true)
                    {
                        // Recarrega a grade para pintar o card caso uma nova nota tenha sido criada
                        GerarGradeCalendario();
                    }
                }
            }
        }
    }
}