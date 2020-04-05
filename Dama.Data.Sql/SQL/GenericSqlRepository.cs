using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Dama.Data.Interfaces;
using Dama.Data.Sql.Interfaces;
using LinqKit;

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
            var list = itemsToRemove.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                Delete(list[i]);
            }
        }

        public IEnumerable<T> Get(Expression<Func<T, bool>> filter = null,
                                 Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                 params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;

            if (includeProperties != null)
                foreach (var prop in includeProperties)
                    query = query.Include(prop);

            if (filter != null)
            {
                var func = filter.Compile();
                query = query.AsExpandable().Where(func).AsQueryable();
            }

            if (orderBy != null)
                query = orderBy(query);

            return query.ToList();
        }

        public virtual T GetById(object id)
        {
            int parsedId;

            if(int.TryParse(id.ToString(), out parsedId))
                return dbSet.Find(parsedId);

            return dbSet.Find(id);
        }
    }
}
