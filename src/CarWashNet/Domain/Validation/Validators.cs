using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Services;
using FluentValidation;
using LinqToDB;
using System.Linq;

namespace CarWashNet.Domain.Validation
{
    public abstract class DbValidator<T> : AbstractValidator<T>
    {
        protected CarWashDb _db;
        public DbValidator()
        {

        }
        public DbValidator(CarWashDb db)
        {
            _db = db;
        }
        public void SetDb(CarWashDb db)
        {
            _db = db;
        }
    }

    public class AppItemValidator : DbValidator<AppItem>
    {
        public AppItemValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");
            RuleFor(p => p.Code).NotEmpty().WithMessage("Код не должен быть пустым");
        }
    }
    public class UserValidator : DbValidator<User>
    {
        public UserValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");
            RuleSet("Unused", () =>
            {
                RuleFor(p => p.IsAdmin).Equal(false).WithMessage("Нельзя заблокировать этого пользователя");
            });
            RuleSet("Delete", () =>
            {
                RuleFor(p => p.IsAdmin).Equal(false).WithMessage("Нельзя удалить этого пользователя");
                RuleFor(s => s.ID)
                    .Must(s =>
                    {
                        var orders = _db.Orders.Where(p => p.UserID == s);
                        return orders.Count() == 0;
                    })
                    .WithMessage("Пользователь используется в заказах и его нельзя удалить");
            });
        }
    }
    public class ServiceValidator : DbValidator<Service>
    {        
        public ServiceValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");

            RuleSet("Delete", () =>
            {
                RuleFor(s => s.ID)
                    .Must(s =>
                    {
                        var pricelistitems = _db.PricelistItems
                                .LoadWith(p => p.Pricelist)
                                .Where(p => p.ServiceID == s)
                                .ToList();

                        return !(pricelistitems.Any(p => p.Pricelist.EntityState != EntityStateEnum.Deleted));
                    })
                    .WithMessage("Услуга используется в прайслистах и ее нельзя удалить")
                    .Must(s =>
                    {
                        var discountitems = _db.DiscountlistItems
                                .LoadWith(p => p.Discountlist)
                                .Where(p => p.ServiceID == s)
                                .ToList();

                        return !(discountitems.Any(p => p.Discountlist.EntityState != EntityStateEnum.Deleted));
                    })
                    .WithMessage("Услуга используется в скидках и ее нельзя удалить")
                    .Must(s =>
                    {
                        var orders = _db.OrderItems.Where(p => p.ServiceID == s);
                        return orders.Count() == 0;
                    })
                    .WithMessage("Услуга используется в заказах и ее нельзя удалить");
            });
        }
    }
    public class WorkerValidator : DbValidator<Worker>
    {
        public WorkerValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");            
            RuleSet("Delete", () =>
            {
                RuleFor(s => s.ID)
                    .Must(s =>
                    {
                        var orders = _db.Orders.Where(p => p.WorkerID == s);
                        return orders.Count() == 0;
                    })
                    .WithMessage("Работник используется в заказах и его нельзя удалить");
            });
        }
    }
    public class PricelistValidator : DbValidator<Pricelist>
    {
        public PricelistValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");
            RuleSet("Delete", () =>
            {

            });
        }
    }
    public class DiscountlistValidator : DbValidator<Discountlist>
    {
        public DiscountlistValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");
            RuleSet("Delete", () =>
            {
                RuleFor(s => s.ID)
                    .Must(s =>
                    {
                        var clients = _db.Clients
                                .OnlyNotDeleted()
                                .Where(p => p.DiscountlistID == s)
                                .ToList();

                        return clients.Count == 0;
                    })
                    .WithMessage("Скидка закреплена за некоторыми клиентами и ее нельзя удалить")
                    .Must(s =>
                    {
                        var orders = _db.Orders.Where(p => p.DiscountID == s);

                        return orders.Count() == 0;
                    })
                    .WithMessage("Скидка используется в заказах и ее нельзя удалить");
            });
        }
    }
    public class CarModelValidator : DbValidator<CarModel>
    {
        public CarModelValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");
            RuleSet("Delete", () =>
            {
                RuleFor(s => s.ID)
                    .Must(s =>
                    {
                        var cars = _db.Cars
                                .OnlyNotDeleted()
                                .Where(p => p.CarModelID == s)
                                .ToList();

                        return cars.Count == 0;
                    })
                    .WithMessage("Модель закреплена за некоторыми автомобилями и ее нельзя удалить");
            });
        }
    }
    public class ClientValidator : DbValidator<Client>
    {
        public ClientValidator()
        {
            RuleFor(p => p.Caption).NotEmpty().WithMessage("Наименование не должно быть пустым");
            RuleSet("Delete", () =>
            {
                RuleFor(s => s.ID)
                    .Must(s =>
                    {
                        var cars = _db.Cars
                                .OnlyNotDeleted()
                                .Where(p => p.ClientID == s)
                                .ToList();

                        return cars.Count == 0;
                    })
                    .WithMessage("Клиент закреплен за некоторыми автомобилями и его нельзя удалить")
                    .Must(s =>
                    {
                        var orders = _db.Orders.Where(p => p.ClientID == s);

                        return orders.Count() == 0;
                    })
                    .WithMessage("Клиент используется в заказах и его нельзя удалить");
            });
        }
    }
    public class CarValidator : DbValidator<Car>
    {
        public CarValidator()
        {
            RuleFor(p => p.FedCode).NotEmpty().WithMessage("Гос№ не должен быть пустым");
            RuleSet("CheckFedCode", () =>
            {
                RuleFor(s => s)
                .Must(s =>
                {
                    var cars = _db.Cars
                            .OnlyNotDeleted()
                            .Where(p => p.ID != s.ID && p.FedCode == s.FedCode)
                            .ToList();

                    return cars.Count == 0;
                })
                .WithMessage("Такой Гос№ уже существует");
            });
            RuleSet("Delete", () =>
            {
                RuleFor(s => s.ID)                    
                    .Must(s =>
                    {
                        var orders = _db.Orders.Where(p => p.ClientID == s);

                        return orders.Count() == 0;
                    })
                    .WithMessage("Автомобиль используется в заказах и его нельзя удалить");
            });
        }
    }
    public class OrderValidator : DbValidator<Order>
    {
        public OrderValidator()
        {
            //RuleFor(p => p.FedCode).NotEmpty().WithMessage("Гос№ не должен быть пустым");
            RuleSet("Close", () =>
            {
                RuleFor(p => p.WorkerID).NotNull().WithMessage("Выберите работника");
                RuleFor(p => p.CarID).NotNull().WithMessage("Выберите машину");
                RuleFor(p => p.InTime).LessThan(p => p.OutTime).WithMessage("Время заезда должно быть меньше времени выезда");
            });
            RuleSet("Delete", () =>
            {

            });
        }
    }
}
