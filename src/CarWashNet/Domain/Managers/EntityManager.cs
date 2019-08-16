using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Validation;
using FluentValidation;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarWashNet.Domain.Managers
{
    //public interface IEntityManager<T> where T : IEntity
    //{
    //    //IEnumerable<T> GetAll();
    //    //T Get(int id);
    //}
    //public interface IEntityWithStateManager<T> : IEntityManager<T> where T : class, IEntityWithState
    //{
    //    void Lock(T entity);
    //    void Lock(IEnumerable<T> entities);
    //    void Unlock(T entity);
    //    void Unlock(IEnumerable<T> entities);
    //    void Delete(T entity, bool hardDelete = false);
    //    void Delete(IEnumerable<T> entities, bool hardDelete = false);
    //}

    public class EntityManager
    {
        protected CarWashDb _db;
        public EntityManager(CarWashDb db)
        {
            _db = db;
        }
        #region CRUD
        protected int InsertToDb<T>(T entity) where T : IEntity
        {
            var i = _db.InsertWithIdentity(entity);
            entity.ID = Convert.ToInt32(i);
            return entity.ID;
        }
        protected int UpdateToDb<T>(T entity) where T : IEntity
        {
            return _db.Update(entity);
        }
        protected int InsertOrUpdateToDb<T>(T entity) where T : IEntity
        {
            if (entity.ID == 0) return InsertToDb(entity);
            else return UpdateToDb(entity);
        }
        protected int DeleteFromDb<T>(T entity) where T : IEntity
        {
            return _db.Delete(entity);
        }
        #endregion
        protected void SetEntityState<T>(T entity, EntityStateEnum state) where T : class, IEntityWithState
        {
            if (entity.EntityState == state) return;
            _db.GetTable<T>().Where(p => p.ID == entity.ID)
                .Set(p => p.EntityState, state)
                .Update();
            entity.EntityState = state;
        }

        public virtual void Save<T>(T entity) where T : IEntity
        {
            InsertOrUpdateToDb(entity);
        }        
        public virtual void Lock<T>(T entity) where T : class, IEntityWithState
        {
            SetEntityState(entity, EntityStateEnum.Unused);
        }
        public virtual void Lock<T>(IEnumerable<T> entities) where T : class, IEntityWithState
        {
            foreach (var entity in entities)
            {
                Lock(entity);
            }
        }
        public virtual void Unlock<T>(T entity) where T : class, IEntityWithState
        {
            SetEntityState(entity, EntityStateEnum.Active);
        }
        public virtual void Unlock<T>(IEnumerable<T> entities) where T : class, IEntityWithState
        {
            foreach (var entity in entities)
            {
                Unlock(entity);
            }
        }
        public virtual void Delete<T>(T entity, bool hardDelete = false) where T : class, IEntityWithState
        {
            if (hardDelete) DeleteFromDb(entity);
            else SetEntityState(entity, EntityStateEnum.Deleted);
        }
        public virtual void Delete<T>(IEnumerable<T> entities, bool hardDelete = false) where T : class, IEntityWithState
        {
            foreach (var entity in entities)
            {
                try
                {
                    Delete(entity, hardDelete);
                }
                catch
                {

                }

            }
        }

        public List<string> GetGroups<T>() where T : class, IEntity, IEntityWithGroup
        {
            return _db.GetTable<T>()
                .Select(p => p.Group)
                .ToList()
                .Where(p => String.IsNullOrEmpty(p) == false)
                .OrderBy(p => p)
                .Distinct()
                .ToList();
        }
        public void SetGroup<T>(IEnumerable<T> entities, string group) where T : class, IEntity, IEntityWithGroup
        {
            _db.BeginTransaction();
            foreach (var entity in entities)
            {
                _db.GetTable<T>()
                    .Where(p => p.ID == entity.ID)
                    .Set(p => p.Group, p => group)
                    .Update();
            }
            _db.CommitTransaction();
        }        
    }

    public class EntityManager<TEntity, TValidator> : EntityManager where TEntity : class, IEntityWithState where TValidator : DbValidator<TEntity>, new()
    {
        public EntityManager(CarWashDb db) : base(db) { }

        public virtual void ValidateAndSave(TEntity entity)
        {
            entity.ValidateAndThrow<TValidator, TEntity>(_db);
            base.Save(entity);
        }
        public virtual void ValidateAndSetState(TEntity entity, EntityStateEnum newstate)
        {
            if (newstate == EntityStateEnum.Preparing) entity.ValidateAndThrow<TValidator, TEntity>("Preparing", _db);
            else if (newstate == EntityStateEnum.Active) entity.ValidateAndThrow<TValidator, TEntity>("Active", _db);
            else if (newstate == EntityStateEnum.Unused) entity.ValidateAndThrow<TValidator, TEntity>("Unused", _db);
            else if (newstate == EntityStateEnum.Deleted) entity.ValidateAndThrow<TValidator, TEntity>("Deleted", _db);

            SetEntityState(entity, newstate);
        }
        public virtual void ValidateAndDelete(TEntity entity, bool hardDelete = false)
        {
            entity.ValidateAndThrow<TValidator, TEntity>("Delete", _db);
            base.Delete(entity, hardDelete);
        }
        public virtual void ValidateAndDelete(IEnumerable<TEntity> entities, bool hardDelete = false)
        {
            foreach (var entity in entities)
            {
                try
                {
                    ValidateAndDelete(entity, hardDelete);
                }
                catch
                {

                }

            }
        }
    }
}
