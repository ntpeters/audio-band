using AudioBand.AudioSource;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using WindowsAudioSource.Extensions;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    public class WindowsAudioSessionManager : IWindowsAudioSessionManager
    {
        private static readonly GlobalSystemMediaTransportControlsSessionPlaybackStatus[] SupportedPlaybackStatusesInPriorityOrder =
            new GlobalSystemMediaTransportControlsSessionPlaybackStatus[]
            {
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped
            };

        // TODO: Add setting for this?
        private static readonly ISet<MediaPlaybackType?> SupportedPlaybackTypes =
            new HashSet<MediaPlaybackType?>
            {
                MediaPlaybackType.Music
            };

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

        public string SessionSourceDisallowList
        {
            get => string.Join(",", _disallowedAppUserModelIds);

            set
            {
                _logger?.Debug($"SessionSourceDisallowList Changed: {value}");
                _disallowedAppUserModelIds = value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                OnSessionsChanged(_sessionManager, null);
            }
        }

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

        public bool MusicSessionsOnly
        {
            get => _musicSessionsOnly;

            set
            {
                _musicSessionsOnly = value;
                _logger?.Warn("MusicSessionsOnly setting not yet implemented!");
            }
        }

        public IGlobalSystemMediaTransportControlsSessionWrapper CurrentSession
        {
            get => _currentSession;
            private set => _currentSession = value;
        }

        // TODO: Change these to use native types for these APIs
        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;
        public event EventHandler<bool> IsPlayingChanged;
        public event EventHandler<TimeSpan> TrackProgressChanged;
        public event EventHandler<float> VolumeChanged;
        public event EventHandler<bool> ShuffleChanged;
        public event EventHandler<RepeatMode> RepeatModeChanged;

        private readonly IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory _windowsAudioSessionManagerFactory;
        private IAudioSourceLogger _logger;
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
        // TODO: Hookup these fields to settings
        private string _currentSourceAppUserModlelId = string.Empty;
        private string _currentSourceType = string.Empty;
        private IList<string> _disallowedAppUserModelIds = new List<string>();
        private string _currentSourceCapabilities = string.Empty;
        private bool _musicSessionsOnly = false;

        public WindowsAudioSessionManager(IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory windowsAudioSessionManagerFactory)
        {
            _windowsAudioSessionManagerFactory = windowsAudioSessionManagerFactory;
        }

        public async Task InitializeAsync(IAudioSourceLogger logger)
        {
            _logger = logger;
            var sessionManager = await _windowsAudioSessionManagerFactory.GetInstanceAsync();
            SetSessionManager(sessionManager);
        }

        public void Unintialize() => SetSessionManager(null);

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

            // Register event handlers on the new current session
            if (_sessionManager != null)
            {
                // TODO: Add setting to choose between the just system's current session or letting us try to make a better guess
                _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                // TODO: Restore this
                //_sessionManager.SessionsChanged += OnSessionsChanged;

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
                _logger?.Debug("Ignoring current session changed event: New session is already the current session");
                return;
            }

            // Only accept music sessions
            // TODO: Add option to exclude certain apps?
            // TODO: Add option to lock to a specific app?
            var newSessionPlaybackType = newCurrentSession?.GetPlaybackInfo()?.PlaybackType;
            if (newSessionPlaybackType != null && !SupportedPlaybackTypes.Contains(newSessionPlaybackType))
            {
                _logger?.Debug($"Ignoring current session changed event: New session is not of a supported playback type. PlaybackType='{newSessionPlaybackType}'; SupportedPlaybackTypes='{String.Join(",", SupportedPlaybackTypes)}'");
                return;
            }

            // Check whether the new session source has been disallowed by the user
            var newSessionSourceAppUserModelId = newCurrentSession?.SourceAppUserModelId;
            if (newSessionSourceAppUserModelId != null && _disallowedAppUserModelIds.Contains(newSessionSourceAppUserModelId))
            {
                _logger?.Debug($"Ignoring current session changed event: New session source is disallowed. SourceAppUserModelId='{newSessionSourceAppUserModelId}'; DisallowedAppUserModelIds='{SessionSourceDisallowList}'");
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
            CurrentSessionType = newSessionPlaybackType?.ToString() ?? string.Empty;

            if (_currentSession == null)
            {
                CurrentSessionCapabilities = string.Empty;
            }
            else
            {
                _currentSession.TryGetIsPlayPauseCapable(out var isPlayPauseCapable);
                _currentSession.TryGetIsNextPreviousCapable(out var isNextPreviousCapable);
                _currentSession.TryGetIsPlaybackPositionCapable(out var isPlaybackPositionCapable);
                _currentSession.TryGetIsShuffleCapable(out var isShuffleCapable);
                _currentSession.TryGetIsRepeatCapable(out var isRepeatCapable);
                CurrentSessionCapabilities = $"Play/Pause={isPlayPauseCapable}; Next/Previous={isNextPreviousCapable}; Playback Position={isPlaybackPositionCapable}; Shuffle={isShuffleCapable}; Repeat={isRepeatCapable}";
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
                _logger?.Debug("New session is null, resetting media info and playback state");

                // If there is no new session, reset everything
                ResetPlaybackInfo();
                ResetTrackInfo();
                ResetTrackProgress();
            }
        }

        #region Session Manager Event Handler Delegates
        private void OnCurrentSessionChanged(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, CurrentSessionChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger?.Warn("Ignoring current session changed event: Sender is null");
                return;
            }

            SetCurrentSession(sender.GetCurrentSession());
        }

        private void OnSessionsChanged(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, SessionsChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger?.Warn("Ignoring session changed event: Sender is null");
                return;
            }

            // TODO: do we actually want this check here?
            if (Equals(sender.GetCurrentSession(), _currentSession))
            {
                _logger?.Debug("Ignoring session changed event: Current session has not changed");
                return;
            }

            var currentSessions = sender.GetSessions();
            if (!ShouldCheckForBetterSession(currentSessions, out var checkForBetterSessionReason))
            {
                _logger?.Warn("Ignoring session changed event: Current session is still the most likely best session");
                return;
            }

            _logger?.Debug($"Checking for better session: {checkForBetterSessionReason}");

            // TODO: Restore this
            // Only attempt updating the session if our current session is no longer present
            //if (currentSessions.Contains(_currentSession))
            //{
            //    Logger.Warn("Ignoring session changed event: Current session still exists");
            //    return;
            //}

            // Try to find another session that might be one the user wants
            // If no active music session is found, we'll just assume nothing is playing and reset everything
            var newSession = GetNextBestSession(currentSessions);
            if (newSession == null)
            {
                _logger?.Debug("No valid session found, resetting media info and playback state");
            }
            else if (Equals(newSession, _currentSession))
            {
                _logger?.Debug("No better session found, keeping current session");
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
                _logger?.Debug($"Better session found: NewPlaybackType='{newPlaybackInfo.PlaybackType}'; NewPlaybackStatus='{newPlaybackInfo.PlaybackStatus}'; NewAppId='{newSession.SourceAppUserModelId}'; {oldSessionLogInfo}");
            }

            SetCurrentSession(newSession);
        }
        #endregion Session Manager Event Handler Delegates

        private bool ShouldCheckForBetterSession(IReadOnlyList<IGlobalSystemMediaTransportControlsSessionWrapper> sessions, out string reason)
        {
            if (_currentSession == null)
            {
                reason = "Current session is null";
                return true;
            }

            var currentPlaybackInfo = _currentSession.GetPlaybackInfo();
            if (!SupportedPlaybackTypes.Contains(currentPlaybackInfo.PlaybackType))
            {
                reason = $"Playback type is not supported - PlaybackType='{currentPlaybackInfo.PlaybackType}'; SupportedPlaybackTypes='{String.Join(",", SupportedPlaybackTypes)}'";
                return true;
            }

            if (!SupportedPlaybackStatusesInPriorityOrder.Contains(currentPlaybackInfo.PlaybackStatus))
            {
                reason = $"Playback status is not supported - PlaybackStatus='{currentPlaybackInfo.PlaybackStatus}'; SupportedPlaybackStatuses='{String.Join(",", SupportedPlaybackStatusesInPriorityOrder)}'";
                return true;
            }

            if (_disallowedAppUserModelIds.Contains(_currentSession.SourceAppUserModelId))
            {
                reason = $"Session source has been disallowed by the user - SourceAppUserModelId='{_currentSession.SourceAppUserModelId}'; DisallowedAppUserModelIds='{SessionSourceDisallowList}'";
                return true;
            }

            if (!sessions.Contains(_currentSession))
            {
                reason = "Current session no longer exists";
                return true;
            }

            reason = string.Empty;
            return false;
        }

        private IGlobalSystemMediaTransportControlsSessionWrapper GetNextBestSession(IReadOnlyList<IGlobalSystemMediaTransportControlsSessionWrapper> sessions)
        {
            try
            {
                // We only care about music sessions for sources that have not been disallowed by the user
                var musicSessions = sessions.Where(session =>
                SupportedPlaybackTypes.Contains(session.GetPlaybackInfo().PlaybackType) &&
                !_disallowedAppUserModelIds.Contains(session.SourceAppUserModelId));

                if (musicSessions.Count() == 1)
                {
                    return musicSessions.First();
                }
                else
                {
                    // When there is more than one music session, select the one in the "most active" playback state
                    var musicSessionsByPlaybackStatus = musicSessions.GroupBy(session => session.GetPlaybackInfo().PlaybackStatus);
                    foreach (var playbackStatus in SupportedPlaybackStatusesInPriorityOrder)
                    {
                        var musicSession = musicSessionsByPlaybackStatus.FirstInGroupOrDefault(playbackStatus);
                        if (musicSession != null)
                        {
                            return musicSession;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e);
            }

            return null;
        }

        #region Session Event Handler Delegates
        private void OnPlaybackInfoChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, PlaybackInfoChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger?.Warn("Ignoring playback info changed event: Sender is null");
                return;
            }

            if (!Equals(sender,_currentSession))
            {
                _logger?.Warn("Ignoring playback info changed event: Sender is not the current session");
                return;
            }

            try
            {
                var playbackInfo = sender.GetPlaybackInfo();
                var currentIsPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                if (currentIsPlaying != _isPlaying)
                {
                    _isPlaying = currentIsPlaying;
                    //IsPlayingChanged?.Invoke(this, _isPlaying);
                    LogEventInvocationIfFailed(IsPlayingChanged, this, _isPlaying);
                }

                if (playbackInfo.IsShuffleActive != _shuffle)
                {
                    _shuffle = playbackInfo.IsShuffleActive ?? false;
                    //ShuffleChanged?.Invoke(this, _shuffle);
                    LogEventInvocationIfFailed(ShuffleChanged, this, _shuffle);
                }

                var currentRepeatMode = playbackInfo.AutoRepeatMode.ToRepeatMode();
                if (currentRepeatMode != _repeatMode)
                {
                    _repeatMode = currentRepeatMode;
                    //RepeatModeChanged?.Invoke(this, _repeatMode);
                    LogEventInvocationIfFailed(RepeatModeChanged, this, _repeatMode);
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e);
            }
        }

        private void OnTimelinePropertiesChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, TimelinePropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger?.Warn("Ignoring track progress changed event: Sender is null");
                return;
            }

            if (!Equals(sender, _currentSession))
            {
                _logger?.Warn("Ignoring track progress changed event: Sender is not the current session");
                return;
            }

            // Some apps (I'm looking at you Groove Music) don't support changing the playback postition, but
            // still fire a timeline properties changed event containing the initial timeline state when the
            // session is first initiated.
            if (!_currentSession.GetPlaybackInfo().Controls.IsPlaybackPositionEnabled)
            {
                _logger?.Warn("Ignoring track progress changed event: Current session does not support setting playback position");

                // Ensure all subscribers know that track progress has not changed
                // TODO: Is this actually needed if track length is unset?
                _trackProgress = TimeSpan.Zero;
                LogEventInvocationIfFailed(TrackProgressChanged, this, _trackProgress);
                return;
            }

            try
            {
                var timelineProperties = sender.GetTimelineProperties();
                if (timelineProperties.Position != _trackProgress)
                {
                    _trackProgress = timelineProperties.Position;
                    //TrackProgressChanged?.Invoke(this, _trackProgress);
                    LogEventInvocationIfFailed(TrackProgressChanged, this, _trackProgress);
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e);
            }
        }

        private async void OnMediaPropertiesChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, MediaPropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger?.Warn("Ignoring track info changed event: Sender is null");
                return;
            }

            if (!Equals(sender, _currentSession))
            {
                _logger?.Warn("Ignoring track info changed event: Sender is not the current session");
                return;
            }

            try
            {
                var mediaProperties = await sender.TryGetMediaPropertiesAsync();

                // Convert media properties to event args to update track info.
                var trackInfoChangedArgs = await mediaProperties.ToTrackInfoChangedEventArgsAsync(includeAlbumArt: true, _logger);

                // Only set the track length for sessions that support setting the playback position.
                // This prevents the user from being able to change the track position when it's not supported.
                if (_currentSession.GetPlaybackInfo().Controls.IsPlaybackPositionEnabled)
                {
                    trackInfoChangedArgs.TrackLength = sender.GetTimelineProperties().EndTime.Duration();
                }
                else
                {
                    _logger?.Warn("Ignoring track length: Current session does not support setting playback position");
                    trackInfoChangedArgs.TrackLength = TimeSpan.Zero;
                }
                
                _currentTrackName = trackInfoChangedArgs.TrackName;
                _currentArtist = trackInfoChangedArgs.Artist;
                _currentAlbum = trackInfoChangedArgs.Album;
                _albumArt = trackInfoChangedArgs.AlbumArt;

                //TrackInfoChanged.Invoke(this, trackInfoChangedArgs);
                LogEventInvocationIfFailed(TrackInfoChanged, this, trackInfoChangedArgs);
            }
            catch (Exception e)
            {
                _logger?.Error(e);
            }
        }
        #endregion Session Event Handler Delegates

        private void ResetTrackInfo()
        {
            _currentTrackName = null;
            _currentArtist = null;
            _currentAlbum = null;
            var emptyTrackInfoChangedArgs = new TrackInfoChangedEventArgs();
            emptyTrackInfoChangedArgs.AlbumArt = null;  // Must be null to ensure we reset to the placeholder art
            //TrackInfoChanged.Invoke(this, emptyTrackInfoChangedArgs);
            LogEventInvocationIfFailed(TrackInfoChanged, this, emptyTrackInfoChangedArgs);

            _albumArt?.Dispose();
            _albumArt = null;
        }

        private void ResetTrackProgress()
        {
            _trackProgress = TimeSpan.Zero;
            //TrackProgressChanged.Invoke(this, _trackProgress);
            LogEventInvocationIfFailed(TrackProgressChanged, this, _trackProgress);
        }

        private void ResetPlaybackInfo()
        {
            _isPlaying = false;
            //IsPlayingChanged.Invoke(this, _isPlaying);
            LogEventInvocationIfFailed(IsPlayingChanged, this, _isPlaying);

            _shuffle = false;
            //ShuffleChanged.Invoke(this, _shuffle);
            LogEventInvocationIfFailed(ShuffleChanged, this, _shuffle);

            _repeatMode = RepeatMode.Off;
            //RepeatModeChanged.Invoke(this, _repeatMode);
            LogEventInvocationIfFailed(RepeatModeChanged, this, _repeatMode);
        }

        private void LogSessionCapabilities(IGlobalSystemMediaTransportControlsSessionWrapper session)
        {
            // These capabilities can change based on the current session state.
            // For example, when media is already playing "IsPlayEnabled" is false and "IsPauseEnabled" is true, and the opposite is the case when media is paused.
            // Likewise, when there is no next track queued "IsNextEnabled" may be false.
            var sessionControls = session.GetPlaybackInfo().Controls;
            _logger?.Debug($"New session for: '{session.SourceAppUserModelId}'. Capabilities: " +
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
                _logger?.Error($"Event handler is null. ArgsType={typeof(T)}");
            }
            else
            {
                try
                {
                    eventHandler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    _logger?.Error(e);
                }
            }
        }
    }
}
