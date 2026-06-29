using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ApiConsentRequestRepository : IApiConsentRequestRepository
    {
        private readonly AppDbContext _context;

        public ApiConsentRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiConsentRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ApiConsentRequest>().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<ApiConsentRequest?> GetActivePendingRequestAsync(Guid citizenId, string thirdPartyClientId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ApiConsentRequest>()
                .Where(a => a.CitizenId == citizenId && a.ThirdPartyClientId == thirdPartyClientId && a.Status == ConsentStatus.PendingOTP)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(ApiConsentRequest entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<ApiConsentRequest>().AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(ApiConsentRequest entity, CancellationToken cancellationToken = default)
        {
            _context.Set<ApiConsentRequest>().Update(entity);
            return Task.CompletedTask;
        }
    }
}