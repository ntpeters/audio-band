using AudioBand.AudioSource;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WindowsAudioSource
{
    public class WindowsAudioSource : IAudioSource
    {
        #region Brainstorming TODOs
        // TODO: Add setting to attempt choosing the "best" session, and default to the current session always
        //       -> Maybe. Needs more thought. Only very specific cases make sense due to the way multi-session apps work
        // TODO: Add setting to auto pause previous session when sessions switch
        //       -> No, multi-session apps like browsers are too unreliable to ensure the correct session is being paused if the app is reporting a new active session
        // TODO: Add setting for locking to a specified app
        //       -> No, tested and is too unreliable for multi-session apps like browsers, and no way to detect such apps
        // TODO: Add setting for *temporarily* locking to the current session (useful for browsers)
        //       -> Nope, see above
        // TODO: Write seen AppUserModelIds to a rotating temp file, and add readonly setting showing its path
        //       -> Maybe, unclear if this would actually be useful to users
        // TODO: Switch this to a couple of flags for music, video, and unknown sources
        //       -> Just one flag for music, default to current session always
        // TODO: Move impl of media controls into separte controls class
        //       -> Seems unnecessary, their impl is minimal and refactoring wouldn't seem to benefit much other than division of responsibilities at the cost of increase complexity
        #endregion Brainstorming TODOs

        [AudioSourceSetting(SettingConstants.CurrentSessionSourceName,
            Description = SettingConstants.CurrentSessionSourceDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionSource
        {
            // TODO: Fix this
            get => _windowsAudioSessionManager?.CurrentSessionSource ?? string.Empty;
            set
            {
                if (_windowsAudioSessionManager == null)
                {
                    return;
                }
                _windowsAudioSessionManager.CurrentSessionSource = value;
            }
        }

        [AudioSourceSetting(SettingConstants.CurrentSessionTypeName,
            Description = SettingConstants.CurrentSessionTypeDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionType
        {
            // TODO: Fix this
            get => _windowsAudioSessionManager?.CurrentSessionType ?? string.Empty;
            set
            {
                if (_windowsAudioSessionManager == null)
                {
                    return;
                }
                _windowsAudioSessionManager.CurrentSessionType = value;
            }
        }

        [AudioSourceSetting(SettingConstants.SessionSourceDisallowListName,
            Description = SettingConstants.SessionSourceDisallowListDescription)]
        public string SessionSourceDisallowList
        {
            // TODO: Fix this
            get => _windowsAudioSessionManager?.SessionSourceDisallowList ?? string.Empty;
            set
            {
                if (_windowsAudioSessionManager == null)
                {
                    return;
                }
                _windowsAudioSessionManager.SessionSourceDisallowList = value;
            }
        }

        [AudioSourceSetting(SettingConstants.MusicSessionsOnlyName,
            Description = SettingConstants.MusicSessionsOnlyDescription)]
        public bool MusicSessionsOnly
        {
            // TODO: Fix this
            get => _windowsAudioSessionManager?.MusicSessionsOnly ?? false;
            set
            {
                if (_windowsAudioSessionManager == null)
                {
                    return;
                }
                _windowsAudioSessionManager.MusicSessionsOnly = value;
            }
        }

        [AudioSourceSetting(SettingConstants.CurrentSessionCapabilitiesName,
            Description = SettingConstants.CurrentSessionCapabilitiesDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionCapabilities
        {
            // TODO: Fix this
            get => _windowsAudioSessionManager?.CurrentSessionCapabilities ?? string.Empty;
            set
            {
                if (_windowsAudioSessionManager == null)
                {
                    return;
                }
                _windowsAudioSessionManager.CurrentSessionCapabilities = value;
            }
        }

        public string Name => "Windows";

        public IAudioSourceLogger Logger { get; set; }

        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;
        public event EventHandler<bool> IsPlayingChanged;
        public event EventHandler<TimeSpan> TrackProgressChanged;
        public event EventHandler<float> VolumeChanged;
        public event EventHandler<bool> ShuffleChanged;
        public event EventHandler<RepeatMode> RepeatModeChanged;

        private readonly IWindowsAudioSessionManager _windowsAudioSessionManager;

        public WindowsAudioSource()
            : this(new WindowsAudioSessionManager(new GlobalSystemMediaTransportControlsSessionManagerWrapperFactory()))
        {
        }

        public WindowsAudioSource(IWindowsAudioSessionManager windowsSessionManager)
        {
            _windowsAudioSessionManager = windowsSessionManager;
        }

        public async Task ActivateAsync()
        {
            // TODO: Clean this up
            _windowsAudioSessionManager.SettingChanged += (sender, args) => SettingChanged?.Invoke(this, args);
            _windowsAudioSessionManager.TrackInfoChanged += (sender, args) => TrackInfoChanged?.Invoke(this, args);
            _windowsAudioSessionManager.IsPlayingChanged += (sender, args) => IsPlayingChanged?.Invoke(this, args);
            _windowsAudioSessionManager.TrackProgressChanged += (sender, args) => TrackProgressChanged?.Invoke(this, args);
            _windowsAudioSessionManager.VolumeChanged += (sender, args) => VolumeChanged?.Invoke(this, args);
            _windowsAudioSessionManager.ShuffleChanged += (sender, args) => ShuffleChanged?.Invoke(this, args);
            _windowsAudioSessionManager.RepeatModeChanged += (sender, args) => RepeatModeChanged?.Invoke(this, args);
            await _windowsAudioSessionManager.InitializeAsync(Logger);
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
            _windowsAudioSessionManager.Unintialize();

            return Task.CompletedTask;
        }

        // TODO: Only attempt each control if it's supported by the current session
        public Task NextTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TrySkipNextAsync());

        public Task PauseTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryPauseAsync());

        public Task PlayTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryPlayAsync());

        public Task PreviousTrackAsync() => LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TrySkipPreviousAsync());

        // This needs to be in ticks, per discussion here:
        // https://github.com/MicrosoftDocs/winrt-api/issues/1725
        public Task SetPlaybackProgressAsync(TimeSpan newProgress)
        {
            var currentSession = _windowsAudioSessionManager.CurrentSession;

            // Some apps (I'm looking at you Groove Music) don't support changing the playback postition, but
            // still fire a timeline properties changed event containing the initial timeline state when the
            // session is first initiated.
            // TODO: Is this actually needed if track length is unset?
            if (!currentSession?.GetPlaybackInfo()?.Controls.IsPlaybackPositionEnabled == true)
            {
                Logger.Warn("Ignoring set playback progress command: Current session does not support setting playback position");

                // Revert UI progress change from the user
                TrackProgressChanged.Invoke(this, TimeSpan.Zero);
                return Task.CompletedTask;
            }

            return LogPlayerCommandIfFailed(() => _windowsAudioSessionManager.CurrentSession?.TryChangePlaybackPositionAsync(newProgress.Ticks));
        }

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
