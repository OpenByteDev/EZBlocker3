using System;
using System.Collections.Generic;
using EZBlocker3.Extensions;

namespace EZBlocker3.Utils {
    internal sealed class KeyValueDisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TKey : IDisposable
        where TValue : IDisposable {
        public KeyValueDisposableDictionary() { }
        public KeyValueDisposableDictionary(int capacity) : base(capacity) { }
        public KeyValueDisposableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public KeyValueDisposableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public KeyValueDisposableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
        public KeyValueDisposableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        // protected FullDisposableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public void Dispose() {
            foreach (var (key, value) in this) {
                key?.Dispose();
                value?.Dispose();
            }
        }
    }
}
