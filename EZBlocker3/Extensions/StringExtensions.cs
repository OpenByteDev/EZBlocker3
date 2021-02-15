using System;

namespace EZBlocker3.Extensions {
    internal static class StringExtensions {
        public static string[] Split(this string str, string delimiter) =>
            str.Split(delimiter, StringSplitOptions.None);
        public static string[] Split(this string str, string delimiter, int maxCount) =>
            str.Split(delimiter, maxCount, StringSplitOptions.None);
        public static string[] Split(this string str, string delimiter, StringSplitOptions options) =>
            str.Split(delimiter, int.MaxValue, options);
        public static string[] Split(this string str, string delimiter, int maxCount, StringSplitOptions options) =>
            str.Split(new string[] { delimiter }, maxCount, options);

        public static bool Contains(this string str, string substring, StringComparison comparisonType) => str.IndexOf(substring, comparisonType) >= 0;
    }
}
