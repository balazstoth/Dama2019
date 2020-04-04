using Dama.Data.Sql.Interfaces;
using System;

namespace Dama.Data.UnitTest
{
    class MockContext : IContext
    {
        public void Dispose()
        {
        }

        public int SaveChanges()
        {
            return 0;
        }
    }
}
