using RevisaFacil.Data;
using RevisaFacil.Helpers;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace RevisaFacil
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Força o padrão brasileiro (pt-BR) em todos os elementos da interface (WPF)
            Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Inicializa o banco e verifica o Seed
            using (var db = new EstudoDbContext())
            {
                TemaManager.SeedDatabase(db);
            }
        }
    }
}