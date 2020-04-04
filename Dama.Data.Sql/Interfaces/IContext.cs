using System;

namespace Dama.Data.Sql.Interfaces
{
    public interface IContext : IDisposable
    {
        int SaveChanges();
    }
}
