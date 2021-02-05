using System;
using System.Collections.Generic;

namespace EZBlocker3.Utils {
    internal sealed class DisposableList<T> : List<T>, IDisposable where T : IDisposable? {
        public DisposableList() { }
        public DisposableList(int capacity) : base(capacity) { }
        public DisposableList(IEnumerable<T> collection) : base(collection) { }

        public void Dispose() {
            foreach (var obj in this) {
                obj?.Dispose();
            }
        }
    }
}