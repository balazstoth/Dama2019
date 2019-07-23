using Dama.Data.Enums;
using Dama.Data.Sql.Models;

namespace Dama.Data.Sql.Interfaces
{
    public interface IRepositoryManager
    {
        void RemoveCategoryFromDataTables(DbSetAction dbSetAction, ActivityType activityType);
    }
}
