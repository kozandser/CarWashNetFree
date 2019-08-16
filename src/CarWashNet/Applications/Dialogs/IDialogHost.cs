using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarWashNet.Applications.Dialogs
{
    public interface IDialogHost
    {
        object DataContext { get; set; }
        Task OnCriticalErrorAsync(string message);
        void Logout();

    }   
}
