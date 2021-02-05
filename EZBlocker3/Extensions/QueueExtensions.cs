using System.Collections.Generic;

namespace EZBlocker3.Extensions {
    internal static class QueueExtensions {
        public static bool TryDequeue<T>(this Queue<T> queue, /*[NotNullWhen(true)]*/ out T? result) {
            if (queue.Count == 0) {
                result = default!;
                return false;
            } else {
                result = queue.Dequeue();
                return true;
            }
        }
    }
}
