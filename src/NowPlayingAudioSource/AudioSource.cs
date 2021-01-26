using AudioBand.AudioSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Control;

namespace NowPlayingAudioSource
{
    public class AudioSource : IAudioSource
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


        public string Name => "Now Playing";

        public IAudioSourceLogger Logger { get; set; }

        public event EventHandler<SettingChangedEventArgs> SettingChanged;
        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;
        public event EventHandler<bool> IsPlayingChanged;
        public event EventHandler<TimeSpan> TrackProgressChanged;
        public event EventHandler<float> VolumeChanged;
        public event EventHandler<bool> ShuffleChanged;
        public event EventHandler<RepeatMode> RepeatModeChanged;

        private GlobalSystemMediaTransportControlsSessionManager _sessionManager;
        private GlobalSystemMediaTransportControlsSession _currentSession;
        private bool _isPlaying;
        private TimeSpan _trackProgress;
        private bool _shuffle;
        private RepeatMode _repeatMode;
        private string _currentTrackName;
        private string _currentArtist;
        private string _currentAlbum;

        public async Task ActivateAsync()
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
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

        public Task SetPlaybackProgressAsync(TimeSpan newProgress) =>
            LogPlayerCommandIfFailed(() => _currentSession?.TryChangePlaybackPositionAsync((long)newProgress.TotalMilliseconds));

        public Task SetRepeatModeAsync(RepeatMode newRepeatMode) =>
            LogPlayerCommandIfFailed(() => _currentSession?.TryChangeAutoRepeatModeAsync(newRepeatMode.ToMediaPlaybackAutoRepeatMode()));

        public Task SetShuffleAsync(bool shuffleOn) => LogPlayerCommandIfFailed(() => _currentSession?.TryChangeShuffleActiveAsync(shuffleOn));

        public Task SetVolumeAsync(float newVolume)
        {
            Logger.Error("Volume Not Supported!");
            return Task.CompletedTask;
        }

        private void UpdatePlaybackInfo(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger.Warn("Ignoring playback info changed event: Sender is null");
                return;
            }

            if (sender != _currentSession)
            {
                Logger.Warn("Ignoring playback info changed event: Sender is not the current session");
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
                Logger.Error(e);
            }
        }

        private void UpdateTrackProgress(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger.Warn("Ignoring track progress changed event: Sender is null");
                return;
            }

            if (sender != _currentSession)
            {
                Logger.Warn("Ignoring track progress changed event: Sender is not the current session");
                return;
            }

            try
            {
                var timelineProperties = sender.GetTimelineProperties();
                if (timelineProperties.Position != _trackProgress)
                {
                    _trackProgress = timelineProperties.Position;
                    TrackProgressChanged.Invoke(this, _trackProgress);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void UpdateTrackInfo(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger.Warn("Ignoring track info changed event: Sender is null");
                return;
            }

            if (sender != _currentSession)
            {
                Logger.Warn("Ignoring track info changed event: Sender is not the current session");
                return;
            }

            try
            {
                var mediaProperties = sender.TryGetMediaPropertiesAsync().AsTask().GetAwaiter().GetResult();
                // TODO: Restore this
                //if (_currentTrackName == mediaProperties.Title && _currentArtist == mediaProperties.Artist && _currentAlbum == mediaProperties.AlbumTitle)
                //{
                //    Logger.Debug("Ignoring track info changed event: New info is the same as the current info");
                //    return;
                //}

                // Convert media properties to event args to update track info.
                // TODO: Only try to update the image if we don't already have album art and the album hasn't changed
                var trackInfoChangedArgs = mediaProperties.ToTrackInfoChangedEventArgsAsync(includeAlbumArt: true, Logger).GetAwaiter().GetResult();
                trackInfoChangedArgs.TrackLength = sender.GetTimelineProperties().EndTime.Duration();
                _currentTrackName = trackInfoChangedArgs.TrackName;
                _currentArtist = trackInfoChangedArgs.Artist;
                _currentAlbum = trackInfoChangedArgs.Album;

                TrackInfoChanged.Invoke(this, trackInfoChangedArgs);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void ResetTrackInfo()
        {
            _currentTrackName = null;
            _currentArtist = null;
            _currentAlbum = null;
            var emptyTrackInfoChangedArgs = new TrackInfoChangedEventArgs();
            emptyTrackInfoChangedArgs.AlbumArt = null;
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

        private void UpdateCurrentSession(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger.Warn("Ignoring current session changed event: Sender is null");
                return;
            }

            SetCurrentSession(sender.GetCurrentSession());
        }

        private void UpdateSession(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            if (sender == null)
            {
                Logger.Warn("Ignoring session changed event: Sender is null");
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
                Logger.Warn("Ignoring session changed event: Current session is still the most likely best session");
                return;
            }

            Logger.Debug($"Checking for better session: {checkForBetterSessionReason}");

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
                Logger.Debug("No better session found, resetting media info and playback state");
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
                Logger.Debug($"Better session found: NewPlaybackType='{newPlaybackInfo.PlaybackType}'; NewPlaybackStatus='{newPlaybackInfo.PlaybackStatus}'; NewAppId='{newSession.SourceAppUserModelId}'; {oldSessionLogInfo}");
            }

            SetCurrentSession(newSession);
        }

        private bool ShouldCheckForBetterSession(IReadOnlyList<GlobalSystemMediaTransportControlsSession> sessions, out string reason)
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

            if (!sessions.Contains(_currentSession))
            {
                reason = "Current session no longer exists";
                return true;
            }

            reason = string.Empty;
            return false;
        }

        private GlobalSystemMediaTransportControlsSession GetNextBestSession(IReadOnlyList<GlobalSystemMediaTransportControlsSession> sessions)
        {
            try
            {
                // We only care about music sessions
                var musicSessions = sessions.Where(session => SupportedPlaybackTypes.Contains(session.GetPlaybackInfo().PlaybackType));
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
                Logger.Error(e);
            }

            return null;
        }

        private void SetCurrentSession(GlobalSystemMediaTransportControlsSession newCurrentSession)
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
                Logger.Debug("Ignoring current session changed event: Both current and new sessions are null");
                return;
            }

            // Only accept music sessions
            // TODO: Add option to exclude certain apps?
            var newSessionPlaybackType = newCurrentSession?.GetPlaybackInfo()?.PlaybackType;
            if (newSessionPlaybackType != null && !SupportedPlaybackTypes.Contains(newSessionPlaybackType))
            {
                Logger.Debug($"Ignoring current session changed event: New session is not of a supported playback type. PlaybackType='{newSessionPlaybackType}'; SupportedPlaybackTypes='{String.Join(",", SupportedPlaybackTypes)}'");
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
            }
            else
            {
                Logger.Debug("New session is null, resetting media info and playback state");

                // If there is no new session, reset everything
                ResetPlaybackInfo();
                ResetTrackInfo();
                ResetTrackProgress();
            }
        }

        private void SetSessionManager(GlobalSystemMediaTransportControlsSessionManager newSessionManager)
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
                _sessionManager.CurrentSessionChanged += UpdateCurrentSession;
                _sessionManager.SessionsChanged += UpdateSession;
            }

            // Look for the new best session from the new session manager, and update everything with it
            UpdateSession(_sessionManager, null);
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
