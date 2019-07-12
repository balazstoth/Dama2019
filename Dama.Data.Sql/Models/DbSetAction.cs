using Dama.Data.Models;
using System;
using System.Data.Entity;

namespace Dama.Data.Sql.Models
{
    public class DbSetAction
    {
        public Action<DbSet<FixedActivity>> FixedActivityAction { get; set; }

        public Action<DbSet<UnfixedActivity>> UnfixedActivityAction { get; set; }

        public Action<DbSet<UndefinedActivity>> UndefinedActivityAction { get; set; }

        public Action<DbSet<DeadlineActivity>> DeadlineActivityAction { get; set; }
    }
}
