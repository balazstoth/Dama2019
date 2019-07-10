using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dama.Data.Interfaces
{
    public interface IRepository<T> where T : IEntity
    {
        Task AddAsync(T item);

        Task DeleteAsync(T item);

        Task UpdateAsync(T oldValue, T newValue);

        T GetEntityById(string id);

        Task<T> FindAsync(object value);

        IEnumerable<T> FindByPredicate(Expression<Func<T, bool>> predicate);

        IEnumerable<T> GetAllEntitites();
    }
}
