using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RevisaFacil.Helpers
{
    public class StatusToColorConverter : IValueConverter
    {
        // Usando cores estáticas para não sobrecarregar a memória do WPF
        private static readonly SolidColorBrush VerdeEsmeralda = new SolidColorBrush(Color.FromRgb(39, 174, 96));
        private static readonly SolidColorBrush VermelhoAlizarin = new SolidColorBrush(Color.FromRgb(192, 57, 43));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verifica se o valor é booleano e se está concluído
            if (value is bool concluido && concluido)
            {
                return VerdeEsmeralda;
            }

            // Se for false ou nulo, retorna vermelho
            return VermelhoAlizarin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // O DataGrid não precisa converter a cor de volta para booleano
            return Binding.DoNothing;
        }
    }
}