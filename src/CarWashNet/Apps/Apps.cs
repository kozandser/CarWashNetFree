using CarWashNet.Pages;
using CarWashNet.Application.Navigation;
using CarWashNet.View;

namespace CarWashNet.Apps
{
    public class A001 : BaseApp //Заказы
    {
        public A001(string title) : base(title)
        {
            createAndNavigateToStartPage(new OrdersPage(new ViewModel.OrdersViewModel(), "A001"));
        }
    }
    public class A002 : BaseApp //Управление пользователями
    {
        public A002(string title) : base(title)
        {
            createAndNavigateToStartPage(new UserAppBindingsPage());
        }
    }
    public class A003 : BaseApp //Работники
    {
        public A003(string title) : base(title)
        {
            createAndNavigateToStartPage(new WorkersPage());
        }
    }
    public class A004 : BaseApp //Услуги
    {
        public A004(string title) : base(title)
        {
            createAndNavigateToStartPage(new ServicesPage());
        }
    }
    public class A005 : BaseApp //Прайсы
    {
        public A005(string title) : base(title)
        {
            createAndNavigateToStartPage(new PricelistsPage());
        }
    }
    public class A006 : BaseApp //Скидки
    {
        public A006(string title) : base(title)
        {
            createAndNavigateToStartPage(new DiscountlistsPage());
        }
    }
    public class A008 : BaseApp //Модели автомобилей
    {
        public A008(string title) : base(title)
        {
            createAndNavigateToStartPage(new CarModelsPage());
        }
    }
    public class A009 : BaseApp //Автомобили
    {
        public A009(string title) : base(title)
        {
            createAndNavigateToStartPage(new CarsPage(new ViewModel.CarsViewModel(), "A009"));
        }
    }
    public class A010 : BaseApp //Клиенты
    {
        public A010(string title) : base(title)
        {
            createAndNavigateToStartPage(new ClientsPage());
        }
    }
    public class A011 : BaseApp //Отчеты
    {
        public A011(string title) : base(title)
        {
            createAndNavigateToStartPage(new ReportsPage());
        }
    }
    public class A012 : BaseApp //Настройки БД
    {
        public A012(string title) : base(title)
        {
            createAndNavigateToStartPage(new DbSettingsPage());
        }
    }

    public class XAppSettings : BaseApp //Настройки приложения
    {
        public XAppSettings(string title) : base(title)
        {
            createAndNavigateToStartPage(new AppSettingsPage());
        }

    }
}
