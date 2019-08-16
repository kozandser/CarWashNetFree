using CarWashNet.Applications;
using CarWashNet.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarWashNet.WPF
{
    public partial class SplashWindow
    {
        public SplashWindow()
        {
            InitializeComponent();

        }

        private async void MetroWindow_LoadedAsync(object sender, RoutedEventArgs e)
        {
            message.Text = "Подключение к БД...";
            DbService.Init();
            await DbService.ConnectToDbAsync();            
            message.Text = "Загрузка настроек...";
            await GlobalService.InitAsync();                        
            var w = new MainWindow();
            await Task.Delay(1000);
            message.Text = "Запуск программы...";
            w.Show();
            Close();
        }
    }
}
