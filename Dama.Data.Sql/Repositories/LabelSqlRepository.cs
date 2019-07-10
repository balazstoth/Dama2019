using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class LabelSqlRepository : SqlRepository<Label>
    {
        public LabelSqlRepository(SqlConfiguration configuration)
            :base(configuration)
        {
        }
    }
}
