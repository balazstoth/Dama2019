using Dama.Data.Interfaces;
using Dama.Data.Models;

namespace Dama.Data.Sql.SQL
{
    public class ContentRepository : IContentRepository
    {
        public IRepository<FixedActivity> FixedActivitySqlRepository { get; set; }

        public IRepository<UnfixedActivity> UnfixedActivitySqlRepository { get; set; }

        public IRepository<UndefinedActivity> UndefinedActivitySqlRepository { get; set; }

        public IRepository<DeadlineActivity> DeadlineActivitySqlRepository { get; set; }

        public IRepository<Label> LabelSqlRepository { get; set; }

        public IRepository<Category> CategorySqlRepository { get; set; }

        public IRepository<Milestone> MilestoneSqlRepository { get; set; }

        public IRepositorySettings RepositorySettings { get; set; }
    }
}
