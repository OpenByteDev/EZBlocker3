using System.Collections.Generic;

namespace EZBlocker3 {
    public readonly struct SongInfo {

        public readonly string Title;
        public readonly string Artist;

        public SongInfo(string title, string artist) {
            Title = title;
            Artist = artist;
        }

        public override bool Equals(object? obj) =>
            obj is SongInfo songInfo && Equals(songInfo);

        public bool Equals(SongInfo info) => 
            Title == info.Title && Artist == info.Artist;

        public override int GetHashCode() {
            // return HashCode.Combine(Title, Artist);
            var hashCode = -1370019569;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Artist);
            return hashCode;
        }

        public static bool operator ==(SongInfo left, SongInfo right)
            => left.Equals(right);
        public static bool operator !=(SongInfo left, SongInfo right)
            => !(left == right);

    }
}
