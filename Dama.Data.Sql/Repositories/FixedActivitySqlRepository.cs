using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class FixedActivitySqlRepository : SqlRepository<FixedActivity>
    {
        public FixedActivitySqlRepository(SqlConfiguration configuration)
            :base(configuration)
        {
        }
    }
}
