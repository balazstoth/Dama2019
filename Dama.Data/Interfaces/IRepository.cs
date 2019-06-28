using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dama.Data
{
    interface IRepository<T> where T : IEntity
    {
        void Add(T item);
        void Delete(T item);
        void Update();
        T GetEntityById(int id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
    }
}
