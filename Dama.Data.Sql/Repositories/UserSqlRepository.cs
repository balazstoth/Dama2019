using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class UserSqlRepository : IUserRepository
    {
        public async Task AddAsync(User item)
        {
            using (var context = new DamaContext())
            {
                context.Set<User>().Add(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(User item)
        {
            using (var context = new DamaContext())
            {
                context.Set<User>().Remove(item);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveRangeAsync(IEnumerable<User> collection)
        {
            using (var context = new DamaContext())
            {
                context.Set<User>().RemoveRange(collection);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(User oldValue, User newValue)
        {
            using (var context = new DamaContext())
            {
                User value = GetEntityById(oldValue.Id);
                value = newValue;
                await context.SaveChangesAsync();
            }
        }

        public IEnumerable<User> FindByPredicate(Expression<Func<User, bool>> predicate)
        {
            using (var context = new DamaContext())
            {
                return context.Set<User>().Where(predicate).ToArray();
            }
        }

        public async Task<User> FindAsync(object value)
        {
            using (var context = new DamaContext())
            {
                return await context.Set<User>().FindAsync(value);
            }
        }

        public User GetEntityById(string id)
        {
            using (var context = new DamaContext())
            {
                return context.Set<User>().Where(item => item.Id == id).SingleOrDefault();
            }
        }

        public IEnumerable<User> GetAllEntitites()
        {
            using (var context = new DamaContext())
            {
                return context.Set<User>().ToArray();
            }
        }

        public Task<List<User>> FindByExpressionAsync(Expression<Func<DbSet<User>, Task<List<User>>>> expression)
        {
            using (var context = new DamaContext())
            {
                var func = expression.Compile();
                return func(context.Set<User>());
            }
        }

        public IEnumerable<User> FindByPredicate(Predicate<User> predicate)
        {
            using (var context = new DamaContext())
            {
                Expression<Func<User, bool>> expression = a => predicate(a);
                return context.Set<User>().Where(expression).ToList();
            }
        }
    }
}
