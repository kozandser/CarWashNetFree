using LinqToDB.Mapping;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Domain.Model
{
    #region Enums
    public enum EntityStateEnum
    {
        [Description("На подготовке")] Preparing = 1,
        [Description("Готов")] Active = 2,
        [Description("Не используется")] Unused = 3,
        [Description("Удален")] Deleted = 4
    }
    #endregion

    #region Interfaces
    public interface IEntity
    {
        int ID { get; set; }
    }
    public interface IEntityWithState : IEntity
    {
        EntityStateEnum EntityState { get; set; }
    }
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
    public interface IEntityWithGroup
    {
        string Group { get; set; }
    }
    #endregion

    #region Classes
    public abstract class Entity : ReactiveObject, IEntity, ISelectable
    {
        [Identity, PrimaryKey] public int ID { get; set; }
        public bool IsNew => ID == 0;
        [Reactive] bool CanEdit { get; set; }
        [Reactive] public bool IsSelected { get; set; }
    }
    public abstract class EntityWithState : Entity, IEntityWithState
    {
        [Reactive][Column] public virtual EntityStateEnum EntityState { get; set; }       

        public EntityWithState()
        {
            EntityState = EntityStateEnum.Active;
        }
    }
    #endregion

    #region Exceptions
    public class DbCriticalException : Exception
    {
        public DbCriticalException() : base("БД повреждена") { }
        public DbCriticalException(string message) : base(message) { }
        public DbCriticalException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class DbConnectionException : Exception
    {
        public DbConnectionException() : base("Невозможно подключиться к БД") { }
        public DbConnectionException(string message) : base(message) { }
        public DbConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class DbConsistencyException : Exception
    {
        public DbConsistencyException() : base("БД повреждена") { }
        public DbConsistencyException(string message) : base(message) { }
        public DbConsistencyException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class DbLoginException : Exception
    {
        public DbLoginException() : base("Ошибка входа пользователя") { }
        public DbLoginException(string message) : base(message) { }
        public DbLoginException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException() : base("Исключение бизнес-логики") { }
        public BusinessLogicException(string message) : base(message) { }
        public BusinessLogicException(string message, Exception innerException) : base(message, innerException) { }
    }
    #endregion
}
