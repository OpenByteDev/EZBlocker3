using System;

namespace EZBlocker3.Spotify {
    public abstract class AbstractSpotifyAdBlocker : IActivatable {
        public bool IsActive { get; private set; }
        public ISpotifyHook Hook { get; }

        protected AbstractSpotifyAdBlocker(ISpotifyHook hook) {
            Hook = hook;
        }

        public void Activate() {
            if (IsActive)
                throw new InvalidOperationException("Blocker is already active.");

            IsActive = true;

            Hook.HookChanged += OnHookChanged;
            Hook.SpotifyStateChanged += OnSpotifyStateChanged;
            Hook.ActiveSongChanged += OnActiveSongChanged;
        }

        public void Deactivate() {
            if (!IsActive)
                throw new InvalidOperationException("Hook has to be active.");

            IsActive = false;

            Hook.HookChanged -= OnHookChanged;
            Hook.SpotifyStateChanged -= OnSpotifyStateChanged;
            Hook.ActiveSongChanged -= OnActiveSongChanged;
        }

        protected virtual void OnHookChanged(object sender, EventArgs e) { }
        protected virtual void OnSpotifyStateChanged(object sender, SpotifyStateChangedEventArgs e) { }
        protected virtual void OnActiveSongChanged(object sender, ActiveSongChangedEventArgs e) { }
    }
}
