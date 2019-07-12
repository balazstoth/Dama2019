using Dama.Data.Enums;
using Dama.Data.Sql.Models;
using Dama.Data.Sql.SQL;

namespace Dama.Data.Sql.Repositories
{
    public class RepositoryManager
    {
        private readonly SqlConfiguration _config;

        public RepositoryManager(SqlConfiguration config)
        {
            _config = config;
        }

        public void RemoveCategoryFromDataTables(DbSetAction dbSetAction, ActivityType activityType)
        {
            using (var context = new DamaContext(_config))
            {
                switch (activityType)
                {
                    case ActivityType.FixedActivity:
                        dbSetAction.FixedActivityAction(context.FixedActivities);
                        break;

                    case ActivityType.UnfixedActivity:
                        dbSetAction.UnfixedActivityAction(context.UnFixedActivities);
                        break;

                    case ActivityType.UndefinedActivity:
                        dbSetAction.UndefinedActivityAction(context.UndefinedActivities);
                        break;

                    case ActivityType.DeadlineActivity:
                        dbSetAction.DeadlineActivityAction(context.DeadLineActivities);
                        break;
                }
            }
        }
    }
}
