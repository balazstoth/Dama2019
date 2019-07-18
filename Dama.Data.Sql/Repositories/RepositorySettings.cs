using System.Data.Entity;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class RepositorySettings : IRepositorySettings
    {
        private readonly SqlConfiguration _config;

        public RepositorySettings(SqlConfiguration config)
        {
            _config = config;
        }

        public void ChangeCategoryEntryState(Category category, EntityState entityState)
        {
            using (var context = new DamaContext(_config))
            {
                context.Entry(category).State = entityState;
                context.SaveChanges();
            }
        }
    }
}
