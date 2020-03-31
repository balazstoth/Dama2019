using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dama.Web.AccountControllerTest
{
    class UserSpecificIRepository : IRepository<User>
    {
        private List<User> _collection;

        public UserSpecificIRepository(IEnumerable<User> collection)
        {
            _collection = collection.ToList();
        }

        public void Delete(object id)
        {
            User u = new User();
            _collection.Remove(_collection.Where(i => i.Id == id.ToString()).SingleOrDefault());
        }

        public void Delete(User entityToDelete)
        {
            _collection.Remove(entityToDelete);
        }

        public void DeleteRange(IEnumerable<User> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
                _collection.Remove(item);
        }

        public IEnumerable<User> Get(Expression<Func<User, bool>> filter = null, Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null, params Expression<Func<User, object>>[] includeProperties)
        {
            var items = _collection.AsQueryable();

            if (filter != null)
                items = items.Where(filter);

            if (orderBy != null)
                items = orderBy(items);

            return items.AsEnumerable();
        }

        public User GetByID(object id)
        {
            return _collection.SingleOrDefault(i => i.Id == id.ToString());
        }

        public void Insert(User entity)
        {
            _collection.Add(entity);
        }

        public void Update(User entityToUpdate)
        {
            Delete(GetByID(entityToUpdate.Id));
            _collection.Add(entityToUpdate);
        }
    }
}
