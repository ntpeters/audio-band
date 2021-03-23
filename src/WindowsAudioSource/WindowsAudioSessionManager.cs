using AudioBand.AudioSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    public class WindowsAudioSessionManager
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

        // TODO: Hookup these properties to settings
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
                SettingChanged?.Invoke(this, new SettingChangedEventArgs("Current Session Source"));
            }
        }

        public string SessionSourceDisallowList
        {
            get => string.Join(",", _disallowedAppUserModelIds);

            set
            {
                _logger.Debug($"SessionSourceDisallowList Changed: {value}");
                _disallowedAppUserModelIds = value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                UpdateSession(_sessionManager, null);
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

        // For settings
        // TODO: Hookup these fields to settings
        private string _currentSourceAppUserModlelId = string.Empty;
        private IList<string> _disallowedAppUserModelIds = new List<string>();

        // TODO: Convert into separate factory
        public static async Task<WindowsAudioSessionManager> CreateInstance(IAudioSourceLogger logger)
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var windowsAudioSessionManager = new WindowsAudioSessionManager(logger, new GlobalSystemMediaTransportControlsSessionManagerWrapper(sessionManager));
            return windowsAudioSessionManager;
        }

        public WindowsAudioSessionManager(IAudioSourceLogger logger, IGlobalSystemMediaTransportControlsSessionManagerWrapper sessionManager)
        {
            _logger = logger;
            SetSessionManager(sessionManager);
        }

        ~WindowsAudioSessionManager()
        {
            SetSessionManager(null);
        }

        private void SetSessionManager(IGlobalSystemMediaTransportControlsSessionManagerWrapper newSessionManager)
        {
            // Unregister event handlers from the old session manager
            if (_sessionManager != null)
            {
                _sessionManager.CurrentSessionChanged -= UpdateCurrentSession;
                _sessionManager.SessionsChanged -= UpdateSession;
            }

            // Swap the session managers
            _sessionManager = newSessionManager;

            // Register event handlers on the new current session
            if (_sessionManager != null)
            {
                // TODO: Add setting to choose between the just system's current session or letting us try to make a better guess
                _sessionManager.CurrentSessionChanged += UpdateCurrentSession;
                //_sessionManager.SessionsChanged += UpdateSession;

                // Look for the new best session from the new session manager, and update everything with it
                //UpdateSession(_sessionManager, null);
                SetCurrentSession(_sessionManager.GetCurrentSession());
            }
            else
            {
                SetCurrentSession(null);
            }
        }

        private void SetCurrentSession(IGlobalSystemMediaTransportControlsSessionWrapper newCurrentSession)
        {
            // TODO: Restore this
            //if (newCurrentSession == _currentSession)
            //{
            //    Logger.Debug("Ignoring current session changed event: New session is already the current session");
            //    return;
            //}

            // TODO: Delete this
            if (newCurrentSession == null && _currentSession == null)
            {
                _logger.Debug("Ignoring current session changed event: Both current and new sessions are null");
                return;
            }

            // Only accept music sessions
            // TODO: Add option to exclude certain apps?
            // TODO: Add option to lock to a specific app?
            var newSessionPlaybackType = newCurrentSession?.GetPlaybackInfo()?.PlaybackType;
            if (newSessionPlaybackType != null && !SupportedPlaybackTypes.Contains(newSessionPlaybackType))
            {
                _logger.Debug($"Ignoring current session changed event: New session is not of a supported playback type. PlaybackType='{newSessionPlaybackType}'; SupportedPlaybackTypes='{String.Join(",", SupportedPlaybackTypes)}'");
                return;
            }

            // Check whether the new session source has been disallowed by the user
            var newSessionSourceAppUserModelId = newCurrentSession?.SourceAppUserModelId;
            if (newSessionSourceAppUserModelId != null && _disallowedAppUserModelIds.Contains(newSessionSourceAppUserModelId))
            {
                _logger.Debug($"Ignoring current session changed event: New session source is disallowed. SourceAppUserModelId='{newSessionSourceAppUserModelId}'; DisallowedAppUserModelIds='{SessionSourceDisallowList}'");
                return;
            }

            // Unregister event handlers from the old session
            if (_currentSession != null)
            {
                _currentSession.PlaybackInfoChanged -= UpdatePlaybackInfo;
                _currentSession.TimelinePropertiesChanged -= UpdateTrackProgress;
                _currentSession.MediaPropertiesChanged -= UpdateTrackInfo;
            }

            // Swap the sessions
            _currentSession = newCurrentSession;
            CurrentSessionSource = _currentSession?.SourceAppUserModelId ?? string.Empty;

            // Setup the new session
            if (_currentSession != null)
            {
                // Register event handlers on the new current session
                _currentSession.PlaybackInfoChanged += UpdatePlaybackInfo;
                _currentSession.TimelinePropertiesChanged += UpdateTrackProgress;
                _currentSession.MediaPropertiesChanged += UpdateTrackInfo;

                // Update everything for the new session
                UpdatePlaybackInfo(_currentSession, null);
                UpdateTrackProgress(_currentSession, null);
                UpdateTrackInfo(_currentSession, null);

                LogSessionCapabilities(_currentSession);
            }
            else
            {
                _logger.Debug("New session is null, resetting media info and playback state");

                // If there is no new session, reset everything
                ResetPlaybackInfo();
                ResetTrackInfo();
                ResetTrackProgress();
            }
        }

        private void UpdateCurrentSession(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, CurrentSessionChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Warn("Ignoring current session changed event: Sender is null");
                return;
            }

            SetCurrentSession(sender.GetCurrentSession());
        }

        private void UpdateSession(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, SessionsChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Warn("Ignoring session changed event: Sender is null");
                return;
            }

            // TODO: Restore this
            //if (sender.GetCurrentSession() == _currentSession)
            //{
            //    Logger.Debug("Ignoring session changed event: Current session has not changed");
            //    return;
            //}

            var currentSessions = sender.GetSessions();
            if (!ShouldCheckForBetterSession(currentSessions, out var checkForBetterSessionReason))
            {
                _logger.Warn("Ignoring session changed event: Current session is still the most likely best session");
                return;
            }

            _logger.Debug($"Checking for better session: {checkForBetterSessionReason}");

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
                _logger.Debug("No valid session found, resetting media info and playback state");
            }
            else if (newSession == _currentSession)
            {
                _logger.Debug("No better session found, keeping current session");
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
                _logger.Debug($"Better session found: NewPlaybackType='{newPlaybackInfo.PlaybackType}'; NewPlaybackStatus='{newPlaybackInfo.PlaybackStatus}'; NewAppId='{newSession.SourceAppUserModelId}'; {oldSessionLogInfo}");
            }

            SetCurrentSession(newSession);
        }

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
                    // When there is more than none music session, select the one in the "most active" playback state
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
                _logger.Error(e);
            }

            return null;
        }

        private void UpdatePlaybackInfo(IGlobalSystemMediaTransportControlsSessionWrapper sender, PlaybackInfoChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Warn("Ignoring playback info changed event: Sender is null");
                return;
            }

            if (sender != _currentSession)
            {
                _logger.Warn("Ignoring playback info changed event: Sender is not the current session");
                return;
            }

            try
            {
                var playbackInfo = sender.GetPlaybackInfo();
                var currentIsPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                if (currentIsPlaying != _isPlaying)
                {
                    _isPlaying = currentIsPlaying;
                    IsPlayingChanged.Invoke(this, _isPlaying);
                }

                if (playbackInfo.IsShuffleActive != _shuffle)
                {
                    _shuffle = playbackInfo.IsShuffleActive ?? false;
                    ShuffleChanged.Invoke(this, _shuffle);
                }

                var currentRepeatMode = playbackInfo.AutoRepeatMode.ToRepeatMode();
                if (currentRepeatMode != _repeatMode)
                {
                    _repeatMode = currentRepeatMode;
                    RepeatModeChanged.Invoke(this, _repeatMode);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void UpdateTrackProgress(IGlobalSystemMediaTransportControlsSessionWrapper sender, TimelinePropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Warn("Ignoring track progress changed event: Sender is null");
                return;
            }

            if (sender != _currentSession)
            {
                _logger.Warn("Ignoring track progress changed event: Sender is not the current session");
                return;
            }

            try
            {
                // TODO: Only set track progress if the current session *actually* supports setting it
                // Some apps (I'm looking at you Groove Music) that don't support changing the playback postition 
                // still fire a timeline properties changed event containing the initial timeline state when the
                // session is first initiated.
                var timelineProperties = sender.GetTimelineProperties();
                if (timelineProperties.Position != _trackProgress)
                {
                    _trackProgress = timelineProperties.Position;
                    TrackProgressChanged.Invoke(this, _trackProgress);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void UpdateTrackInfo(IGlobalSystemMediaTransportControlsSessionWrapper sender, MediaPropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Warn("Ignoring track info changed event: Sender is null");
                return;
            }

            if (sender != _currentSession)
            {
                _logger.Warn("Ignoring track info changed event: Sender is not the current session");
                return;
            }

            try
            {
                var mediaProperties = sender.TryGetMediaPropertiesAsync().AsTask().GetAwaiter().GetResult();
                // TODO: Restore this - Or, do we even care?
                //if (_currentTrackName == mediaProperties.Title && _currentArtist == mediaProperties.Artist && _currentAlbum == mediaProperties.AlbumTitle)
                //{
                //    Logger.Debug("Ignoring track info changed event: New info is the same as the current info");
                //    return;
                //}

                // Convert media properties to event args to update track info.
                // TODO: Only try to update the image if we don't already have album art and the album hasn't changed - Or, do we even care?
                var trackInfoChangedArgs = mediaProperties.ToTrackInfoChangedEventArgsAsync(includeAlbumArt: true, _logger).GetAwaiter().GetResult();
                trackInfoChangedArgs.TrackLength = sender.GetTimelineProperties().EndTime.Duration();
                _currentTrackName = trackInfoChangedArgs.TrackName;
                _currentArtist = trackInfoChangedArgs.Artist;
                _currentAlbum = trackInfoChangedArgs.Album;

                TrackInfoChanged.Invoke(this, trackInfoChangedArgs);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void ResetTrackInfo()
        {
            _currentTrackName = null;
            _currentArtist = null;
            _currentAlbum = null;
            var emptyTrackInfoChangedArgs = new TrackInfoChangedEventArgs();
            emptyTrackInfoChangedArgs.AlbumArt = null;  // Must be null to ensure we reset to the placeholder art
            TrackInfoChanged.Invoke(this, emptyTrackInfoChangedArgs);
        }

        private void ResetTrackProgress()
        {
            _trackProgress = TimeSpan.Zero;
            TrackProgressChanged.Invoke(this, _trackProgress);
        }

        private void ResetPlaybackInfo()
        {
            _isPlaying = false;
            IsPlayingChanged.Invoke(this, _isPlaying);

            _shuffle = false;
            ShuffleChanged.Invoke(this, _shuffle);

            _repeatMode = RepeatMode.Off;
            RepeatModeChanged.Invoke(this, _repeatMode);
        }

        private void LogSessionCapabilities(IGlobalSystemMediaTransportControlsSessionWrapper session)
        {
            // These capabilities can change based on the current session state.
            // For example, when media is already playing "IsPlayEnabled" is false and "IsPauseEnabled" is true, and the opposite is the case when media is paused.
            // Likewise, when there is no next track queued "IsNextEnabled" may be false.
            var sessionControls = session.GetPlaybackInfo().Controls;
            _logger.Debug($"New session for: '{session.SourceAppUserModelId}'. Capabilities: " +
                $"Play={sessionControls.IsPlayEnabled}; " +
                $"Pause={sessionControls.IsPauseEnabled}; " +
                $"Next={sessionControls.IsNextEnabled}; " +
                $"Previous={sessionControls.IsPreviousEnabled}; " +
                $"Repeat={sessionControls.IsRepeatEnabled}; " +
                $"Shuffle={sessionControls.IsShuffleEnabled}; " +
                $"PlaybackPosition={sessionControls.IsPlaybackPositionEnabled}; ");
        }
    }
}
