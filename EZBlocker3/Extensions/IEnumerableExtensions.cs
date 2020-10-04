using System;
using System.Collections.Generic;

namespace EZBlocker3.Extensions {
    internal static class IEnumerableExtensions {

        public static void DisposeAll<T>(this IEnumerable<T> enumerable) where T : IDisposable {
            foreach (var item in enumerable)
                item.Dispose();
        }

    }
}
