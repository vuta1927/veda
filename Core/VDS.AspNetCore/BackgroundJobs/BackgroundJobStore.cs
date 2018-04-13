using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.BackgroundJobs;
using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Dependency;
using VDS.Timing;

namespace VDS.AspNetCore.BackgroundJobs
{
    /// <summary>
    /// Implements <see cref="IBackgroundJobStore"/> using repositories.
    /// </summary>
    public class BackgroundJobStore : IBackgroundJobStore, ITransientDependency
    {
        private readonly IRepository<BackgroundJobInfo, long> _backgroundJobRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public BackgroundJobStore(IRepository<BackgroundJobInfo, long> backgroundJobRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            _backgroundJobRepository = backgroundJobRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public Task<BackgroundJobInfo> GetAsync(long jobId)
        {
            return _backgroundJobRepository.GetAsync(jobId);
        }

        public Task InsertAsync(BackgroundJobInfo jobInfo)
        {
            return _unitOfWorkManager.PerformAsyncUow<Task>(() => _backgroundJobRepository.InsertAsync(jobInfo));
        }
        
        public virtual Task<List<BackgroundJobInfo>> GetWaitingJobsAsync(int maxResultCount)
        {
            return _unitOfWorkManager.PerformAsyncUow<Task<List<BackgroundJobInfo>>>(() =>
            {
                var waitingJobs = _backgroundJobRepository.GetAll()
                    .Where(t => !t.IsAbandoned && t.NextTryTime <= Clock.Now)
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.TryCount)
                    .ThenBy(t => t.NextTryTime)
                    .Take(maxResultCount)
                    .ToList();

                return Task.FromResult(waitingJobs);
            });
        }

        public Task DeleteAsync(BackgroundJobInfo jobInfo)
        {
            return _unitOfWorkManager.PerformAsyncUow(() => _backgroundJobRepository.DeleteAsync(jobInfo));
        }

        public Task UpdateAsync(BackgroundJobInfo jobInfo)
        {
            return _unitOfWorkManager.PerformAsyncUow<Task>(() => _backgroundJobRepository.UpdateAsync(jobInfo));
        }
    }
}