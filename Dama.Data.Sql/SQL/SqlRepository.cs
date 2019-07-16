using System;
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
        private readonly SqlConfiguration _config;

        public SqlRepository(SqlConfiguration config)
        {
            _config = config;
        }

        public async Task AddAsync(T item)
        {
            using (var context = new DamaContext(_config))
            {
                context.Set<T>().Add(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(T item)
        {
            using (var context = new DamaContext(_config))
            {
                context.Set<T>().Remove(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveRangeAsync(IEnumerable<T> collection)
        {
            using (var context = new DamaContext(_config))
            {
                context.Set<T>().RemoveRange(collection);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(T oldValue, T newValue)
        {
            using (var context = new DamaContext(_config))
            {
                T value = GetEntityById(oldValue.Id.ToString());
                value = newValue;
                await context.SaveChangesAsync();
            }
        }

        //public IEnumerable<T> FindByPredicate(Predicate<T> predicate, Expression<Func<IQueryable<T>, IEnumerable<T>>> expression)
        //{
        //    using (var context = new DamaContext(_config))
        //    {
        //        Expression<Func<T, bool>> ex = a => predicate(a);
        //        var filteredResult = context.Set<T>().Where(ex);
        //        var func = expression.Compile();
        //        return func(filteredResult);
        //    }
        //}

        public IEnumerable<T> FindByPredicate(Predicate<T> predicate)
        {
            using (var context = new DamaContext(_config))
            {
                Expression<Func<T, bool>> expression = a => predicate(a);
                return context.Set<T>().Where(expression).ToList();
            }
        }

        public Task<List<T>> FindByExpressionAsync(Expression<Func<DbSet<T>, Task<List<T>>>> expression)
        {
            using (var context = new DamaContext(_config))
            {
                var func = expression.Compile();
                return func(context.Set<T>());
            }
        }

        public async Task<T> FindAsync(object value)
        {
            using (var context = new DamaContext(_config))
            {
                return await context.Set<T>().FindAsync(value);
            }
        }

        public T GetEntityById(string id)
        {
            using (var context = new DamaContext(_config))
            {
                return context.Set<T>().Where(item => item.Id == int.Parse(id)).SingleOrDefault();
            }
        }

        public IEnumerable<T> GetAllEntitites()
        {
            using (var context = new DamaContext(_config))
            {
                return context.Set<T>().ToArray();
            }
        }
    }
}
