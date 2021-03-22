using AudioBand.AudioSource;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WindowsAudioSource
{
    public class WindowsAudioSource : IAudioSource
    {
        // TODO: Add setting to attempt choosing the "best" session, and default to the current session always
        // TODO: Add setting to auto pause previous session when sessions switch
        // TODO: Add setting for disallowed apps
        // TODO: Add setting for locking to a specified app
        // TODO: Add setting for *temporarily* locking to the current session (useful for browsers)
        // TODO: Write seen AppUserModelIds to a rotating temp file, and add readonly setting showing its path
        // TODO: Add readonly setting showing the current session AppUserModelId
        // TODO: Switch this to a couple of flags for music, video, and unknown sources
        [AudioSourceSetting("Current Session Source",
            Description = "The AppUserModelId of the session currently being controlled.",
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionSource
        {
            get => _windowsAudioSessionManager.CurrentSessionSource;
            set => _windowsAudioSessionManager.CurrentSessionSource = value;
        }

        [AudioSourceSetting("Session Source Disallow List",
            Description = "Comma separated list of AppUserModelIds to block from being controlled.")]
        public string SessionSourceDisallowList
        {
            get => _windowsAudioSessionManager.SessionSourceDisallowList;
            set => _windowsAudioSessionManager.SessionSourceDisallowList = value;
        }

        // NOTE: ReadOnly only seems to work for strings?
        [AudioSourceSetting("Smart Session Switching",
            Description = "[EXPERIMENTAL] When enabled, the controlled session will be selected based on media type and play state.\n" +
            "May not match the current session Windows is controlling.")]
        public bool SmartSessionSwitchingEnabled { get; set; }

        public string Name => "Windows";

        public IAudioSourceLogger Logger { get; set; }

        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;
        public event EventHandler<bool> IsPlayingChanged;
        public event EventHandler<TimeSpan> TrackProgressChanged;
        public event EventHandler<float> VolumeChanged;
        public event EventHandler<bool> ShuffleChanged;
        public event EventHandler<RepeatMode> RepeatModeChanged;

        private WindowsAudioSessionManager _windowsAudioSessionManager;

        public async Task ActivateAsync()
        {
            _windowsAudioSessionManager = await WindowsAudioSessionManager.CreateInstance(Logger);
            _windowsAudioSessionManager.SettingChanged += SettingChanged;
            _windowsAudioSessionManager.TrackInfoChanged += TrackInfoChanged;
            _windowsAudioSessionManager.IsPlayingChanged += IsPlayingChanged;
            _windowsAudioSessionManager.TrackProgressChanged += TrackProgressChanged;
            _windowsAudioSessionManager.VolumeChanged += VolumeChanged;
            _windowsAudioSessionManager.ShuffleChanged += ShuffleChanged;
            _windowsAudioSessionManager.RepeatModeChanged += RepeatModeChanged;
        }

        public Task DeactivateAsync()
        {
            _windowsAudioSessionManager.SettingChanged -= SettingChanged;
            _windowsAudioSessionManager.TrackInfoChanged -= TrackInfoChanged;
            _windowsAudioSessionManager.IsPlayingChanged -= IsPlayingChanged;
            _windowsAudioSessionManager.TrackProgressChanged -= TrackProgressChanged;
            _windowsAudioSessionManager.VolumeChanged -= VolumeChanged;
            _windowsAudioSessionManager.ShuffleChanged -= ShuffleChanged;
            _windowsAudioSessionManager.RepeatModeChanged -= RepeatModeChanged;
            _windowsAudioSessionManager = null;

            return Task.CompletedTask;
        }

        // TODO: Move impl for these into separte controls class
        // TODO: Only attempt each control if it's supported by the current session
        public Task NextTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TrySkipNextAsync());

        public Task PauseTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryPauseAsync());

        public Task PlayTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryPlayAsync());

        public Task PreviousTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TrySkipPreviousAsync());

        // This needs to be in ticks, per discussion here:
        // https://github.com/MicrosoftDocs/winrt-api/issues/1725
        public Task SetPlaybackProgressAsync(TimeSpan newProgress) =>
            LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryChangePlaybackPositionAsync(newProgress.Ticks));

        public Task SetRepeatModeAsync(RepeatMode newRepeatMode) =>
            LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryChangeAutoRepeatModeAsync(newRepeatMode.ToMediaPlaybackAutoRepeatMode()));

        public Task SetShuffleAsync(bool shuffleOn) => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryChangeShuffleActiveAsync(shuffleOn));

        public Task SetVolumeAsync(float newVolume)
        {
            Logger.Error("Volume Not Supported!");
            return Task.CompletedTask;
        }

        private async Task LogPlayerCommandIfFailed(Func<IAsyncOperation<bool>> command, [CallerMemberName] string caller = null)
        {
            try
            {
                var success = await command();
                if (!success)
                {
                    Logger.Warn($"Player command failed: '{caller}'");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}
