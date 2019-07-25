using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dama.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T item);

        Task RemoveAsync(T item);

        Task RemoveRangeAsync(IEnumerable<T> item);

        Task UpdateAsync(T oldValue, T newValue);

        T GetEntityById(string id);

        Task<T> FindAsync(string value);

        Task<List<T>> FindByExpressionAsync(Expression<Func<DbSet<T>, Task<List<T>>>> expression);

        IEnumerable<T> FindByPredicate(Predicate<T> predicate);

        IEnumerable<T> GetAllEntitites();
    }
}
