using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Validation;
using KLib.FluentValidation;
using KLib.Native;

namespace CarWashNet.Domain.Managers
{
    public class CarManager : EntityManager<Car>
    {
        public CarManager(CarWashDb db) : base(db) { }

        public override void ValidateAndSave(Car entity)
        {
            entity.ValidateAndThrow<CarValidator, Car>();
            base.Save(entity);
        }
        public void ValidateAndSetState(Car entity, EntityStateEnum newstate)
        {
            if (newstate == EntityStateEnum.Preparing) entity.ValidateAndThrow<CarValidator, Car>("Preparing");
            else if (newstate == EntityStateEnum.Active) entity.ValidateAndThrow<CarValidator, Car>("Active");
            else if (newstate == EntityStateEnum.Unused) entity.ValidateAndThrow<CarValidator, Car>("Unused");
            else if (newstate == EntityStateEnum.Deleted) entity.ValidateAndThrow<CarValidator, Car>("Deleted");

            SetEntityState(entity, newstate);
        }
        public override void ValidateAndDelete(Car entity, bool hardDelete = false)
        {
            entity.ValidateAndThrow<CarValidator, Car>("Delete");
            base.Delete(entity, hardDelete);
        }        
    }
}
