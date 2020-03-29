using Dama.Data.Models;
using Dama.Data.Sql.Repositories;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<FixedActivity> FixedActivityRepository { get; set; }
        IRepository<UnfixedActivity> UnfixedActivityRepository { get; set; }
        IRepository<UndefinedActivity> UndefinedActivityRepository { get; set; }
        IRepository<DeadlineActivity> DeadlineActivityRepository { get; set; }
        IRepository<Category> CategoryRepository { get; set; }
        IRepository<Label> LabelRepository { get; set; }
        IRepository<Milestone> MilestoneRepository { get; set; }
        IRepository<User> UserRepository { get; set; }

        void Save();

        void Dispose();
    }
}
