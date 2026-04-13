// DiaParaEstiloConverter.cs  (Helpers)
// Contém: DiaParaThicknessConverter e DiaParaCorBordaConverter
// CORRIGIDO: "is DateTime?" é ilegal no C# — substituído por cast via GetType() == typeof(DateTime?)

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RevisaFacil.Helpers
{
    // Retorna grossura da borda: 3 se for hoje, senão 1.
    public class DiaParaThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? data = ExtrairData(value);
            if (data.HasValue && data.Value.Date == DateTime.Today)
                return new Thickness(3);
            return new Thickness(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static DateTime? ExtrairData(object value)
        {
            if (value is DateTime dt) return dt;

            // DateTime? boxed como object não pode usar "is DateTime?" — usamos GetType()
            if (value != null && value.GetType() == typeof(DateTime?))
                return (DateTime?)value;

            // Suporte ao DiaCalendarioItem via reflexão
            if (value != null)
            {
                var prop = value.GetType().GetProperty("Data");
                if (prop != null)
                {
                    var val = prop.GetValue(value);
                    if (val is DateTime dtProp) return dtProp;
                    if (val != null && val.GetType() == typeof(DateTime?)) return (DateTime?)val;
                }
            }

            return null;
        }
    }

    // Retorna cor da borda: Preta se for hoje, senão Cinza claro.
    public class DiaParaCorBordaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? data = ExtrairData(value);
            if (data.HasValue && data.Value.Date == DateTime.Today)
                return Brushes.Black;
            return (Brush)new BrushConverter().ConvertFromString("#D5DBDB");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static DateTime? ExtrairData(object value)
        {
            if (value is DateTime dt) return dt;

            if (value != null && value.GetType() == typeof(DateTime?))
                return (DateTime?)value;

            if (value != null)
            {
                var prop = value.GetType().GetProperty("Data");
                if (prop != null)
                {
                    var val = prop.GetValue(value);
                    if (val is DateTime dtProp) return dtProp;
                    if (val != null && val.GetType() == typeof(DateTime?)) return (DateTime?)val;
                }
            }

            return null;
        }
    }
}
