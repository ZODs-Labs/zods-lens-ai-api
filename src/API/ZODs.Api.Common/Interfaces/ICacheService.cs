using Microsoft.Extensions.Caching.Distributed;

namespace ZODs.Api.Common.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Checks if cache exists.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if cache exists, otherwise returns false.</returns>
        Task<bool> Exists(
            string key,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets cached object by key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Generic object type parameter.</typeparam>
        /// <returns>T.</returns>
        Task<T> Get<T>(
            string key,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Sets object value to cache.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Generic object type parameter.</typeparam>
        /// <returns>T.</returns>
        Task Set<T>(
            string key,
            T value,
            DistributedCacheEntryOptions options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes object from cache by key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>T.</returns>
        Task Remove(
            string key,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets integer value from cache by key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int?> GetInteger(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an item to the end of a Redis list asynchronously.
        /// </summary>
        /// <param name="listKey">The key of the Redis list.</param>
        /// <param name="item">The item to add to the list. The item will be serialized to a string.</param>
        /// <typeparam name="T">The type of the item to add.</typeparam>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddItemToListAsync<T>(string listKey, T item);

        /// <summary>
        /// Retrieves a range of items from a Redis list asynchronously.
        /// </summary>
        /// <param name="listKey">The key of the Redis list.</param>
        /// <param name="start">The starting index of the range to retrieve. Default is 0.</param>
        /// <param name="stop">The ending index of the range to retrieve. Default is -1, which represents the end of the list.</param>
        /// <typeparam name="T">The type of the items in the list.</typeparam>
        /// <returns>A task that represents the asynchronous operation, containing a list of deserialized items from the specified range.</returns>
        Task<IEnumerable<T>> GetItemsFromListAsync<T>(string listKey, long start = 0, long stop = -1);

        /// <summary>
        /// Retrieves a range of string items from a Redis list asynchronously.
        /// </summary>
        /// <param name="listKey">The key of the Redis list.</param>
        /// <param name="start">The starting index of the range to retrieve. Default is 0.</param>
        /// <param name="stop">The ending index of the range to retrieve. Default is -1, which represents the end of the list.</param>
        /// <returns>A task that represents the asynchronous operation, containing a list of string items from the specified range.</returns>
        Task<IEnumerable<string>> GetStringItemsFromListAsync(string listKey, long start = 0, long stop = -1);

        /// <summary>
        /// Removes a specified number of occurrences of an item from a Redis list asynchronously.
        /// </summary>
        /// <param name="listKey">The key of the Redis list.</param>
        /// <param name="item">The item to remove. The item will be serialized to a string for comparison.</param>
        /// <param name="count">The number of occurrences of the item to remove. Default is 1. If count is greater than the number of occurrences, all occurrences will be removed. If count is negative, the search is performed from the end of the list.</param>
        /// <typeparam name="T">The type of the item to remove.</typeparam>
        /// <returns>A task that represents the asynchronous operation, containing the number of items removed.</returns>
        Task<long> RemoveItemsFromListAsync<T>(string listKey, T item, long count = 1);


        Task SetListAsync<T>(string listKey, IEnumerable<T> items, TimeSpan? timeToLive = null);
    }
}