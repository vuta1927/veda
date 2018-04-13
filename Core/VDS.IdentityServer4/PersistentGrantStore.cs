using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Mapping;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace VDS.IdentityServer4
{
    public class PersistentGrantStore : IPersistedGrantStore
    {
        private readonly IRepository<PersistedGrantEntity, string> _persistedGrantRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public PersistentGrantStore(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<PersistedGrantEntity, string> persistedGrantRepository)
        {
            _persistedGrantRepository = persistedGrantRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                var entity = await _persistedGrantRepository.FirstOrDefaultAsync(grant.Key);
                if (entity == null)
                {
                    await _persistedGrantRepository.InsertAsync(Mapper.Map<PersistedGrantEntity>(grant));
                }
                else
                {
                    Mapper.Map(grant, entity);
                }
            });
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            return await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                var entity = await _persistedGrantRepository.FirstOrDefaultAsync(key);
                if (entity == null)
                {
                    return null;
                }

                return Mapper.Map<PersistedGrant>(entity);
            });
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            return await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                var entities = await _persistedGrantRepository.GetAllListAsync(x => x.SubjectId == subjectId);
                return Mapper.Map<List<PersistedGrant>>(entities);
            });
        }

        public async Task RemoveAsync(string key)
        {
            await _unitOfWorkManager.PerformAsyncUow(() => _persistedGrantRepository.DeleteAsync(key));
        }

        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            await _unitOfWorkManager.PerformAsyncUow(() =>
                _persistedGrantRepository.DeleteAsync(x => x.SubjectId == subjectId && x.ClientId == clientId));
        }

        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            await _unitOfWorkManager.PerformAsyncUow(() =>
                _persistedGrantRepository.DeleteAsync(x =>
                    x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type));
        }
    }
}