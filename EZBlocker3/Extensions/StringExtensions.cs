using System;

namespace EZBlocker3.Extensions {
    internal static class StringExtensions {

        public static string[] Split(this string str, string delimiter) {
            return str.Split(delimiter, StringSplitOptions.None);
        }

        public static string[] Split(this string str, string delimiter, int maxCount) {
            return str.Split(delimiter, maxCount, StringSplitOptions.None);
        }

        public static string[] Split(this string str, string delimiter, StringSplitOptions options) {
            return str.Split(delimiter, int.MaxValue, options);
        }

        public static string[] Split(this string str, string delimiter, int maxCount, StringSplitOptions options) {
            return str.Split(new string[] { delimiter }, maxCount, options);
        }
    }
}
