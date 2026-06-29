using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Caching
{
    public interface ICacheService
    {
        Task<(bool Found, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
        Task RemoveMultipleAsync(IEnumerable<string> keys, IEnumerable<string> tags, CancellationToken cancellationToken = default);
        Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expirationTime = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);
        Task ClearAllAsync(CancellationToken cancellationToken = default);
    }
}