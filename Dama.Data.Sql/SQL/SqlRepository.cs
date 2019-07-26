﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dama.Data.Interfaces;

namespace Dama.Data.Sql.SQL
{
    public class SqlRepository<T> : IRepository<T> where T : class, IEntity
    {
        public async Task AddAsync(T item)
        {
            using (var context = new DamaContext())
            {
                context.Set<T>().Attach(item);
                context.Set<T>().Add(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(T item)
        {
            using (var context = new DamaContext())
            {
                context.Set<T>().Attach(item);
                context.Set<T>().Remove(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveRangeAsync(IEnumerable<T> collection)
        {
            using (var context = new DamaContext())
            {
                foreach (var item in collection)
                    context.Set<T>().Attach(item);

                context.Set<T>().RemoveRange(collection);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(T newValue)
        {
            using (var context = new DamaContext())
            {
                context.Set<T>().Attach(newValue);
                var entry = context.Entry(newValue);
                entry.State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }

        //public IEnumerable<T> FindByPredicate(Predicate<T> predicate, Expression<Func<IQueryable<T>, IEnumerable<T>>> expression)
        //{
        //    using (var context = new DamaContext())
        //    {
        //        Expression<Func<T, bool>> ex = a => predicate(a);
        //        var filteredResult = context.Set<T>().Where(ex);
        //        var func = expression.Compile();
        //        return func(filteredResult);
        //    }
        //}

        public IEnumerable<T> FindByPredicate(Predicate<T> predicate)
        {
            using (var context = new DamaContext())
            {
                Expression<Func<T, bool>> expression = a => predicate(a);
                var c = expression.Compile();
                return context.Set<T>().Where(c).ToList();
            }
        }

        public Task<List<T>> FindByExpressionAsync(Expression<Func<DbSet<T>, Task<List<T>>>> expression)
        {
            using (var context = new DamaContext())
            {
                var func = expression.Compile();
                return func(context.Set<T>());
            }
        }

        public async Task<T> FindAsync(string value)
        {
            var parsed = int.Parse(value);
            using (var context = new DamaContext())
            {
                return await context.Set<T>().FindAsync(parsed);
            }
        }

        public T GetEntityById(string id)
        {
            using (var context = new DamaContext())
            {
                return context.Set<T>().Where(item => item.Id == int.Parse(id)).SingleOrDefault();
            }
        }

        public IEnumerable<T> GetAllEntitites()
        {
            using (var context = new DamaContext())
            {
                return context.Set<T>().ToArray();
            }
        }
    }
}
