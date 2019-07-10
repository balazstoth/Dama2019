using Dama.Data.Models;

namespace Dama.Data.Interfaces
{
    public interface IUserRepository
    {
        IRepository<User> UserSqlRepository { get; set; }
    }
}
