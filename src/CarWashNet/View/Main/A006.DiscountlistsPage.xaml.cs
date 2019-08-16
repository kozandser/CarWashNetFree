﻿using CarWashNet.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarWashNet.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserAppBindingsPage.xaml
    /// </summary>
    public partial class DiscountlistsPage : Page, IViewFor<DiscountlistsViewModel>
    {
        bool isManualEditCommit;
        protected bool firstRun = true;
        public DiscountlistsViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (DiscountlistsViewModel)value; } }
        public DiscountlistsPage()
        {
            InitializeComponent();
            ViewModel = new DiscountlistsViewModel();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                    x.DataContext)
                    .DisposeWith(disposables);

                if (firstRun)
                {
                    ViewModel.Init(0);
                    firstRun = false;
                }
            });
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!isManualEditCommit)
            {
                isManualEditCommit = true;
                dg1.CommitEdit(DataGridEditingUnit.Row, true);
                isManualEditCommit = false;
                ViewModel.ServicesViewModel.SaveDiscount.Execute().Subscribe();
            }
        }
    }
}