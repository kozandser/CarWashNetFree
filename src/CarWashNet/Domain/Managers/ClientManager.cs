using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Validation;
using KLib.FluentValidation;
using KLib.Native;

namespace CarWashNet.Domain.Managers
{
    //public class ClientManager : EntityManager<Client>
    //{
    //    public ClientManager(CarWashDb db) : base(db) { }

    //    public override void ValidateAndSave(Client entity)
    //    {
    //        entity.ValidateAndThrow<ClientValidator, Client>();
    //        base.Save(entity);
    //    }
    //    public void ValidateAndSetState(Client entity, EntityStateEnum newstate)
    //    {
    //        if (newstate == EntityStateEnum.Preparing) entity.ValidateAndThrow<ClientValidator, Client>("Preparing");
    //        else if (newstate == EntityStateEnum.Active) entity.ValidateAndThrow<ClientValidator, Client>("Active");
    //        else if (newstate == EntityStateEnum.Unused) entity.ValidateAndThrow<ClientValidator, Client>("Unused");
    //        else if (newstate == EntityStateEnum.Deleted) entity.ValidateAndThrow<ClientValidator, Client>("Deleted");

    //        SetEntityState(entity, newstate);
    //    }
    //    public override void ValidateAndDelete(Client entity, bool hardDelete = false)
    //    {
    //        entity.ValidateAndThrow<ClientValidator, Client>("Delete");
    //        base.Delete(entity, hardDelete);
    //    }        
    //}

    
}
