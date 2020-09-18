using System.Collections.Generic;

namespace EZBlocker3.Extensions {
    internal static class IEnumerableExtensions {

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) {
            return new HashSet<T>(enumerable);
        }

    }
}
