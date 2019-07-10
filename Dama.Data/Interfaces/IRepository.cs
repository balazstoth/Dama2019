using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dama.Data.Interfaces
{
    public interface IRepository<T>
    {
        Task AddAsync(T item);

        Task RemoveAsync(T item);

        Task RemoveRangeAsync(IEnumerable<T> item);

        Task UpdateAsync(T oldValue, T newValue);

        T GetEntityById(int id);

        Task<T> FindAsync(object value);

        IEnumerable<T> FindByPredicate(Expression<Func<T, bool>> predicate);

        IEnumerable<T> GetAllEntitites();
    }
}
