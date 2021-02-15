namespace EZBlocker3.Spotify {
    public interface IActivatable {
        bool IsActive { get; }
        void Activate();
        void Deactivate();
    }
}
