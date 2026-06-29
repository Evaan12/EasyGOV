using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly Lazy<IConnectionMultiplexer>? _redisLazy;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly TimeProvider _timeProvider;

        private const int LockStripes = 1024;
        private static readonly SemaphoreSlim[] _lockPool = Enumerable
            .Range(0, LockStripes)
            .Select(_ => new SemaphoreSlim(1, 1))
            .ToArray();

        private static readonly JsonSerializerOptions _options = new()
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { PrivateSetterModifier }
            }
        };

        private static void PrivateSetterModifier(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

            foreach (var property in typeInfo.Properties)
            {
                if (property.Set == null)
                {
                    var currentType = typeInfo.Type;
                    while (currentType != null)
                    {
                        var propInfo = currentType.GetProperty(property.Name,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                        if (propInfo != null && propInfo.CanWrite)
                        {
                            property.Set = propInfo.SetValue;
                            break;
                        }
                        currentType = currentType.BaseType;
                    }
                }
            }
        }

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger, TimeProvider timeProvider, Lazy<IConnectionMultiplexer>? redisLazy = null)
        {
            _cache = cache;
            _logger = logger;
            _timeProvider = timeProvider;
            _redisLazy = redisLazy;
        }

        private IConnectionMultiplexer? Redis => _redisLazy?.Value;

        private static SemaphoreSlim GetLockByKey(string key)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in key)
                {
                    hash = ((hash << 5) - hash) + c;
                }
                return _lockPool[Math.Abs((int)hash) % LockStripes];
            }
        }

        public async Task<(bool Found, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _cache.GetStringAsync(key, cancellationToken);
                if (data == null) return (false, default);
                return (true, JsonSerializer.Deserialize<T>(data, _options));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache Get failure for key {Key}. Bypassing.", key);
                return (false, default);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromHours(2) };
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(value, _options), options, cancellationToken);

                var redis = Redis;
                if (redis != null && redis.IsConnected && tags != null && tags.Any())
                {
                    var db = redis.GetDatabase();
                    
                    // Integrating Custom TimeProvider mapping
                    var score = _timeProvider.GetUtcNow().Add(expirationTime ?? TimeSpan.FromHours(2)).ToUnixTimeSeconds();
                    var currentTime = _timeProvider.GetUtcNow().ToUnixTimeSeconds();

                    var batch = db.CreateBatch();
                    var tasks = new List<Task>();

                    foreach (var tag in tags)
                    {
                        var tagKey = $"{CacheKeys.Prefix}tag_zset_{tag}";
                        tasks.Add(batch.SortedSetAddAsync(tagKey, key, score));
                        tasks.Add(batch.SortedSetRemoveRangeByScoreAsync(tagKey, 0, currentTime));
                    }

                    batch.Execute();
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache Set failure for key {Key}. Bypassing.", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
            await _cache.RemoveAsync(key, cancellationToken);

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var redis = Redis;
            if (redis == null || !redis.IsConnected) return;

            var db = redis.GetDatabase();
            var endpoints = redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = redis.GetServer(endpoint);
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                if (keys.Length > 0)
                {
                    await db.KeyDeleteAsync(keys);
                }
            }
        }

        public async Task RemoveMultipleAsync(IEnumerable<string> keys, IEnumerable<string> tags, CancellationToken cancellationToken = default)
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key, cancellationToken);
            }

            var redis = Redis;
            if (redis == null || !redis.IsConnected) return;

            var db = redis.GetDatabase();
            var batch = db.CreateBatch();
            var tasks = new List<Task>();

            foreach (var tag in tags)
            {
                var tagKey = $"{CacheKeys.Prefix}tag_zset_{tag}";
                var members = await db.SortedSetRangeByValueAsync(tagKey);
                
                if (members.Length > 0)
                {
                    var keysToRemove = members.Select(m => (RedisKey)m.ToString()).ToArray();
                    tasks.Add(batch.KeyDeleteAsync(keysToRemove));
                    tasks.Add(batch.KeyDeleteAsync(tagKey));
                }
            }
            
            batch.Execute();
            await Task.WhenAll(tasks);
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expirationTime = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
        {
            var (found, value) = await TryGetAsync<T>(key, cancellationToken);
            if (found) return value!;

            var semaphore = GetLockByKey(key);
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                (found, value) = await TryGetAsync<T>(key, cancellationToken);
                if (found) return value!;

                value = await factory(cancellationToken);
                await SetAsync(key, value, expirationTime, tags, cancellationToken);
                return value;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task ClearAllAsync(CancellationToken cancellationToken = default)
        {
            var redis = Redis;
            if (redis == null || !redis.IsConnected)
            {
                throw new InvalidOperationException("Cannot clear cache. Redis is not connected or configured.");
            }

            try
            {
                var endpoints = redis.GetEndPoints();
                var db = redis.GetDatabase();

                foreach (var endpoint in endpoints)
                {
                    var server = redis.GetServer(endpoint);
                    if (server.IsReplica) continue;

                    await foreach (var key in server.KeysAsync(pattern: CacheKeys.Prefix + "*").WithCancellation(cancellationToken))
                    {
                        await db.KeyDeleteAsync(key, CommandFlags.FireAndForget);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure during ClearAll operation.");
                throw;
            }
        }
    }
}