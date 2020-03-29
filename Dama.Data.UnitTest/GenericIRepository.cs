using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dama.Data.UnitTest
{
    public class GenericIRepository<T> : IRepository<T> where T : class, IEntity
    {
        private List<T> _collection;

        public GenericIRepository(IEnumerable<T> collection)
        {
            _collection = collection.ToList();
        }

        public void Delete(object id)
        {
            _collection.Remove(_collection.Where(i => i.Id == (int)id).SingleOrDefault());
        }

        public void Delete(T entityToDelete)
        {
            _collection.Remove(entityToDelete);
        }

        public void DeleteRange(IEnumerable<T> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
                _collection.Remove(item);
        }

        public IEnumerable<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includeProperties)
        {
            var items = _collection.AsQueryable<T>();

            if (filter != null)
                items = items.Where(filter);

            if (orderBy != null)
                items = orderBy(items);

            return items.AsEnumerable<T>();
        }

        public T GetByID(object id)
        {
           return _collection.SingleOrDefault(i => i.Id == (int)id);
        }

        public void Insert(T entity)
        {
            _collection.Add(entity);
        }

        public void Update(T entityToUpdate)
        {
            Delete(GetByID(entityToUpdate.Id));
            _collection.Add(entityToUpdate);
        }
    }
}
