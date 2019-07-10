using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class UserSqlRepository : SqlRepository<User>
    {
        public UserSqlRepository(SqlConfiguration config)
            :base(config)
        {
        }
    }
}
