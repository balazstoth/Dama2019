using Dama.Data.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dama.Web.AccountControllerTest
{
    class MockedUserManager : UserManager<User>
    {
        public MockedUserManager(IUserStore<User> userStore) : base(userStore)
        {
        }

        public override async Task<IList<string>> GetRolesAsync(string userId)
        {
            return await Task.Run(() => new List<string>());
        }

        public override Task<IdentityResult> UpdateAsync(User user)
        {
            return Task.Run(() => new IdentityResult());
        }

        public override Task<User> FindByIdAsync(string userId)
        {
            return Task.Run(() => new User() { Id = userId });
        }
    }
}
