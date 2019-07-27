using Dama.Data.Models;
using Dama.Data.Sql.Repositories;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Interfaces
{
    public interface IUnitOfWork
    {
        GenericSqlRepository<FixedActivity> FixedActivityRepository { get; }

        GenericSqlRepository<UnfixedActivity> UnfixedActivityRepository { get; }

        GenericSqlRepository<UndefinedActivity> UndefinedActivityRepository { get; }

        GenericSqlRepository<DeadlineActivity> DeadlineActivityRepository { get; }

        GenericSqlRepository<Category> CategoryRepository { get; }

        GenericSqlRepository<Label> LabelRepository { get; }

        GenericSqlRepository<Milestone> MilestoneRepository { get; }

        UserSqlRepository UserRepository { get; }

        void Save();

        void Dispose();
    }
}
