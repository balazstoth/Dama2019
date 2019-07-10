using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dama.Data.Interfaces;

namespace Dama.Data.Sql.SQL
{
    public class SqlRepository<T> : IRepository<T> where T: class
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
                T value = GetEntityById(oldValue.Id);
                value = newValue;
                await context.SaveChangesAsync();
            }
        }

        public IEnumerable<T> FindByPredicate(Expression<Func<T, bool>> predicate)
        {
            using (var context = new DamaContext(_config))
            {
                return context.Set<T>().Where(predicate).ToArray();
            }
        }

        public async Task<T> FindAsync(object value)
        {
            using (var context = new DamaContext(_config))
            {
                return await context.Set<T>().FindAsync(value);
            }
        }

        public T GetEntityById(int id)
        {
            using (var context = new DamaContext(_config))
            {
                return context.Set<T>().Where(item => item.Id == id).SingleOrDefault();
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
