using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dama.Data.Interfaces
{
    public interface IRepository<T> where T : IEntity
    {
        void Add(T item);
        void Delete(T item);
        void Update(T oldValue, T newValue);
        T GetEntityById(string id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
    }
}
