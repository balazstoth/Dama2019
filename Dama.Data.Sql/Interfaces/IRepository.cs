using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dama.Data.Sql.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> Get(Expression<Func<T, bool>> filter = null,
                           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                           params Expression<Func<T, object>>[] includeProperties);

        T GetById(object id);

        void Insert(T entity);

        void Delete(object id);

        void DeleteRange(IEnumerable<T> itemsToRemove);

        void Delete(T entityToDelete);

        void Update(T entityToUpdate);
    }
}
