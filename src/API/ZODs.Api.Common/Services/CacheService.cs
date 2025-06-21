using ZODs.Api.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ZODs.Api.Common.Services
{
    public sealed class CacheService : ICacheService
    {
        private readonly IDistributedCache cache;
        private readonly ILogger<CacheService> logger;
        private readonly IDatabase redisDb;

        public CacheService(
            IDistributedCache _cache,
            ILogger<CacheService> _logger,
            IConnectionMultiplexer connectionMultiplexer)
        {
            cache = _cache;
            logger = _logger;
            redisDb = connectionMultiplexer.GetDatabase();
        }

        private readonly DistributedCacheEntryOptions cacheOptions =
            new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(30));

        private readonly JsonSerializerOptions SERIALIZER_OPTIONS = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public async Task<bool> Exists(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheContent = await this.cache.GetStringAsync(key, cancellationToken);
                return !string.IsNullOrEmpty(cacheContent);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error checking existence of cache key {Key}", key);
                return false;
            }
        }

        public async Task<T> Get<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var value = await this.cache.GetStringAsync(key, cancellationToken);
                if (!string.IsNullOrEmpty(value))
                {
                    return DeserializeObject<T>(value);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting cache key {Key}", key);
            }

            return null;
        }

        public async Task<int?> GetInteger(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await this.cache.GetStringAsync(key, cancellationToken);
                if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var parsedInt))
                {
                    return parsedInt;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting integer cache for key {Key}", key);
            }

            return null;
        }

        public async Task Set<T>(string key, T value, DistributedCacheEntryOptions options = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var serializedValue = SerializeObject<T>(value);
                await this.cache.SetStringAsync(key, serializedValue, options ?? this.cacheOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting cache key {Key}", key);
            }
        }

        public async Task Remove(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing cache key {Key}", key);
            }
        }

        public async Task AddItemToListAsync<T>(string listKey, T item)
        {
            try
            {
                var serializedItem = SerializeObject(item);
                await redisDb.ListRightPushAsync(listKey, serializedItem);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to add item to list {ListKey}", listKey);
            }
        }

        public async Task SetListAsync<T>(
            string listKey,
            IEnumerable<T> items,
            TimeSpan? timeToLive = null)
        {
            try
            {
                timeToLive ??= TimeSpan.FromDays(30);

                var serializedItems = items.Select(x => SerializeObject(x));

                await redisDb.KeyDeleteAsync(listKey);

                foreach (var item in serializedItems)
                {
                    await redisDb.ListRightPushAsync(listKey, item);
                }

                await redisDb.KeyExpireAsync(listKey, timeToLive);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set list {ListKey}", listKey);
            }
        }

        public async Task<IEnumerable<T>> GetItemsFromListAsync<T>(
            string listKey,
            long start = 0,
            long stop = -1)
        {
            try
            {
                var items = await redisDb.ListRangeAsync(listKey, start, stop);
                var result = new List<T>();

                foreach (var item in items)
                {
                    T deserializedItem = DeserializeObject<T>(item);
                    if (deserializedItem != null)
                    {
                        result.Add(deserializedItem);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve items from list {ListKey}", listKey);
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetStringItemsFromListAsync(string listKey, long start = 0, long stop = -1)
        {
            try
            {
                var items = await redisDb.ListRangeAsync(listKey, start, stop);
                return items.Select(i => i.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve string items from list {ListKey}", listKey);
                return null;
            }
        }

        public async Task<long> RemoveItemsFromListAsync<T>(string listKey, T item, long count = 1)
        {
            try
            {
                var serializedItem = SerializeObject<T>(item);
                return await redisDb.ListRemoveAsync(listKey, serializedItem, count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to remove item from list {ListKey}", listKey);
                return 0;
            }
        }

        private string SerializeObject<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, SERIALIZER_OPTIONS);
        }

        private T DeserializeObject<T>(string serializedObj)
        {
            return JsonSerializer.Deserialize<T>(serializedObj, SERIALIZER_OPTIONS);
        }
    }
}