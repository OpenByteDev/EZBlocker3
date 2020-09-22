namespace EZBlocker3 {
    public record SongInfo(string Title, string Artist) {

        public override string ToString() => $"{Title} by {Artist}";

    }
}
