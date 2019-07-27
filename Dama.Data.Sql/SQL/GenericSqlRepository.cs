using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Dama.Data.Interfaces;
using Dama.Data.Sql.Interfaces;

namespace Dama.Data.Sql.SQL
{
    public class GenericSqlRepository<T> : IRepository<T> 
        where T : class, IEntity
    {
        internal DamaContext context;
        internal DbSet<T> dbSet;

        public GenericSqlRepository(DamaContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        public virtual void Insert(T entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Update(T entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void Delete(object id)
        {
            var entity = dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Delete(T entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
                dbSet.Attach(entityToDelete);
            
            dbSet.Remove(entityToDelete);
        }

        public void DeleteRange(IEnumerable<T> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
                Delete(item);
        }

        public IEnumerable<T> Get(Expression<Func<T, bool>> filter = null,
                                 Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                 string includeProperties = "")
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
                query = query.Where(filter);

            var properties = includeProperties.Split(',');

            foreach (var includeProperty in properties)
                query = query.Include(includeProperty);

            if (orderBy != null)
                return orderBy(query).ToList();

            return query.ToList();
        }

        public virtual T GetByID(object id)
        {
            return dbSet.Find(id);
        }
    }
}
