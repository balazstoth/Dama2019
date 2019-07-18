using Dama.Data.Models;
using System.Data.Entity;

namespace Dama.Data.Interfaces
{
    public interface IRepositorySettings
    {
        void ChangeCategoryEntryState(Category category, EntityState entityState);
    }
}
