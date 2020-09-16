using System;

namespace EZBlocker3 {
    public readonly struct SongInfo {

        public readonly string Title;
        public readonly string Artist;

        public SongInfo(string title, string artist) {
            Title = title;
            Artist = artist;
        }

        public override bool Equals(object? obj) {
            return obj is SongInfo songInfo && Equals(songInfo);
        }

        public bool Equals(SongInfo info) {
            return Title == info.Title && Artist == info.Artist;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Title, Artist);
        }

        public static bool operator ==(SongInfo left, SongInfo right) {
            return left.Equals(right);
        }

        public static bool operator !=(SongInfo left, SongInfo right) {
            return !(left == right);
        }
    }
}
