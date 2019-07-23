using Dama.Data.Interfaces;
using Dama.Data.Models;

namespace Dama.Data.Sql.Repositories
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

        public ContentRepository(IRepository<FixedActivity> fixedActivitySqlRepository,
                                 IRepository<UnfixedActivity> unfixedActivitySqlRepository,
                                 IRepository<UndefinedActivity> undefinedActivitySqlRepository,
                                 IRepository<DeadlineActivity> deadlineActivitySqlRepository,
                                 IRepository<Label> labelSqlRepository,
                                 IRepository<Category> categorySqlRepository,
                                 IRepository<Milestone> milestoneSqlRepository,
                                 IRepositorySettings repositorySettings)
        {
            FixedActivitySqlRepository = fixedActivitySqlRepository;
            UnfixedActivitySqlRepository = unfixedActivitySqlRepository;
            UndefinedActivitySqlRepository = undefinedActivitySqlRepository;
            DeadlineActivitySqlRepository = deadlineActivitySqlRepository;
            LabelSqlRepository = labelSqlRepository;
            CategorySqlRepository = categorySqlRepository;
            MilestoneSqlRepository = milestoneSqlRepository;
            RepositorySettings = repositorySettings;
        }
    }
}
