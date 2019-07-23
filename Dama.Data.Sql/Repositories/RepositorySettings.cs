using System.Data.Entity;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class RepositorySettings : IRepositorySettings
    {
        public void ChangeCategoryEntryState(Category category, EntityState entityState)
        {
            using (var context = new DamaContext())
            {
                context.Entry(category).State = entityState;
                context.SaveChanges();
            }
        }
    }
}
