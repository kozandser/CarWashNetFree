using CarWashNet.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Domain.Repository
{
    public static class DbExtensions
    {
        public static IQueryable<T> OnlyNotDeleted<T>(this IQueryable<T> items) where T : IEntityWithState
        {
            return items.Where(p => p.EntityState != EntityStateEnum.Deleted);
        }
        public static IQueryable<T> OnlyActive<T>(this IQueryable<T> items) where T : IEntityWithState
        {
            return items.Where(p => p.EntityState == EntityStateEnum.Active);
        }
        public static IQueryable<T> OnlyActive<T>(this IQueryable<T> items, int id) where T : IEntityWithState
        {
            return items.Where(p => p.EntityState == EntityStateEnum.Active || p.ID == id);
        }
        public static IEnumerable<T> OnlySelected<T>(this IEnumerable<T> items) where T : ISelectable
        {
            return items.Where(p => p.IsSelected == true);
        }        
    }
}
