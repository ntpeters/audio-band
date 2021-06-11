using AudioBand.AudioSource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using WindowsAudioSource.Extensions;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    public class WindowsAudioSource : IAudioSource
    {
        #region Brainstorming TODOs
        // TODO: Add setting for "smooth" track progress, since updates don't seem to come in each second
        //       -> Maybe. Need to test how well this would work
        // TODO: Add setting for "forcing" stricter synchronization between the current session and playback/timeline/media properties changes, as media properties (especially album art) sometimes get out of sync
        //       -> Maybe. Depends on the complexity and if it's worth the potential tradeoffs.
        //          Primarily an issue when *very* quickly switching between multiple audio streams from the same app.
        //          Could either fire all playback/timeline/media properties changed events when any of them fire, or just use the current session rather than the sender?
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
        // NOTE: It appears that the settings in the UI are placed in lexical order based on the property names. 
        // AudioSourceSetting.Priority has no part in the setting display order, only the order in which
        // simultaneous setting changes are applied.
        [AudioSourceSetting(SettingConstants.CurrentSessionSourceName,
            Description = SettingConstants.CurrentSessionSourceDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionSource
        {
            get
            {
                lock (_currentSessionSourceMutex)
                {
                    return _currentSourceAppUserModlelId;
                }
            }

            set
            {
                lock (_currentSessionSourceMutex)
                {
                    if (value == _currentSourceAppUserModlelId)
                    {
                        return;
                    }

                    _currentSourceAppUserModlelId = value;
                }

                LogEventInvocationIfFailed(SettingChanged, this, new SettingChangedEventArgs(SettingConstants.CurrentSessionSourceName));
            }
        }

        [AudioSourceSetting(SettingConstants.SessionSourceDisallowListName,
            Description = SettingConstants.SessionSourceDisallowListDescription,
            Priority = 10)]
        public string SessionSourceDisallowList
        {
            get
            {
                lock (_sessionSourceDisallowListMutex)
                {
                    return _disallowedAppUserModelIdsDisplayText;
                }
            }

            set
            {
                IList<string> newDisallowedAppUserModelIdsValue;
                string newDisallowedAppUserModelIdsDisplayText;
                bool valueDidChange;
                lock (_sessionSourceDisallowListMutex)
                {
                    if (value == _disallowedAppUserModelIdsDisplayText)
                    {
                        return;
                    }

                    _disallowedAppUserModelIds = value.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    _disallowedAppUserModelIdsDisplayText = string.Join(",", _disallowedAppUserModelIds);
                    newDisallowedAppUserModelIdsValue = _disallowedAppUserModelIds;
                    newDisallowedAppUserModelIdsDisplayText = _disallowedAppUserModelIdsDisplayText;
                    valueDidChange = value != _disallowedAppUserModelIdsDisplayText;
                }

                _logger.Debug($"SessionSourceDisallowList Changed: {newDisallowedAppUserModelIdsDisplayText}");
                OnSessionSourceDisallowListSettingChanged(newDisallowedAppUserModelIdsValue);

                // If the raw text value of the setting changed then update the settings UI
                if (valueDidChange)
                {
                    LogEventInvocationIfFailed(SettingChanged, this, new SettingChangedEventArgs(SettingConstants.SessionSourceDisallowListName));
                }
            }
        }

        [AudioSourceSetting(SettingConstants.CurrentSessionTypeName,
            Description = SettingConstants.CurrentSessionTypeDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionType
        {
            get
            {
                lock (_currentSessionTypeMutex)
                {
                    return _currentSourceType;
                }
            }

            set
            {
                lock (_currentSessionTypeMutex)
                {
                    if (value == _currentSourceType)
                    {
                        return;
                    }

                    _currentSourceType = value;
                }

                LogEventInvocationIfFailed(SettingChanged, this, new SettingChangedEventArgs(SettingConstants.CurrentSessionTypeName));
            }
        }

        [AudioSourceSetting(SettingConstants.MusicSessionsOnlyName,
            Description = SettingConstants.MusicSessionsOnlyDescription,
            Priority = 9)]
        public bool MusicSessionsOnly
        {
            get
            {
                lock (_musicSessionsOnlyMutex)
                {
                    return _musicSessionsOnly;
                }
            }

            set
            {
                bool newMusicSessionsOnlyValue;
                lock (_musicSessionsOnlyMutex)
                {
                    if (value == _musicSessionsOnly)
                    {
                        return;
                    }

                    _musicSessionsOnly = value;
                    newMusicSessionsOnlyValue = _musicSessionsOnly;
                }

                _logger.Debug($"MusicSessionsOnly Setting Changed: {value}");
                OnMusicSessionsOnlySettingChanged(newMusicSessionsOnlyValue);
            }
        }

        [AudioSourceSetting(SettingConstants.CurrentSessionCapabilitiesName,
            Description = SettingConstants.CurrentSessionCapabilitiesDescription,
            Options = SettingOptions.ReadOnly)]
        public string CurrentSessionCapabilities
        {
            get
            {
                lock (_currentSessionCapabilitiesMutex)
                {
                    return _currentSourceCapabilities;
                }
            }

            set
            {
                lock (_currentSessionCapabilitiesMutex)
                {
                    if (value == _currentSourceCapabilities)
                    {
                        return;
                    }

                    _currentSourceCapabilities = value;
                }

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
        private readonly SessionLogger _logger;

        private IGlobalSystemMediaTransportControlsSessionManagerWrapper _sessionManager;
        private IGlobalSystemMediaTransportControlsSessionWrapper _currentSession;

        // State
        private bool _isPlaying;
        private TimeSpan _trackProgress;
        private bool _shuffle;
        private RepeatMode _repeatMode;
        private Image _albumArt;
        private bool _isPlaybackPositionEnabled;

        // Settings
        private string _currentSourceAppUserModlelId = string.Empty;
        private string _currentSourceType = string.Empty;
        private IList<string> _disallowedAppUserModelIds = new List<string>();
        private string _disallowedAppUserModelIdsDisplayText = string.Empty;
        private string _currentSourceCapabilities = string.Empty;
        private bool _musicSessionsOnly = false;

        // Synchronization
        private readonly object _sessionManagerMutex = new object();
        private readonly object _currentSessionMutex = new object();

        private readonly object _isPlayingMutex = new object();
        private readonly object _trackProgressMutex = new object();
        private readonly object _shuffleMutex = new object();
        private readonly object _repeatModeMutex = new object();
        private readonly object _isPlaybackProgressEnabledMutex = new object();
        private readonly SemaphoreSlim _mediaPropertiesSemaphore = new SemaphoreSlim(1, 1);

        private readonly object _currentSessionSourceMutex = new object();
        private readonly object _sessionSourceDisallowListMutex = new object();
        private readonly object _currentSessionTypeMutex = new object();
        private readonly object _musicSessionsOnlyMutex = new object();
        private readonly object _currentSessionCapabilitiesMutex = new object();
        #endregion Instance Variables

        #region Constructors
        public WindowsAudioSource()
            : this(new GlobalSystemMediaTransportControlsSessionManagerWrapperFactory(), new ApiInformationProvider())
        {
        }

        public WindowsAudioSource(IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory windowsAudioSessionManagerFactory, IApiInformationProvider apiInformationProvider)
        {
            _windowsAudioSessionManagerFactory = windowsAudioSessionManagerFactory ?? throw new ArgumentNullException(nameof(windowsAudioSessionManagerFactory));
            _apiInformationProvider = apiInformationProvider ?? throw new ArgumentNullException(nameof(apiInformationProvider));

            // Intentionally not locking around current session here as it would cause too much contention due to the lock being aquired for every log call
            // This is only for log attribution, so it's not an issue if it's not always 100% accurate
            _logger = new SessionLogger(() => Logger, () => _currentSession?.SourceAppUserModelId);
        }
        #endregion Constructors

        #region Activation
        public async Task ActivateAsync()
        {
            if (!IsWindowsVersionSupported)
            {
                _logger.Warn("Windows audio source is only supported on Windows 10 version 1809 and later");
                return;
            }

            var sessionManager = await _windowsAudioSessionManagerFactory.GetInstanceAsync();
            SetSessionManager(sessionManager);
        }

        public Task DeactivateAsync()
        {
            SetSessionManager(null);
            return Task.CompletedTask;
        }
        #endregion Activation

        #region Controls
        public Task NextTrackAsync() =>
            LogPlayerCommandIfFailed(() =>
                {
                    lock (_currentSessionMutex)
                    {
                        if (_currentSession == null)
                        {
                            return Task.FromResult(false);
                        }

                        return _currentSession.TrySkipNextAsync().AsTask();
                    }
                });

        public Task PauseTrackAsync() =>
            LogPlayerCommandIfFailed(() =>
                {
                    lock (_currentSessionMutex)
                    {
                        if (_currentSession == null)
                        {
                            return Task.FromResult(false);
                        }

                        return _currentSession.TryPauseAsync().AsTask();
                    }
                });

        public Task PlayTrackAsync() =>
                LogPlayerCommandIfFailed(() =>
                {
                    lock (_currentSessionMutex)
                    {
                        if (_currentSession == null)
                        {
                            return Task.FromResult(false);
                        }

                        return _currentSession.TryPlayAsync().AsTask();
                    }
                });

        public Task PreviousTrackAsync() =>
            LogPlayerCommandIfFailed(() =>
                {
                    lock (_currentSessionMutex)
                    {
                        if (_currentSession == null)
                        {
                            return Task.FromResult(false);
                        }

                        return _currentSession.TrySkipPreviousAsync().AsTask();
                    }
                });

        public Task SetPlaybackProgressAsync(TimeSpan newProgress)
        {
            // Some apps (I'm looking at you Groove Music) don't support changing the playback postition, but
            // still fire a timeline properties changed event containing the initial timeline state when the
            // session is first initiated.
            bool isPlaybackPositionEnabled;
            lock (_currentSessionMutex)
            {
                isPlaybackPositionEnabled = _currentSession?.GetPlaybackInfo()?.Controls.IsPlaybackPositionEnabled == true;
            }

            if (!isPlaybackPositionEnabled)
            {
                _logger.Warn("Ignoring set playback progress command: Current session does not support setting playback position");

                // Revert UI progress change from the user
                LogEventInvocationIfFailed(TrackProgressChanged, this, TimeSpan.Zero);
                return Task.CompletedTask;
            }

            return LogPlayerCommandIfFailed(() =>
                {
                    lock (_currentSessionMutex)
                    {
                        if (_currentSession == null)
                        {
                            return Task.FromResult(false);
                        }

                        // This needs to be in ticks, per discussion here:
                        // https://github.com/MicrosoftDocs/winrt-api/issues/1725
                        return _currentSession.TryChangePlaybackPositionAsync(newProgress.Ticks).AsTask();
                    }
                });
        }

        public Task SetRepeatModeAsync(RepeatMode newRepeatMode) =>
            LogPlayerCommandIfFailed(() =>
                {
                    lock (_currentSessionMutex)
                    {
                        if (_currentSession == null)
                        {
                            return Task.FromResult(false);
                        }

                        return _currentSession.TryChangeAutoRepeatModeAsync(newRepeatMode.ToMediaPlaybackAutoRepeatMode()).AsTask();
                    }
                });

        public Task SetShuffleAsync(bool shuffleOn) =>
            LogPlayerCommandIfFailed(() =>
                {
                    lock (_currentSessionMutex)
                    {
                        if (_currentSession == null)
                        {
                            return Task.FromResult(false);
                        }

                        return _currentSession.TryChangeShuffleActiveAsync(shuffleOn).AsTask();
                    }
                });

        public Task SetVolumeAsync(float newVolume)
        {
            _logger.Error("Volume Not Supported!");
            return Task.CompletedTask;
        }
        #endregion Controls

        #region Session Management
        private void SetSessionManager(IGlobalSystemMediaTransportControlsSessionManagerWrapper newSessionManager)
        {
            lock (_sessionManagerMutex)
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
                }
            }

            if (_sessionManager != null)
            {
                // Get the current session from the new session manager, and update everything with it
                SetCurrentSession(_sessionManager.GetCurrentSession());
            }
            else
            {
                SetCurrentSession(null);
            }
        }

        private void SetCurrentSession(IGlobalSystemMediaTransportControlsSessionWrapper newCurrentSession)
        {
            // Check whether the new session has been disallowed by the user
            if (!IsSessionAllowed(newCurrentSession, out var disallowMessage))
            {
                _logger.Info($"Ignoring current session changed event: {disallowMessage}");
                return;
            }

            lock (_currentSessionMutex)
            {
                if (Equals(newCurrentSession, _currentSession))
                {
                    _logger.Debug("Ignoring current session changed event: New session is already the current session");
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

                // Register event handlers on the new current session
                if (_currentSession != null)
                {
                    _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                    _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
                    _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                }
            }

            // Setup the new session
            CurrentSessionSource = newCurrentSession?.SourceAppUserModelId ?? string.Empty;
            if (newCurrentSession != null)
            {
                // Update everything for the new session
                OnPlaybackInfoChanged(newCurrentSession, null);
                OnTimelinePropertiesChanged(newCurrentSession, null);
                OnMediaPropertiesChanged(newCurrentSession, null);

                LogSessionCapabilities(newCurrentSession);
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

        #endregion Session Management

        #region Session Manager Event Handler Delegates
        private void OnCurrentSessionChanged(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, CurrentSessionChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Debug("Ignoring current session changed event: Sender is null");
                return;
            }

            SetCurrentSession(sender.GetCurrentSession());
        }

        private void OnSessionsChanged(IGlobalSystemMediaTransportControlsSessionManagerWrapper sender, SessionsChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Debug("Ignoring session changed event: Sender is null");
                return;
            }

            string disallowMessage;
            lock (_currentSessionMutex)
            {
                // Only try selecting a better session if the user has settings that restrict the current session
                if (IsSessionAllowed(_currentSession, out disallowMessage))
                {
                    _logger.Debug("Ignoring sessions changed event: Current session is still allowed based on user settings");
                    return;
                }
            }

            var currentSessions = sender.GetSessions();

            _logger.Debug($"Checking for better session based on user settings: {disallowMessage}");

            // Try to find another session that might be one the user wants
            // If no valid session is found, we'll just assume nothing is playing and reset everything
            var newSession = GetNextBestSession(currentSessions);
            if (newSession == null)
            {
                _logger.Debug("No valid session found");
            }
            else
            {
                var newPlaybackInfo = newSession.GetPlaybackInfo();
                _logger.Debug($"Better session candidate found: NewPlaybackType='{newPlaybackInfo.PlaybackType}'; NewPlaybackStatus='{newPlaybackInfo.PlaybackStatus}'; NewAppId='{newSession.SourceAppUserModelId}'");
            }

            SetCurrentSession(newSession);
        }
        #endregion Session Manager Event Handler Delegates

        #region Session Event Handler Delegates
        private void OnPlaybackInfoChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, PlaybackInfoChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Debug("Ignoring playback info changed event: Sender is null");
                return;
            }

            lock (_currentSessionMutex)
            {
                if (!Equals(sender, _currentSession))
                {
                    _logger.Debug("Ignoring playback info changed event: Sender is not the current session");
                    return;
                }
            }

            try
            {
                var playbackInfo = sender.GetPlaybackInfo();
                var currentIsPlaying = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                if (TrySetIsPlaying(currentIsPlaying))
                {
                    LogEventInvocationIfFailed(IsPlayingChanged, this, currentIsPlaying);
                }

                var currentShuffle = playbackInfo.IsShuffleActive ?? false;
                if (TrySetShuffle(currentShuffle))
                {
                    LogEventInvocationIfFailed(ShuffleChanged, this, currentShuffle);
                }

                var currentRepeatMode = playbackInfo.AutoRepeatMode.ToRepeatMode();
                if (TrySetRepeatMode(currentRepeatMode))
                {
                    LogEventInvocationIfFailed(RepeatModeChanged, this, currentRepeatMode);
                }

                CurrentSessionType = playbackInfo.PlaybackType?.ToString() ?? string.Empty;

                var playbackControls = playbackInfo?.Controls;
                if (playbackControls == null)
                {
                    CurrentSessionCapabilities = string.Empty;
                }
                else
                {
                    var isPlayPauseCapable = playbackControls.IsPlayEnabled || playbackControls.IsPauseEnabled || playbackControls.IsPlayPauseToggleEnabled;
                    var isNextPreviousCapable = playbackControls.IsNextEnabled || playbackControls.IsPreviousEnabled;
                    CurrentSessionCapabilities = $"Play/Pause={isPlayPauseCapable}; Next/Previous={isNextPreviousCapable}; Progress={playbackControls.IsPlaybackPositionEnabled}; Shuffle={playbackControls.IsShuffleEnabled}; Repeat={playbackControls.IsRepeatEnabled}";

                    // If the playback position capability has changed, timeline and media properties are updated to include the track length and position
                    if (TrySetIsPlaybackPositionEnabled(playbackControls.IsPlaybackPositionEnabled))
                    {
                        OnTimelinePropertiesChanged(sender, null);
                        OnMediaPropertiesChanged(sender, null);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void OnTimelinePropertiesChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, TimelinePropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Debug("Ignoring track progress changed event: Sender is null");
                return;
            }

            lock (_currentSessionMutex)
            {
                if (!Equals(sender, _currentSession))
                {
                    _logger.Debug("Ignoring track progress changed event: Sender is not the current session");
                    return;
                }
            }

            // Some apps (I'm looking at you Groove Music) don't support changing the playback postition, but
            // still fire a timeline properties changed event containing the initial timeline state when the
            // session is first initiated.
            if (!sender.GetPlaybackInfo().Controls.IsPlaybackPositionEnabled)
            {
                _logger.Warn("Ignoring track progress changed event: Current session does not support setting playback position");

                // Ensure all subscribers know that track progress has not changed
                ResetTrackProgress();
                return;
            }

            try
            {
                var currentTrackProgress = sender.GetTimelineProperties().Position;
                if (TrySetTrackProgress(currentTrackProgress))
                {
                    LogEventInvocationIfFailed(TrackProgressChanged, this, currentTrackProgress);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async void OnMediaPropertiesChanged(IGlobalSystemMediaTransportControlsSessionWrapper sender, MediaPropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                _logger.Debug("Ignoring track info changed event: Sender is null");
                return;
            }

            lock (_currentSessionMutex)
            {
                if (!Equals(sender, _currentSession))
                {
                    _logger.Debug("Ignoring track info changed event: Sender is not the current session");
                    return;
                }
            }

            _mediaPropertiesSemaphore.Wait();
            TrackInfoChangedEventArgs trackInfoChangedArgs = null;
            try
            {
                var mediaProperties = await sender.TryGetMediaPropertiesAsync();

                // Only set the track length for sessions that support setting the playback position.
                // This prevents the user from being able to change the track position when it's not supported.
                var trackLength = TimeSpan.Zero;
                if (sender.GetPlaybackInfo().Controls.IsPlaybackPositionEnabled)
                {
                    trackLength = sender.GetTimelineProperties().EndTime.Duration();
                }
                else
                {
                    _logger.Warn("Ignoring track length: Current session does not support setting playback position");
                }

                // Try to convert the album art thumbnail
                var (albumArt, albumArtError) = await mediaProperties.Thumbnail.TryToImageAsync();
                if (albumArt == null)
                {
                    _logger.Debug($"Failed to read album art: {albumArtError}");
                }

                // Convert media properties to event args to update track info
                trackInfoChangedArgs = new TrackInfoChangedEventArgs
                {
                    TrackName = mediaProperties.Title,
                    Artist = mediaProperties.Artist,
                    Album = mediaProperties.AlbumTitle,
                    TrackLength = trackLength,
                    AlbumArt = albumArt
                };

                using (var oldAlbumArt = _albumArt)
                {
                    _albumArt = albumArt;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                _mediaPropertiesSemaphore.Release();
            }

            // Always raise the TrackInfoChanged event if we were able to compose TrackInfoChangedArgs.
            // Unlike other state changes we intentionally do not hold and compare the values for track info
            // to determine whether the event should be raised only when those values change.
            // This is because sometimes the change to album art is received as a separate event invocation
            // after the other track info has already been updated, and comparing the album art for each
            // invocation would be unnecessarily expensive.
            // Instead we simply always update the track info regardless of what, if any, changes there were.
            if (trackInfoChangedArgs != null)
            {
                LogEventInvocationIfFailed(TrackInfoChanged, this, trackInfoChangedArgs);
            }
        }
        #endregion Session Event Handler Delegates

        #region State Management

        private void ResetTrackInfo()
        {
            var emptyTrackInfoChangedArgs = new TrackInfoChangedEventArgs();
            emptyTrackInfoChangedArgs.AlbumArt = null;  // Must be null to ensure we reset to the placeholder art

            _mediaPropertiesSemaphore.Wait();
            try
            {
                using (var oldAlbumArt = _albumArt)
                {
                    _albumArt = null;
                }
            }
            finally
            {
                _mediaPropertiesSemaphore.Release();
            }

            LogEventInvocationIfFailed(TrackInfoChanged, this, emptyTrackInfoChangedArgs);
        }

        private void ResetTrackProgress()
        {
            var newTrackProgress = TimeSpan.Zero;
            if (TrySetTrackProgress(newTrackProgress))
            {
                LogEventInvocationIfFailed(TrackProgressChanged, this, newTrackProgress);
            }
        }

        private void ResetPlaybackInfo()
        {
            var newIsPlaying = false;
            if (TrySetIsPlaying(newIsPlaying))
            {
                LogEventInvocationIfFailed(IsPlayingChanged, this, newIsPlaying);
            }

            var newShuffle = false;
            if (TrySetShuffle(newShuffle))
            {
                LogEventInvocationIfFailed(ShuffleChanged, this, newShuffle);
            }

            var newRepeatMode = RepeatMode.Off;
            if (TrySetRepeatMode(newRepeatMode))
            {
                LogEventInvocationIfFailed(RepeatModeChanged, this, newRepeatMode);
            }

            var newIsPlaybackPositionEnabled = false;
            if (TrySetIsPlaybackPositionEnabled(newIsPlaybackPositionEnabled))
            {
                IGlobalSystemMediaTransportControlsSessionWrapper session;
                lock (_currentSessionMutex)
                {
                    session = _currentSession;
                }
                OnTimelinePropertiesChanged(session, null);
                OnMediaPropertiesChanged(session, null);
            }

            CurrentSessionType = string.Empty;
            CurrentSessionCapabilities = string.Empty;
        }

        private bool TrySetIsPlaying(bool newIsPlaying)
        {
            lock (_isPlayingMutex)
            {
                if (newIsPlaying == _isPlaying)
                {
                    return false;
                }

                _isPlaying = newIsPlaying;
                return true;
            }
        }

        private bool TrySetShuffle(bool newShuffle)
        {
            lock (_shuffleMutex)
            {
                if (newShuffle == _shuffle)
                {
                    return false;
                }

                _shuffle = newShuffle;
                return true;
            }
        }

        private bool TrySetRepeatMode(RepeatMode newRepeatMode)
        {
            lock (_repeatModeMutex)
            {
                if (newRepeatMode == _repeatMode)
                {
                    return false;
                }

                _repeatMode = newRepeatMode;
                return true;
            }
        }

        private bool TrySetIsPlaybackPositionEnabled(bool newIsPlaybackPositionEnabled)
        {
            lock (_isPlaybackProgressEnabledMutex)
            {
                if (newIsPlaybackPositionEnabled == _isPlaybackPositionEnabled)
                {
                    return false;
                }

                _isPlaybackPositionEnabled = newIsPlaybackPositionEnabled;
                return true;
            }
        }

        private bool TrySetTrackProgress(TimeSpan newTrackProgress)
        {
            lock (_trackProgressMutex)
            {
                if (newTrackProgress == _trackProgress)
                {
                    return false;
                }

                _trackProgress = newTrackProgress;
                return true;
            }
        }
        #endregion State Management

        #region Setting Event Handler Delegates

        private void OnMusicSessionsOnlySettingChanged(bool newMusicSessionsOnlyValue) => OnCurrentSessionRestrictionSettingChanged(newMusicSessionsOnlyValue, null);

        private void OnSessionSourceDisallowListSettingChanged(IList<string> newSessionSourceDisallowListValue) => OnCurrentSessionRestrictionSettingChanged(null, newSessionSourceDisallowListValue);

        private void OnCurrentSessionRestrictionSettingChanged(bool? newMusicSessionsOnlyValue, IList<string> newSessionSourceDisallowListValue, [CallerMemberName] string caller = null)
        {
            IGlobalSystemMediaTransportControlsSessionManagerWrapper currentSessionManager;
            lock (_sessionManagerMutex)
            {
                currentSessionManager = _sessionManager;
            }

            if (newMusicSessionsOnlyValue == true || newSessionSourceDisallowListValue?.Count > 0)
            {
                _logger.Info($"Settings restricting the current session enabled. Looking for better session. SettingChanged='{caller}'");
                OnSessionsChanged(currentSessionManager, null);
            }
            else
            {
                _logger.Info($"All settings restricting the current session disabled. Defaulting to the current session. SettingChanged='{caller}'");
                OnCurrentSessionChanged(currentSessionManager, null);
            }
        }
        #endregion Setting Event Handler Delegates

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
                _logger.Error(e);
            }

            return null;
        }

        private bool IsSessionAllowed(IGlobalSystemMediaTransportControlsSessionWrapper session, out string disallowMessage)
        {
            disallowMessage = string.Empty;
            if (session == null)
            {
                return true;
            }

            // Check whether the session playback type has been disallowed by the user
            var sessionPlaybackType = session.GetPlaybackInfo()?.PlaybackType;
            if (sessionPlaybackType != null && sessionPlaybackType != MediaPlaybackType.Music)
            {
                // Only aquire the lock if the setting is applicable to the current state
                lock (_musicSessionsOnlyMutex)
                {
                    if (_musicSessionsOnly)
                    {
                        disallowMessage = $"Session playback type is not music, and user has enabled music sessions only. PlaybackType='{sessionPlaybackType}'";
                        return false;
                    }
                }
            }

            // Check whether the session source has been disallowed by the user
            var sessionSourceAppUserModelId = session.SourceAppUserModelId;
            if (!string.IsNullOrWhiteSpace(sessionSourceAppUserModelId))
            {
                // Only aquire the lock if the setting is applicable to the current state
                lock (_sessionSourceDisallowListMutex)
                {
                    if (_disallowedAppUserModelIds.Contains(sessionSourceAppUserModelId))
                    {
                        disallowMessage = $"Session source is disallowed by user setting. SourceAppUserModelId='{sessionSourceAppUserModelId}'; DisallowedAppUserModelIds='{_disallowedAppUserModelIdsDisplayText}'";
                        return false;
                    }
                }
            }

            return true;
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

        private void LogEventInvocationIfFailed<T>(EventHandler<T> eventHandler, object sender, T args)
        {
            // Sanity check in debug builds to catch event invocations while locked
            // Monitor.IsEntered should be sufficient since we only care that the event isn't being raised from within a locked context, which by definition only applies to the current thread
            Debug.Assert(!Monitor.IsEntered(_currentSessionMutex), "CurrentSession lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_sessionManagerMutex), "SessionManager lock held during event invocation");

            Debug.Assert(!Monitor.IsEntered(_isPlayingMutex), "IsPlaying lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_trackProgressMutex), "TrackProgress lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_shuffleMutex), "Shuffle lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_repeatModeMutex), "RepeatMode lock held during event invocation");

            Debug.Assert(!Monitor.IsEntered(_currentSessionSourceMutex), "CurrentSessionSource lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_sessionSourceDisallowListMutex), "SessionSourceDisallowList lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_currentSessionTypeMutex), "CurrentSessionType lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_musicSessionsOnlyMutex), "MusicSessionsOnly lock held during event invocation");
            Debug.Assert(!Monitor.IsEntered(_currentSessionCapabilitiesMutex), "CurrentSessionCapabilities lock held during event invocation");

            if (eventHandler == null)
            {
                _logger.Error($"Event handler is null. ArgsType={typeof(T)}");
            }
            else
            {
                try
                {
                    eventHandler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
        }

        private async Task LogPlayerCommandIfFailed(Func<Task<bool>> command, [CallerMemberName] string caller = null)
        {
            try
            {
                var success = await command();
                if (!success)
                {
                    _logger.Warn($"Player command failed: '{caller}'");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
        #endregion Helpers
    }
}
