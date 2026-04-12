using RevisaFacil.Data;
using RevisaFacil.Helpers;
using RevisaFacil.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevisaFacil.Views
{
    public partial class CalendarioPage : Page, INotifyPropertyChanged
    {
        private DateTime _dataReferencia;
        private HashSet<DateTime> _datasComNotas = new HashSet<DateTime>();

        // Esta lista será vinculada ao ItemsControl do seu XAML
        public ObservableCollection<DateTime?> DiasDoMes { get; set; } = new ObservableCollection<DateTime?>();

        public event PropertyChangedEventHandler PropertyChanged;

        public HashSet<DateTime> DatasComNotas
        {
            get => _datasComNotas;
            set { _datasComNotas = value; OnPropertyChanged(); }
        }

        public CalendarioPage()
        {
            InitializeComponent();
            _dataReferencia = DateTime.Today;

            // Sincroniza as revisões globais ao abrir
            TemaManager.SincronizarCalendarioGlobal();

            CarregarDatasComNotas();

            this.DataContext = this;
            MontarCalendario();
        }

        private void CarregarDatasComNotas()
        {
            using (var db = new EstudoDbContext())
            {
                var datas = db.NotasCalendario
                              .Select(n => n.Data.Date)
                              .Distinct()
                              .ToList();

                DatasComNotas = new HashSet<DateTime>(datas);
            }
        }

        private void MontarCalendario()
        {
            // Atualiza o cabeçalho
            txtMesAno.Text = _dataReferencia.ToString("MMMM yyyy").ToUpper();

            DiasDoMes.Clear();

            DateTime primeiroDiaMes = new DateTime(_dataReferencia.Year, _dataReferencia.Month, 1);
            int diasNoMes = DateTime.DaysInMonth(_dataReferencia.Year, _dataReferencia.Month);
            int diaDaSemanaInicio = (int)primeiroDiaMes.DayOfWeek;

            // 1. Adiciona espaços vazios (null) para alinhar o dia 1 ao dia da semana correto
            for (int i = 0; i < diaDaSemanaInicio; i++)
            {
                DiasDoMes.Add(null);
            }

            // 2. Adiciona os dias reais do mês
            for (int i = 1; i <= diasNoMes; i++)
            {
                DiasDoMes.Add(new DateTime(_dataReferencia.Year, _dataReferencia.Month, i));
            }

            // 3. Atribui a lista ao ItemsControl do seu XAML
            icDiasCalendario.ItemsSource = DiasDoMes;
        }

        private void btnMesAnterior_Click(object sender, RoutedEventArgs e)
        {
            _dataReferencia = _dataReferencia.AddMonths(-1);
            MontarCalendario();
        }

        private void btnProximoMes_Click(object sender, RoutedEventArgs e)
        {
            _dataReferencia = _dataReferencia.AddMonths(1);
            MontarCalendario();
        }

        private void BorderDia_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is DateTime dataSelecionada)
            {
                var popUp = new PopUpNotaWindow(dataSelecionada);
                if (popUp.ShowDialog() == true)
                {
                    // Recarrega as cores após fechar o popup
                    CarregarDatasComNotas();
                    // Força a atualização visual do ItemsControl
                    icDiasCalendario.Items.Refresh();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}