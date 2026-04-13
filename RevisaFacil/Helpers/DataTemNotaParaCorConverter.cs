// DataTemNotaParaCorConverter.cs  (Helpers)
// CORRIGIDO: removido padrão "is DateTime?" que é ilegal no C# — substituído por cast explícito.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RevisaFacil.Helpers
{
    public class DataTemNotaParaCorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] pode ser DateTime, DateTime? (boxed como object) ou DiaCalendarioItem
            // values[1] é HashSet<DateTime> com as datas que têm notas
            DateTime? data = ExtrairData(values[0]);

            if (data.HasValue && values[1] is HashSet<DateTime> datasComNotas)
            {
                if (datasComNotas.Contains(data.Value.Date))
                    return (Brush)new BrushConverter().ConvertFromString("#EBF5FB"); // azul clarinho
            }

            return Brushes.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private static DateTime? ExtrairData(object value)
        {
            if (value is DateTime dt)
                return dt;

            // DateTime? é boxado como object; tenta cast direto
            if (value is DateTime dtCast)
                return dtCast;

            // Tenta via Nullable<DateTime> unboxing seguro
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
}
