using Lazy;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace EZBlocker3.Extensions {
    internal static class ProcessExtensions {

        [Lazy]
        private static Func<Process, bool> _getProcessAssociatedFunc {
            get {
                // Expression Trees let us change a private field and are faster than reflection (if called multiple times)
                var processParamter = Expression.Parameter(typeof(Process), "process");
                var associatedProperty = Expression.Property(processParamter, "Associated");
                // var returnStatement = Expression.Return()
                // var returnLabel = Expression.Label();
                var lambda = Expression.Lambda<Func<Process, bool>>(associatedProperty, processParamter);
                return lambda.Compile();
            }
        }
        public static bool IsAssociated(this Process process) {
            return _getProcessAssociatedFunc(process);
        }

    }
}
