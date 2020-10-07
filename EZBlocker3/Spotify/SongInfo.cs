namespace EZBlocker3.Spotify {
    public record SongInfo(string Title, string Artist) {

        public override string ToString() => $"{Title} by {Artist}";

    }
}
