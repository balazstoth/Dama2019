using Dama.Data.Enums;
using Dama.Data.Sql.Interfaces;
using Dama.Data.Sql.Models;
using Dama.Data.Sql.SQL;
using System;

namespace Dama.Data.Sql.Repositories
{
    public class RepositoryManager : IRepositoryManager
    {
        public void RemoveCategoryFromDataTables(DbSetAction dbSetAction, ActivityType activityType)
        {
            if(dbSetAction == null)
                throw new ArgumentNullException("dbSetAction");

            using (var context = new DamaContext())
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
