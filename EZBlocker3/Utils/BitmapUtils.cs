using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace EZBlocker3.Utils {
    internal static class BitmapUtils {

        // https://stackoverflow.com/a/11448060/6304917
        public static void SaveAsIcon(Bitmap sourceBitmap, string filePath) {
            using var file = new FileStream(filePath, FileMode.Create);
            SaveAsIcon(sourceBitmap, file);
        }

        public static void SaveAsIcon(Bitmap sourceBitmap, Stream stream) {
            // ICO header
            stream.WriteByte(0); stream.WriteByte(0);
            stream.WriteByte(1); stream.WriteByte(0);
            stream.WriteByte(1); stream.WriteByte(0);

            // Image size
            stream.WriteByte((byte)sourceBitmap.Width);
            stream.WriteByte((byte)sourceBitmap.Height);
            // Palette
            stream.WriteByte(0);
            // Reserved
            stream.WriteByte(0);
            // Number of color planes
            stream.WriteByte(0); stream.WriteByte(0);
            // Bits per pixel
            stream.WriteByte(32); stream.WriteByte(0);

            // Data size, will be written after the data
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);

            // Offset to image data, fixed at 22
            stream.WriteByte(22);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);

            // Writing actual data
            sourceBitmap.Save(stream, ImageFormat.Png);

            // Getting data length (file length minus header)
            var len = stream.Length - 22;

            // Write it in the correct place
            stream.Seek(14, SeekOrigin.Begin);
            stream.WriteByte((byte)len);
            stream.WriteByte((byte)(len >> 8));
        }
    }
}
