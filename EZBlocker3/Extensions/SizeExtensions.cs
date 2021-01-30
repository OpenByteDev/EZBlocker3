using System.Windows;

namespace EZBlocker3.Extensions {
    internal static class SizeExtensions {
        public static void Deconstruct(this Size size, out double width, out double height) =>
            (width, height) = (size.Width, size.Height);
    }
}
