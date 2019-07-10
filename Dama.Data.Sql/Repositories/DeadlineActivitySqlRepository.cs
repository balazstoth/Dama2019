using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class DeadlineActivitySqlRepository : SqlRepository<DeadlineActivity>
    {
        public DeadlineActivitySqlRepository(SqlConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
