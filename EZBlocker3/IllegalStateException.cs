using System;

namespace EZBlocker3 {
    [Serializable]
    internal sealed class IllegalStateException : Exception {
        public IllegalStateException() : this("The current state of the object is illegal.") { }
        public IllegalStateException(string message) : base(message) { }
        public IllegalStateException(string message, Exception innerException) : base(message, innerException) { }
        // protected IllegalStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}