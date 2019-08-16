using KLib.Native;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarWashNet.Apps
{
    [TemplatePart(Name = "PART_RootGrid", Type = typeof(Grid))]
    public class BaseApp : ContentControl
    {
        public string Caption { get; set; }
        public ICommand GoBack { get; set; }

        static BaseApp()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseApp), new FrameworkPropertyMetadata(typeof(BaseApp)));
        }

        protected bool isFirstInited = false;
        protected Page startPage;
        public Frame MainFrame { get; private set; }
        public Grid RootGrid { get; set; }

        //Window mainWindow;

        public BaseApp(string title)
        {
            Caption = title;
            this.DataContext = this;

            MainFrame = new Frame();
            MainFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;

            GoBack = new RelayCommand(
                exec =>
                {
                    MainFrame.GoBack();
                },
                canexec =>
                {
                    return MainFrame.CanGoBack;
                });
        }
        protected void navigateStartPage()
        {
            if (startPage == null) throw new System.InvalidOperationException("Не создана начальная страница");
            if(MainFrame == null) throw new System.InvalidOperationException("Не создана начальная страница");
            startPage.Title = Caption;
            MainFrame.Navigate(startPage);            
        }
        protected void createAndNavigateToStartPage(Page page)
        {
            startPage = page;
            navigateStartPage();
        }

        //public void Init()
        //{
        //    isFirstInited = true;
        //}
        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    mainWindow = System.Windows.Application.Current.MainWindow;
        //    RootGrid = base.Template.FindName("PART_RootGrid", this) as Grid;
        //}
    }
}
