﻿using Dama.Data.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class UndefinedActivitySqlRepository : SqlRepository<UndefinedActivity>
    {
        public UndefinedActivitySqlRepository(SqlConfiguration configuration)
            :base(configuration)
        {
        }
    }
}