using AudioBand.AudioSource;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Control;
using WindowsAudioSource.Extensions;
using WindowsAudioSource.Wrappers;

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
        // TODO: Add setting for "smooth" track progress, since updates don't seem to come in each second
        //       -> Maybe. Need to test how well this would work
        #endregion Brainstorming TODOs

        #region Constants
        private static readonly GlobalSystemMediaTransportControlsSessionPlaybackStatus[] SupportedPlaybackStatusesInPriorityOrder =
            new GlobalSystemMediaTransportControlsSessionPlaybackStatus[]
            {
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped
            };
        #endregion Constants

        #region Settings
        // TODO: Look into other options for controlling setting order in the UI
        // Each of these setting properties has a letter prefix at the moment to control
        // the order in which they are displayed to the user in the settings dialog.
        // It appears that the settings in the UI are placed in lexical order
        // based on the property names.
        [AudioSourceSetting(SettingConstants.CurrentSessionSourceName,
            Description = SettingConstants.CurrentSessionSourceDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionSource
        {
            get => _currentSourceAppUserModlelId;
            set
            {
                if (value == _currentSourceAppUserModlelId)
                {
                    return;
                }

                _currentSourceAppUserModlelId = value;
                LogEventInvocationIfFailed(SettingChanged, this, new SettingChangedEventArgs(SettingConstants.CurrentSessionSourceName));
            }
        }

        [AudioSourceSetting(SettingConstants.SessionSourceDisallowListName,
            Description = SettingConstants.SessionSourceDisallowListDescription,
            Priority = 10)]
        public string SessionSourceDisallowList
        {
            get => _disallowedAppUserModelIdsDisplayText;

            set
            {
                if (value == _disallowedAppUserModelIdsDisplayText)
                {
                    return;
                }

                Logger?.Debug($"SessionSourceDisallowList Changed: {value}");
                _disallowedAppUserModelIds = value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                _disallowedAppUserModelIdsDisplayText = string.Join(",", _disallowedAppUserModelIds);
                OnCurrentSessionRestrictionSettingChanged();
            }
        }

        [AudioSourceSetting(SettingConstants.CurrentSessionTypeName,
            Description = SettingConstants.CurrentSessionTypeDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionType
        {
            get => _currentSourceType;

            set
            {
                if (value == _currentSourceType)
                {
                    return;
                }

                _currentSourceType = value;
                LogEventInvocationIfFailed(SettingChanged, this, new SettingChangedEventArgs(SettingConstants.CurrentSessionTypeName));
            }
        }

        [AudioSourceSetting(SettingConstants.MusicSessionsOnlyName,
            Description = SettingConstants.MusicSessionsOnlyDescription,
            Priority = 9)]
        public bool MusicSessionsOnly
        {
            get => _musicSessionsOnly;

            set
            {
                if (value == _musicSessionsOnly)
                {
                    return;
                }

                Logger?.Debug($"MusicSessionsOnly Setting Changed: {value}");
                _musicSessionsOnly = value;
                OnCurrentSessionRestrictionSettingChanged();
            }
        }

        [AudioSourceSetting(SettingConstants.CurrentSessionCapabilitiesName,
            Description = SettingConstants.CurrentSessionCapabilitiesDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionCapabilities
        {
            get => _currentSourceCapabilities;

            set
            {
                if (value == _currentSourceCapabilities)
                {
                    return;
                }

                _currentSourceCapabilities = value;
                LogEventInvocationIfFailed(SettingChanged, this, new SettingChangedEventArgs(SettingConstants.CurrentSessionCapabilitiesName));
            }
        }
        #endregion Settings

        #region Public Properties
        public string Name => IsWindowsVersionSupported ? "Windows" : "Windows (Not Supported)";

        public IAudioSourceLogger Logger { get; set; }
        #endregion Public Properties

        #region Events
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;
        public event EventHandler<bool> IsPlayingChanged;
        public event EventHandler<TimeSpan> TrackProgressChanged;
        public event EventHandler<float> VolumeChanged;
        public event EventHandler<bool> ShuffleChanged;
        public event EventHandler<RepeatMode> RepeatModeChanged;
        #endregion Events

        #region Private Properties
        private bool IsWindowsVersionSupported => _apiInformationProvider.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
        #endregion Private Properties

        #region Instance Variables
        private readonly IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory _windowsAudioSessionManagerFactory;
        private readonly IApiInformationProvider _apiInformationProvider;

        private IGlobalSystemMediaTransportControlsSessionManagerWrapper _sessionManager;
        private IGlobalSystemMediaTransportControlsSessionWrapper _currentSession;
        private bool _isPlaying;
        private TimeSpan _trackProgress;
        private bool _shuffle;
        private RepeatMode _repeatMode;
        private string _currentTrackName;
        private string _currentArtist;
        private string _currentAlbum;
        private Image _albumArt;

        // For settings
        private string _currentSourceAppUserModlelId = string.Empty;
        private string _currentSourceType = string.Empty;
        private IList<string> _disallowedAppUserModelIds = new List<string>();
        private string _disallowedAppUserModelIdsDisplayText = string.Empty;
        private string _currentSourceCapabilities = string.Empty;
        private bool _musicSessionsOnly = false;
        #endregion Instance Variables

        #region Constructors
        public WindowsAudioSource()
            : this(new GlobalSystemMediaTransportControlsSessionManagerWrapperFactory(), new ApiInformationProvider())
        {
        }

        public WindowsAudioSource(IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory windowsAudioSessionManagerFactory, IApiInformationProvider apiInformationProvider)
        {
            _windowsAudioSessionManagerFactory = windowsAudioSessionManagerFactory;
            _apiInformationProvider = apiInformationProvider;
        }
        #endregion Constructors

        #region Public Methods
        public async Task ActivateAsync()
        {
            if (!IsWindowsVersionSupported)
            {
                Logger.Warn("Windows audio source is only supported on Windows 10 version 1809 and later");
                return;
            }

            // TODO: Clean this up
            var sessionManager = await _windowsAudioSessionManagerFactory.GetInstanceAsync();
            SetSessionManager(sessionManager);
        }

        public Task DeactivateAsync()
        {
            SetSessionManager(null);
            return Task.CompletedTask;
        }

        // TODO: Only attempt each control if it's supported by the current session
        public Task NextTrackAsync() => LogPlayerCommandIfFailed(() => _currentSession?.TrySkipNextAsync());

        public Task PauseTrackAsync() => LogPlayerCommandIfFailed(() => _currentSession?.TryPauseAsync());

        public Task PlayTrackAsync() => LogPlayerCommandIfFailed(() => _currentSession?.TryPlayAsync());

        public Task PreviousTrackAsync() => LogPlayerCommandIfFailed(() => _currentSession?.TrySkipPreviousAsync());

        // This needs to be in ticks, per discussion here:
        // https://github.com/MicrosoftDocs/winrt-api/issues/1725
        public Task SetPlaybackProgressAsync(TimeSpan newProgress)
        {
            // Some apps (I'm looking at you Groove Music) don't support changing the playback postition, but
            // still fire a timeline properties changed event containing the initial timeline state when the
            // session is first initiated.
            // TODO: Is this actually needed if track length is unset?
            if (!_currentSession?.GetPlaybackInfo()?.Controls.IsPlaybackPositionEnabled == true)
            {
                Logger.Warn("Ignoring set playback progress command: Current session does not support setting playback position");

                // Revert UI progress change from the user
                TrackProgressChanged.Invoke(this, TimeSpan.Zero);
                return Task.CompletedTask;
            }

            return LogPlayerCommandIfFailed(() => _currentSession?.TryChangePlaybackPositionAsync(newProgress.Ticks));
        }

        public Task SetRepeatModeAsync(RepeatMode newRepeatMode) =>
            LogPlayerCommandIfFailed(() => _currentSession?.TryChangeAutoRepeatModeAsync(newRepeatMode.ToMediaPlaybackAutoRepeatMode()));

        public Task SetShuffleAsync(bool shuffleOn) => LogPlayerCommandIfFailed(() => _currentSession?.TryChangeShuffleActiveAsync(shuffleOn));

        public Task SetVolumeAsync(float newVolume)
        {
            Logger.Error("Volume Not Supported!");
            return Task.CompletedTask;
        }
        #endregion Public Methods

        #region Session Management
        private void SetSessionManager(IGlobalSystemMediaTransportControlsSessionManagerWrapper newSessionManager)
        {
            // Unregister event handlers from the old session manager
            if (_sessionManager != null)
            {
                _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
                _sessionManager.SessionsChanged -= OnSessionsChanged;
            }

            // Swap the session managers
            _sessionManager = newSessionManager;

            // Register event handlers on the new current session manager
            if (_sessionManager != null)
            {
                _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                _sessionManager.SessionsChanged += OnSessionsChanged;

                // Look for the new best session from the new session manager, and update everything with it
                SetCurrentSession(_sessionManager.GetCurrentSession());
            }
            else
            {
                SetCurrentSession(null);
            }
        }

        private void SetCurrentSession(IGlobalSystemMediaTransportControlsSessionWrapper newCurrentSession)
        {
            if (Equals(newCurrentSession, _currentSession))
            {
                Logger?.Debug("Ignoring current session changed event: New session is already the current session");
                return;
            }

            // Check whether the new session has been disallowed by the user
            if (!IsSessionAllowed(newCurrentSession, out var disallowMessage))
            {
                Logger?.Info($"Ignoring current session changed event: {disallowMessage}");
                return;
            }

            // Unregister event handlers from the old session
            if (_currentSession != null)
            {
                _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            }

            // Swap the sessions
            _currentSession = newCurrentSession;
            CurrentSessionSource = _currentSession?.SourceAppUserModelId ?? string.Empty;
            CurrentSessionType = newCurrentSession?.GetPlaybackInfo()?.PlaybackType?.ToString() ?? string.Empty;

            if (_currentSession == null || !_currentSession.TryGetControls(out var playbackControls))
            {
                CurrentSessionCapabilities = string.Empty;
            }
            else
            {
                var isPlayPauseCapable = playbackControls.IsPlayEnabled || playbackControls.IsPauseEnabled || playbackControls.IsPlayPauseToggleEnabled;
                var isNextPreviousCapable = playbackControls.IsNextEnabled || playbackControls.IsPreviousEnabled;
                CurrentSessionCapabilities = $"Play/Pause={isPlayPauseCapable}; Next/Previous={isNextPreviousCapable}; Progress={playbackControls.IsPlaybackPositionEnabled}; Shuffle={playbackControls.IsShuffleEnabled}; Repeat={playbackControls.IsRepeatEnabled}";
            }

            // Reset everything before setting up the new session
            ResetPlaybackInfo();
            ResetTrackInfo();
            ResetTrackProgress();

            // Setup the new session
            if (_currentSession != null)
            {
                // Register event handlers on the new current session
                _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;

                // Update everything for the new session
                OnPlaybackInfoChanged(_currentSession, null);
                OnTimelinePropertiesChanged(_currentSession, null);
                OnMediaPropertiesChanged(_currentSession, null);

                LogSessionCapabilities(_currentSession);
            }
            else
            {
                Logger?.Debug("New session is null, resetting media info and playback state");

                // If there is no new session, reset everything
                ResetPlaybackInfo();
                ResetTrackInfo();
                ResetTrackProgress();
            }
        }
        #endregion Session Management

        #region Session Manager Event Handler Delegates
        private void OnCurrentSessionChanged(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, CurrentSessionChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger?.Debug("Ignoring current session changed event: Sender is null");
                return;
            }

            SetCurrentSession(sender.GetCurrentSession());
        }

        private void OnSessionsChanged(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, SessionsChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger?.Debug("Ignoring session changed event: Sender is null");
                return;
            }

            // Only try selecting a better session if the user has settings that restrict the current session
            if (IsSessionAllowed(_currentSession, out _))
            {
                Logger?.Debug("Ignoring sessions changed event: Current session is still allowed based on user settings");
                return;
            }

            var currentSessions = sender.GetSessions();

            Logger?.Debug($"Checking for better session based on user settings: MusicSessionsOnly='{_musicSessionsOnly}'; DisallowedAppUserModelIds='{SessionSourceDisallowList}'");

            // Try to find another session that might be one the user wants
            // If no valid session is found, we'll just assume nothing is playing and reset everything
            var newSession = GetNextBestSession(currentSessions);
            if (newSession == null)
            {
                Logger?.Debug("No valid session found");
            }
            else if (Equals(newSession, _currentSession))
            {
                Logger?.Debug("No better session found, keeping current session");
                return;
            }
            else
            {
                var oldSessionLogInfo = "[Previous session was null]";
                if (_currentSession != null)
                {
                    var currentPlaybackInfo = _currentSession.GetPlaybackInfo();
                    oldSessionLogInfo = $"OldPlaybackType='{currentPlaybackInfo.PlaybackType}'; OldPlaybackStatus='{currentPlaybackInfo.PlaybackStatus}'; OldAppId='{_currentSession.SourceAppUserModelId}'";
                }

                var newPlaybackInfo = newSession.GetPlaybackInfo();
                Logger?.Debug($"Better session found: NewPlaybackType='{newPlaybackInfo.PlaybackType}'; NewPlaybackStatus='{newPlaybackInfo.PlaybackStatus}'; NewAppId='{newSession.SourceAppUserModelId}'; {oldSessionLogInfo}");
            }

            SetCurrentSession(newSession);
        }
        #endregion Session Manager Event Handler Delegates

        #region Session Event Handler Delegates
        private void OnPlaybackInfoChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, PlaybackInfoChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger?.Debug("Ignoring playback info changed event: Sender is null");
                return;
            }

            if (!Equals(sender, _currentSession))
            {
                Logger?.Debug("Ignoring playback info changed event: Sender is not the current session");
                return;
            }

            try
            {
                var playbackInfo = sender.GetPlaybackInfo();
                var currentIsPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                if (currentIsPlaying != _isPlaying)
                {
                    _isPlaying = currentIsPlaying;
                    LogEventInvocationIfFailed(IsPlayingChanged, this, _isPlaying);
                }

                if (playbackInfo.IsShuffleActive != _shuffle)
                {
                    _shuffle = playbackInfo.IsShuffleActive ?? false;
                    LogEventInvocationIfFailed(ShuffleChanged, this, _shuffle);
                }

                var currentRepeatMode = playbackInfo.AutoRepeatMode.ToRepeatMode();
                if (currentRepeatMode != _repeatMode)
                {
                    _repeatMode = currentRepeatMode;
                    LogEventInvocationIfFailed(RepeatModeChanged, this, _repeatMode);
                }
            }
            catch (Exception e)
            {
                Logger?.Error(e);
            }
        }

        private void OnTimelinePropertiesChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, TimelinePropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger?.Debug("Ignoring track progress changed event: Sender is null");
                return;
            }

            if (!Equals(sender, _currentSession))
            {
                Logger?.Debug("Ignoring track progress changed event: Sender is not the current session");
                return;
            }

            // Some apps (I'm looking at you Groove Music) don't support changing the playback postition, but
            // still fire a timeline properties changed event containing the initial timeline state when the
            // session is first initiated.
            if (!_currentSession.GetPlaybackInfo().Controls.IsPlaybackPositionEnabled)
            {
                Logger?.Warn("Ignoring track progress changed event: Current session does not support setting playback position");

                // Ensure all subscribers know that track progress has not changed
                ResetTrackProgress();
                return;
            }

            try
            {
                var timelineProperties = sender.GetTimelineProperties();
                if (timelineProperties.Position != _trackProgress)
                {
                    _trackProgress = timelineProperties.Position;
                    LogEventInvocationIfFailed(TrackProgressChanged, this, _trackProgress);
                }
            }
            catch (Exception e)
            {
                Logger?.Error(e);
            }
        }

        private async void OnMediaPropertiesChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, MediaPropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger?.Debug("Ignoring track info changed event: Sender is null");
                return;
            }

            if (!Equals(sender, _currentSession))
            {
                Logger?.Debug("Ignoring track info changed event: Sender is not the current session");
                return;
            }

            try
            {
                var mediaProperties = await sender.TryGetMediaPropertiesAsync();

                // Convert media properties to event args to update track info.
                var trackInfoChangedArgs = await mediaProperties.ToTrackInfoChangedEventArgsAsync(Logger);

                // Only set the track length for sessions that support setting the playback position.
                // This prevents the user from being able to change the track position when it's not supported.
                if (sender.GetPlaybackInfo().Controls.IsPlaybackPositionEnabled)
                {
                    trackInfoChangedArgs.TrackLength = sender.GetTimelineProperties().EndTime.Duration();
                }
                else
                {
                    Logger?.Warn("Ignoring track length: Current session does not support setting playback position");
                    trackInfoChangedArgs.TrackLength = TimeSpan.Zero;
                }

                _currentTrackName = trackInfoChangedArgs.TrackName;
                _currentArtist = trackInfoChangedArgs.Artist;
                _currentAlbum = trackInfoChangedArgs.Album;
                _albumArt = trackInfoChangedArgs.AlbumArt;

                LogEventInvocationIfFailed(TrackInfoChanged, this, trackInfoChangedArgs);
            }
            catch (Exception e)
            {
                Logger?.Error(e);
            }
        }
        #endregion Session Event Handler Delegates

        #region Helpers
        private IGlobalSystemMediaTransportControlsSessionWrapper GetNextBestSession(IReadOnlyList<IGlobalSystemMediaTransportControlsSessionWrapper> sessions)
        {
            try
            {
                // We only care about sessions for sources that have not been disallowed by the user
                var candidateSessions = sessions.Where(session => IsSessionAllowed(session, out _));

                if (candidateSessions.Count() == 1)
                {
                    return candidateSessions.First();
                }
                else
                {
                    // When there is more than one session, select the one in the "most active" playback state
                    var candidateSessionsByPlaybackStatus = candidateSessions.GroupBy(session => session.GetPlaybackInfo().PlaybackStatus);
                    foreach (var playbackStatus in SupportedPlaybackStatusesInPriorityOrder)
                    {
                        var candidateSession = candidateSessionsByPlaybackStatus.FirstInGroupOrDefault(playbackStatus);
                        if (candidateSession != null)
                        {
                            return candidateSession;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger?.Error(e);
            }

            return null;
        }

        private void ResetTrackInfo()
        {
            _currentTrackName = null;
            _currentArtist = null;
            _currentAlbum = null;
            var emptyTrackInfoChangedArgs = new TrackInfoChangedEventArgs();
            emptyTrackInfoChangedArgs.AlbumArt = null;  // Must be null to ensure we reset to the placeholder art
            LogEventInvocationIfFailed(TrackInfoChanged, this, emptyTrackInfoChangedArgs);

            _albumArt?.Dispose();
            _albumArt = null;
        }

        private void ResetTrackProgress()
        {
            _trackProgress = TimeSpan.Zero;
            LogEventInvocationIfFailed(TrackProgressChanged, this, _trackProgress);
        }

        private void ResetPlaybackInfo()
        {
            _isPlaying = false;
            LogEventInvocationIfFailed(IsPlayingChanged, this, _isPlaying);

            _shuffle = false;
            LogEventInvocationIfFailed(ShuffleChanged, this, _shuffle);

            _repeatMode = RepeatMode.Off;
            LogEventInvocationIfFailed(RepeatModeChanged, this, _repeatMode);
        }

        private void OnCurrentSessionRestrictionSettingChanged([CallerMemberName] string caller = null)
        {
            if (_musicSessionsOnly || _disallowedAppUserModelIds.Count != 0)
            {
                Logger?.Info($"Settings restricting the current session enabled. Looking for better session. SettingChanged='{caller}'");
                OnSessionsChanged(_sessionManager, null);
            }
            else
            {
                Logger?.Info($"All settings restricting the current session disabled. Defaulting to the current session. SettingChanged='{caller}'");
                OnCurrentSessionChanged(_sessionManager, null);
            }
        }

        private bool IsSessionAllowed(IGlobalSystemMediaTransportControlsSessionWrapper session, out string disallowMessage)
        {
            // Check whether the session playback type has been disallowed by the user
            var sessionPlaybackType = session?.GetPlaybackInfo()?.PlaybackType;
            if (sessionPlaybackType != null && _musicSessionsOnly && sessionPlaybackType != MediaPlaybackType.Music)
            {
                disallowMessage = $"Session playback type is not music, and user has enabled music sessions only. PlaybackType='{sessionPlaybackType}'";
                return false;
            }

            // Check whether the session source has been disallowed by the user
            var sessionSourceAppUserModelId = session?.SourceAppUserModelId;
            if (sessionSourceAppUserModelId != null && _disallowedAppUserModelIds.Contains(sessionSourceAppUserModelId))
            {
                disallowMessage = $"Session source is disallowed by user setting. SourceAppUserModelId='{sessionSourceAppUserModelId}'; DisallowedAppUserModelIds='{SessionSourceDisallowList}'";
                return false;
            }

            disallowMessage = string.Empty;
            return true;
        }

        private void LogSessionCapabilities(IGlobalSystemMediaTransportControlsSessionWrapper session)
        {
            // These capabilities can change based on the current session state.
            // For example, when media is already playing "IsPlayEnabled" is false and "IsPauseEnabled" is true, and the opposite is the case when media is paused.
            // Likewise, when there is no next track queued "IsNextEnabled" may be false.
            var sessionControls = session.GetPlaybackInfo().Controls;
            Logger?.Debug($"New session for: '{session.SourceAppUserModelId}'. Capabilities: " +
                $"Play={sessionControls.IsPlayEnabled}; " +
                $"Pause={sessionControls.IsPauseEnabled}; " +
                $"Next={sessionControls.IsNextEnabled}; " +
                $"Previous={sessionControls.IsPreviousEnabled}; " +
                $"Repeat={sessionControls.IsRepeatEnabled}; " +
                $"Shuffle={sessionControls.IsShuffleEnabled}; " +
                $"PlaybackPosition={sessionControls.IsPlaybackPositionEnabled}; ");
        }

        private void LogEventInvocationIfFailed<T>(EventHandler<T> eventHandler, object sender, T args)
        {
            if (eventHandler == null)
            {
                Logger?.Error($"Event handler is null. ArgsType={typeof(T)}");
            }
            else
            {
                try
                {
                    eventHandler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    Logger?.Error(e);
                }
            }
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
        #endregion Helpers
    }
}
