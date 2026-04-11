using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RevisaFacil.Helpers
{
    // Retorna grossura da borda 3 se for hoje, senão 1.
    public class DiaParaThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime data && data.Date == DateTime.Today)
                return new Thickness(3); // Borda grossa para hoje

            return new Thickness(1); // Borda normal
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Retorna cor da borda Preta se for hoje, senão Cinza claro.
    public class DiaParaCorBordaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime data && data.Date == DateTime.Today)
                return System.Windows.Media.Brushes.Black; // Borda preta para hoje

            return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#D5DBDB");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}