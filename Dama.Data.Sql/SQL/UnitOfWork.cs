using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using Dama.Data.Sql.Repositories;
using System;

namespace Dama.Data.Sql.SQL
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private bool _disposed = false;
        private DamaContext _context = new DamaContext();
        private GenericSqlRepository<FixedActivity> _fixedActivityRepository;
        private GenericSqlRepository<UnfixedActivity> _unfixedActivityRepository;
        private GenericSqlRepository<UndefinedActivity> _undefinedActivityRepository;
        private GenericSqlRepository<DeadlineActivity> _deadlineActivityRepository;
        private GenericSqlRepository<Category> _categoryRepository;
        private GenericSqlRepository<Label> _labelRepository;
        private GenericSqlRepository<Milestone> _milestoneRepository;
        private UserSqlRepository _userRepository;

        #region Properties
        public GenericSqlRepository<FixedActivity> FixedActivityRepository
        {
            get
            {
                if (_fixedActivityRepository == null)
                    _fixedActivityRepository = new GenericSqlRepository<FixedActivity>(_context);

                return _fixedActivityRepository;
            }
        }

        public GenericSqlRepository<UnfixedActivity> UnfixedActivityRepository
        {
            get
            {
                if (_unfixedActivityRepository == null)
                    _unfixedActivityRepository = new GenericSqlRepository<UnfixedActivity>(_context);

                return _unfixedActivityRepository;
            }
        }

        public GenericSqlRepository<UndefinedActivity> UndefinedActivityRepository
        {
            get
            {
                if (_undefinedActivityRepository == null)
                    _undefinedActivityRepository = new GenericSqlRepository<UndefinedActivity>(_context);

                return _undefinedActivityRepository;
            }
        }

        public GenericSqlRepository<DeadlineActivity> DeadlineActivityRepository
        {
            get
            {
                if (_deadlineActivityRepository == null)
                    _deadlineActivityRepository = new GenericSqlRepository<DeadlineActivity>(_context);

                return _deadlineActivityRepository;
            }
        }

        public GenericSqlRepository<Category> CategoryRepository
        {
            get
            {
                if (_categoryRepository == null)
                    _categoryRepository = new GenericSqlRepository<Category>(_context);

                return _categoryRepository;
            }
        }

        public GenericSqlRepository<Label> LabelRepository
        {
            get
            {
                if (_labelRepository == null)
                    _labelRepository = new GenericSqlRepository<Label>(_context);

                return _labelRepository;
            }
        }

        public GenericSqlRepository<Milestone> MilestoneRepository
        {
            get
            {
                if (_milestoneRepository == null)
                    _milestoneRepository = new GenericSqlRepository<Milestone>(_context);
                
                return _milestoneRepository;
            }
        }

        public UserSqlRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                    _userRepository = new UserSqlRepository(_context);

                return _userRepository;
            }
        }
        #endregion

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
