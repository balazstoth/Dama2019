﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class UserSqlRepository : IRepository<User>
    {
        internal DamaContext context;
        internal DbSet<User> dbSet;

        public UserSqlRepository(DamaContext context)
        {
            this.context = context;
            dbSet = context.Set<User>();
        }

        public virtual void Insert(User entity)
        {
            dbSet.Add(entity);
        }

        public virtual void Update(User entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void Delete(object id)
        {
            var entity = dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Delete(User entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
                dbSet.Attach(entityToDelete);

            dbSet.Remove(entityToDelete);
        }

        public void DeleteRange(IEnumerable<User> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
                Delete(item);
        }

        public IEnumerable<User> Get(Expression<Func<User, bool>> filter = null,
                                 Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null,
                                 string includeProperties = "")
        {
            IQueryable<User> query = dbSet;

            if (filter != null)
                query = query.Where(filter);

            var properties = includeProperties.Split(',');

            foreach (var includeProperty in properties)
                query = query.Include(includeProperty);

            if (orderBy != null)
                return orderBy(query).ToList();

            return query.ToList();
        }

        public virtual User GetByID(object id)
        {
            return dbSet.Find(id);
        }
    }
}
