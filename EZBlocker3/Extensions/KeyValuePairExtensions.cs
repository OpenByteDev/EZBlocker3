using System.Collections.Generic;

namespace EZBlocker3.Extensions {
    internal static class KeyValuePairExtensions {

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value) {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }
    }

}
