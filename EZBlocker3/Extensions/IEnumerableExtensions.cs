using System;
using System.Collections.Generic;
using System.Linq;

namespace EZBlocker3.Extensions {
    internal static class IEnumerableExtensions {

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) {
            return new HashSet<T>(enumerable);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
            return enumerable.Count() == 0;
        }

        public static void DisposeAll<T>(this IEnumerable<T> enumerable) where T : IDisposable {
            foreach (var item in enumerable)
                item.Dispose();
        }

    }
}
