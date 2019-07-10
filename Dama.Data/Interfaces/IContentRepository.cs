using Dama.Data.Models;

namespace Dama.Data.Interfaces
{
    public interface IContentRepository
    {
        IRepository<FixedActivity> FixedActivitySqlRepository { get; set; }

        IRepository<UnfixedActivity> UnfixedActivitySqlRepository { get; set; }

        IRepository<UndefinedActivity> UndefinedActivitySqlRepository { get; set; }

        IRepository<DeadlineActivity> DeadlineActivitySqlRepository { get; set; }

        IRepository<Label> LabelSqlRepository { get; set; }

        IRepository<Category> CategorySqlRepository { get; set; }

        IRepository<Milestone> MilestoneSqlRepository { get; set; }
    }
}
