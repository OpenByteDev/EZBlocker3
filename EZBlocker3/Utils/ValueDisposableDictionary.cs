using System;
using System.Collections.Generic;

namespace EZBlocker3.Utils {
    internal sealed class ValueDisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TValue : IDisposable? {
        public ValueDisposableDictionary() { }
        public ValueDisposableDictionary(int capacity) : base(capacity) { }
        public ValueDisposableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public ValueDisposableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public ValueDisposableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
        public ValueDisposableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        // protected ValueDisposableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public void Dispose() {
            foreach (var value in Values) {
                value?.Dispose();
            }
        }
    }
}
