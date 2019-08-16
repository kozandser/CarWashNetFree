using CarWashNet.Applications;
using CarWashNet.Infrastructure;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace CarWashNet.WPF
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(
                    CultureInfo.CurrentCulture.IetfLanguageTag)));

            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            GlobalService.AppSettings.Save();
            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.Data["OnStartup"] != null && (bool)e.Exception.Data["OnStartup"] == true)
            {
                e.Handled = true;
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"Непредвиденная ошибка: {e.Exception.Message}");
                sb.Append(e.Exception.StackTrace);
                var res = MessageBox.Show(sb.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
                if (res == MessageBoxResult.OK) System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
