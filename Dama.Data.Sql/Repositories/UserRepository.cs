using Dama.Data.Models;
using Dama.Data.Sql.Services;
using Dama.Data.Sql.SQL;
using System.Threading.Tasks;

namespace Dama.Data.Sql.Repositories
{
    public class UserSqlRepository : SqlRepository<User>
    {
        private readonly UserService _userService;
        
        public UserSqlRepository(SqlConfiguration config, UserService userService)
            :base(config)
        {
            _userService = userService;
        }

        public async Task<User> GetUserById(string userId)
        {
            return await _userService.GetUserById(userId);
        }

        public string GetUserName()
        {
            return _userService.GetUserName();
        }

        public string GetUserId()
        {
            return _userService.GetUserId();
        }
    }
}
