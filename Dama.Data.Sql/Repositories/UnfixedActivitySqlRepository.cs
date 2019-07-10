using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class UnfixedActivitySqlRepository : SqlRepository<UnfixedActivity>
    {
        public UnfixedActivitySqlRepository(SqlConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
