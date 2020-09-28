using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EZBlocker3.Extensions;

namespace EZBlocker3.Utils {
    internal class FullDisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
        where TKey : IDisposable
        where TValue : IDisposable {

        public FullDisposableDictionary() { }
        public FullDisposableDictionary(int capacity) : base(capacity) { }
        public FullDisposableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public FullDisposableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public FullDisposableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
        public FullDisposableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        protected FullDisposableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public void Dispose() {
            foreach (var (key, value) in this) {
                key?.Dispose();
                value?.Dispose();
            }
        }
    }
}
