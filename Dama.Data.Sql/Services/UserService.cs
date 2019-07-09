using Dama.Data.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Dama.Data.Sql.Services
{
    public class UserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public UserService(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext.User?.Identity?.Name;
        }

        public string GetUserId()
        {
            return _httpContextAccessor.HttpContext.User?.Identity?.GetUserId();
        }

        public async Task<User> GetUserById(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
    }
}
