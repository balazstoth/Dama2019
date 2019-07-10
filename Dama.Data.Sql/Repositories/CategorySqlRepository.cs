using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class CategorySqlRepository : SqlRepository<Category>
    {
        public CategorySqlRepository(SqlConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
