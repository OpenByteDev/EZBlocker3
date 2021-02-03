using System;
using System.Collections.Generic;

namespace EZBlocker3.Utils {
    internal sealed class KeyDisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TKey : IDisposable {
        public KeyDisposableDictionary() { }
        public KeyDisposableDictionary(int capacity) : base(capacity) { }
        public KeyDisposableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public KeyDisposableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public KeyDisposableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
        public KeyDisposableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        // protected KeyDisposableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public void Dispose() {
            foreach (var key in Keys) {
                key?.Dispose();
            }
        }
    }
}
