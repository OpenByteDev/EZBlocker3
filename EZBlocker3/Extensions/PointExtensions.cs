using System.Windows;

namespace EZBlocker3.Extensions {
    internal static class PointExtensions {

        public static void Deconstruct(this Point point, out double x, out double y) =>
            (x, y) = (point.X, point.Y);

    }
}
