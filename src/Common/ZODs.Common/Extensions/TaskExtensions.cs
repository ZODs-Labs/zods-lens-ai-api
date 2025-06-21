using System.Runtime.CompilerServices;

namespace ZODs.Common.Extensions
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable NoSync(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<T> NoSync<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}