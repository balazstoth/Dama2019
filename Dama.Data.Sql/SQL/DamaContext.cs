using Dama.Data.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Dama.Data.Sql.SQL
{
    public class DamaContext : IdentityDbContext<User>
    {
        SqlConfiguration _config;

        public DamaContext(SqlConfiguration config)
            :base(config.ConnectionString)
        {
            _config = config;
        }

        public DbSet<FixedActivity> FixedActivities { get; set; }
        public DbSet<UnfixedActivity> UnFixedActivities { get; set; }
        public DbSet<DeadlineActivity> DeadLineActivities { get; set; }
        public DbSet<UndefinedActivity> UndefinedActivities { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
    }
}
