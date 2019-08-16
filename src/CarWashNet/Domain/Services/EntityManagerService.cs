using CarWashNet.Domain.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Domain.Services
{
    public static class EntityManagerService
    {
        private static Lazy<EntityManager> _entityManager = new Lazy<EntityManager>(() => new EntityManager(DbService.DefaultDb));
        public static EntityManager DefaultEntityManager => _entityManager.Value;

        private static Lazy<DbSettingManager> _dbSettingManager = new Lazy<DbSettingManager>(() => new DbSettingManager(DbService.DefaultDb));
        public static DbSettingManager DefaultDbSettingManager => _dbSettingManager.Value;

        private static Lazy<AppItemManager> _appItemManager = new Lazy<AppItemManager>(() => new AppItemManager(DbService.DefaultDb));
        public static AppItemManager DefaultAppItemManager => _appItemManager.Value;

        private static Lazy<UserManager> _userManager = new Lazy<UserManager>(() => new UserManager(DbService.DefaultDb));
        public static UserManager DefaultUserManager => _userManager.Value;

        private static Lazy<ServiceManager> _serviceManager = new Lazy<ServiceManager>(() => new ServiceManager(DbService.DefaultDb));
        public static ServiceManager DefaultServiceManager => _serviceManager.Value;

        private static Lazy<WorkerManager> _workerManager = new Lazy<WorkerManager>(() => new WorkerManager(DbService.DefaultDb));
        public static WorkerManager DefaultWorkerManager => _workerManager.Value;

        private static Lazy<PricelistManager> _pricelistManager = new Lazy<PricelistManager>(() => new PricelistManager(DbService.DefaultDb));
        public static PricelistManager DefaultPricelistManager => _pricelistManager.Value;

        private static Lazy<DiscountlistManager> _discountlistManager = new Lazy<DiscountlistManager>(() => new DiscountlistManager(DbService.DefaultDb));
        public static DiscountlistManager DefaultDiscountlistManager => _discountlistManager.Value;

        private static Lazy<CarModelManager> _carModelManager = new Lazy<CarModelManager>(() => new CarModelManager(DbService.DefaultDb));
        public static CarModelManager DefaultCarModelManager => _carModelManager.Value;

        private static Lazy<ClientManager> _clientManager = new Lazy<ClientManager>(() => new ClientManager(DbService.DefaultDb));
        public static ClientManager DefaultClientManager => _clientManager.Value;

        private static Lazy<CarManager> _carManager = new Lazy<CarManager>(() => new CarManager(DbService.DefaultDb));
        public static CarManager DefaultCarManager => _carManager.Value;

        private static Lazy<OrderManager> _orderManager = new Lazy<OrderManager>(() => new OrderManager(DbService.DefaultDb));
        public static OrderManager DefaultOrderManager => _orderManager.Value;
    }
}
