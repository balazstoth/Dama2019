using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using Dama.Data.Sql.Repositories;
using System;

namespace Dama.Data.Sql.SQL
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private bool _disposed = false;
        private DamaContext _context;
      
        public IRepository<FixedActivity> FixedActivityRepository { get; set; }
        public IRepository<UnfixedActivity> UnfixedActivityRepository { get; set; }
        public IRepository<UndefinedActivity> UndefinedActivityRepository { get; set; }
        public IRepository<DeadlineActivity> DeadlineActivityRepository { get; set; }
        public IRepository<Category> CategoryRepository { get; set; }
        public IRepository<Label> LabelRepository { get; set; }
        public IRepository<Milestone> MilestoneRepository { get; set; }
        public IRepository<User> UserRepository { get; set; }

        public UnitOfWork()
        {
            _context = new DamaContext();

            FixedActivityRepository = new GenericSqlRepository<FixedActivity>(_context);
            UnfixedActivityRepository = new GenericSqlRepository<UnfixedActivity>(_context);
            UndefinedActivityRepository = new GenericSqlRepository<UndefinedActivity>(_context);
            DeadlineActivityRepository = new GenericSqlRepository<DeadlineActivity>(_context);
            CategoryRepository = new GenericSqlRepository<Category>(_context);
            LabelRepository = new GenericSqlRepository<Label>(_context);
            MilestoneRepository = new GenericSqlRepository<Milestone>(_context);
            UserRepository = new UserSqlRepository(_context);
        }

        public UnitOfWork(IRepository<FixedActivity> fixedActivityRepository = null, 
                            IRepository<UnfixedActivity> unfixedActivityRepository = null, 
                            IRepository<UndefinedActivity> undefinedActivityRepository = null, 
                            IRepository<DeadlineActivity> deadlineActivityRepository = null, 
                            IRepository<Category> categoryRepository = null, 
                            IRepository<Label> labelRepository = null, 
                            IRepository<Milestone> milestoneRepository = null, 
                            IRepository<User> userRepository = null)
        {
            FixedActivityRepository = fixedActivityRepository;
            UnfixedActivityRepository = unfixedActivityRepository;
            UndefinedActivityRepository = undefinedActivityRepository;
            DeadlineActivityRepository = deadlineActivityRepository;
            CategoryRepository = categoryRepository;
            LabelRepository = labelRepository;
            MilestoneRepository = milestoneRepository;
            UserRepository = userRepository;
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _context.Dispose();
            }

            _disposed = true;
        }
    }
}
