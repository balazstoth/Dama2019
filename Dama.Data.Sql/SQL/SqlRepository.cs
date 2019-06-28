using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dama.Data.Interfaces;

namespace Dama.Data.Sql.SQL
{
    public class SqlRepository<T> : IRepository<T> where T: class, IEntity
    {
        private readonly SqlConfiguration _config;

        public SqlRepository(SqlConfiguration config)
        {
            _config = config;
        }

        public void Add(T item)
        {
            using (var context = new DamaContext(_config))
            {
                context.Set<T>().Add(item);
                context.SaveChanges();
            }
        }

        public void Delete(T item)
        {
            using (var context = new DamaContext(_config))
            {
                context.Set<T>().Remove(item);
                context.SaveChanges();
            }
        }

        public void Update(T oldValue, T newValue)
        {
            using (var context = new DamaContext(_config))
            {
                T value = GetEntityById(oldValue.Id);
                value = newValue;
                context.SaveChanges();
            }
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            using (var context = new DamaContext(_config))
            {
                return context.Set<T>().Where(predicate).ToArray();
            }
        }

        public T GetEntityById(string id)
        {
            using (var context = new DamaContext(_config))
            {
                return context.Set<T>().Where(item => item.Id == id).SingleOrDefault();
            }
        }
    }
}
