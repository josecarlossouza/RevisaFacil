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
            // values[0] é a data do card atual
            // values[1] é a lista de datas que possuem notas carregadas do banco
            if (values[0] is DateTime data && values[1] is HashSet<DateTime> datasComNotas)
            {
                if (datasComNotas.Contains(data.Date))
                {
                    // Cor de fundo se tiver anotação (ex: um azul bem clarinho ou amarelo suave)
                    return (Brush)new BrushConverter().ConvertFromString("#EBF5FB");
                }
            }
            return Brushes.White; // Fundo padrão se não tiver nota
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}