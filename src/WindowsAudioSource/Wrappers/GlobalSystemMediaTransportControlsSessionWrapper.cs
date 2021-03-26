using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Control;
using WindowsAudioSource.Extensions;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="IGlobalSystemMediaTransportControlsSessionWrapper"/>
    public class GlobalSystemMediaTransportControlsSessionWrapper : IGlobalSystemMediaTransportControlsSessionWrapper
    {
        #region Public Events
        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, MediaPropertiesChangedEventArgs> MediaPropertiesChanged
        {
            add
            {
                // Lazily subscribe to the session event once someone subscribes to this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                if (!MediaPropertiesChangedInternal.HasSubscribers())
                {
                    _session.MediaPropertiesChanged += OnMediaPropertiesChanged;
                }
                MediaPropertiesChangedInternal += value;
            }

            remove
            {
                // Lazily unsubscribe from the session event once the last subscriber unsubscribes from this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                MediaPropertiesChangedInternal -= value;
                if (!MediaPropertiesChangedInternal.HasSubscribers())
                {
                    _session.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                }
            }
        }

        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, PlaybackInfoChangedEventArgs> PlaybackInfoChanged
        {
            add
            {
                // Lazily subscribe to the session event once someone subscribes to this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                if (!PlaybackInfoChangedInternal.HasSubscribers())
                {
                    _session.PlaybackInfoChanged += OnPlaybackInfoChanged;
                }
                PlaybackInfoChangedInternal += value;
            }

            remove
            {
                // Lazily unsubscribe from the session event once the last subscriber unsubscribes from this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                PlaybackInfoChangedInternal -= value;
                if (!PlaybackInfoChangedInternal.HasSubscribers())
                {
                    _session.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                }
            }
        }

        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, TimelinePropertiesChangedEventArgs> TimelinePropertiesChanged
        {
            add
            {
                // Lazily subscribe to the session event once someone subscribes to this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                if (!TimelinePropertiesChangedInternal.HasSubscribers())
                {
                    _session.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
                }
                TimelinePropertiesChangedInternal += value;
            }

            remove
            {
                // Lazily unsubscribe from the session event once the last subscriber unsubscribes from this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                TimelinePropertiesChangedInternal -= value;
                if (!TimelinePropertiesChangedInternal.HasSubscribers())
                {
                    _session.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
                }
            }
        }
        #endregion Public Events

        public string SourceAppUserModelId => _session.SourceAppUserModelId;
        public GlobalSystemMediaTransportControlsSession WrappedInstance => _session;

        // Internal events used to allow lazy subscribing to the session events only when needed
        private event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, MediaPropertiesChangedEventArgs> MediaPropertiesChangedInternal;
        private event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, PlaybackInfoChangedEventArgs> PlaybackInfoChangedInternal;
        private event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, TimelinePropertiesChangedEventArgs> TimelinePropertiesChangedInternal;

        private GlobalSystemMediaTransportControlsSession _session;

        public GlobalSystemMediaTransportControlsSessionWrapper(GlobalSystemMediaTransportControlsSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        #region Wrapped Methods
        public IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper GetPlaybackInfo()
        {
            var playbackInfo = _session.GetPlaybackInfo();
            if (playbackInfo == null)
            {
                return null;
            }

            return new GlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper(playbackInfo);
        }

        public IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper GetTimelineProperties()
        {
            var timelineProperties = _session.GetTimelineProperties();
            if (timelineProperties == null)
            {
                return null;
            }

            return new GlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper(timelineProperties);
        }

        public IAsyncOperation<bool> TryChangeAutoRepeatModeAsync(MediaPlaybackAutoRepeatMode requestedAutoRepeatMode) =>
            _session.TryChangeAutoRepeatModeAsync(requestedAutoRepeatMode);

        public IAsyncOperation<bool> TryChangeChannelDownAsync() => _session.TryChangeChannelDownAsync();

        public IAsyncOperation<bool> TryChangeChannelUpAsync() => _session.TryChangeChannelUpAsync();

        public IAsyncOperation<bool> TryChangePlaybackPositionAsync(long requestedPlaybackPosition) =>
            _session.TryChangePlaybackPositionAsync(requestedPlaybackPosition);

        public IAsyncOperation<bool> TryChangePlaybackRateAsync(double requestedPlaybackRate) =>
            _session.TryChangePlaybackRateAsync(requestedPlaybackRate);

        public IAsyncOperation<bool> TryChangeShuffleActiveAsync(bool requestedShuffleState) =>
            _session.TryChangeShuffleActiveAsync(requestedShuffleState);

        public IAsyncOperation<bool> TryFastForwardAsync() => _session.TryFastForwardAsync();

        public IAsyncOperation<IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper> TryGetMediaPropertiesAsync() =>
            TryGetMediaPropertiesInternalAsync().AsAsyncOperation();

        public IAsyncOperation<bool> TryPauseAsync() => _session.TryPauseAsync();

        public IAsyncOperation<bool> TryPlayAsync() => _session.TryPlayAsync();

        public IAsyncOperation<bool> TryRecordAsync() => _session.TryRecordAsync();

        public IAsyncOperation<bool> TryRewindAsync() => _session.TryRewindAsync();

        public IAsyncOperation<bool> TrySkipNextAsync() => _session.TrySkipNextAsync();

        public IAsyncOperation<bool> TrySkipPreviousAsync() => _session.TrySkipPreviousAsync();

        public IAsyncOperation<bool> TryStopAsync() => _session.TryStopAsync();

        public IAsyncOperation<bool> TryTogglePlayPauseAsync() => _session.TryTogglePlayPauseAsync();
        #endregion Wrapped Methods

        #region Event Handler Delegates
        /// <summary>
        /// Event handler delegate that bridges events between <see cref="GlobalSystemMediaTransportControlsSession.MediaPropertiesChanged"/>
        /// and <see cref="MediaPropertiesChanged"/>.
        /// If the sender instance sent with the event is not the same instance as the wrapped instance, we set the wrapped instance to the sender
        /// to ensure we're always referring to the currently active session for an app.
        /// </summary>
        /// <param name="sender">Instance of <see cref="GlobalSystemMediaTransportControlsSession"/> emitting the event.</param>
        /// <param name="args">Arguments sent with the event.</param>
        private void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            SetSession(sender);
            MediaPropertiesChangedInternal?.Invoke(this, args);
        }

        /// <summary>
        /// Event handler delegate that bridges events between <see cref="GlobalSystemMediaTransportControlsSession.PlaybackInfoChanged"/>
        /// and <see cref="PlaybackInfoChanged"/>.
        /// If the sender instance sent with the event is not the same instance as the wrapped instance, we set the wrapped instance to the sender
        /// to ensure we're always referring to the currently active session for an app.
        /// </summary>
        /// <param name="sender">Instance of <see cref="GlobalSystemMediaTransportControlsSession"/> emitting the event.</param>
        /// <param name="args">Arguments sent with the event.</param>
        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            SetSession(sender);
            PlaybackInfoChangedInternal?.Invoke(this, args);
        }

        /// <summary>
        /// Event handler delegate that bridges events between <see cref="GlobalSystemMediaTransportControlsSession.TimelinePropertiesChanged"/>
        /// and <see cref="TimelinePropertiesChanged"/>.
        /// If the sender instance sent with the event is not the same instance as the wrapped instance, we set the wrapped instance to the sender
        /// to ensure we're always referring to the currently active session for an app.
        /// </summary>
        /// <param name="sender">Instance of <see cref="GlobalSystemMediaTransportControlsSession"/> emitting the event.</param>
        /// <param name="args">Arguments sent with the event.</param>
        private void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            SetSession(sender);
            TimelinePropertiesChangedInternal?.Invoke(this, args);
        }
        #endregion Event Handler Delegates

        #region Helpers
        /// <summary>
        /// Helper calling to <see cref="GlobalSystemMediaTransportControlsSession.TryGetMediaPropertiesAsync"/> on the wrapped instance that instead returns
        /// a <see cref="Task{TResult}"/> so that we can use async/await, as async isn't supported on functions with a return type of <see cref="IAsyncOperation{TResult}"/>.
        /// </summary>
        /// <returns>The result of calling <see cref="GlobalSystemMediaTransportControlsSession.TryGetMediaPropertiesAsync"/> on the wrapped instance.</returns>
        private async Task<IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper> TryGetMediaPropertiesInternalAsync()
        {
            var mediaProperties = await _session.TryGetMediaPropertiesAsync();
            if (mediaProperties == null)
            {
                return null;
            }
            return new GlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper(mediaProperties);
        }

        /// <summary>
        /// Sets the wrapped session instance if the given session refers to a different instance.
        /// Internal event subscriptions are moved over to the new instance for the events that have subscribers.
        /// </summary>
        /// <remarks>
        /// Sessions are per app (or more specifically per SourceAppUserModelId), not per app instance, so the event sender may
        /// not refer to the same instance if there are multiple audio streams per app (such as web browsers).
        /// To account for this, we swap out our wrapped instance if the event sender does not refer to the same
        /// instance so that we are always holding the current instance for the app session.
        /// </remarks>
        /// <param name="newSession">Session instance to replace the wrapped instance with if it's not the same instance.</param>
        private void SetSession(GlobalSystemMediaTransportControlsSession newSession)
        {
            // No need to update if the sender is still the same instance as the one we're holding
            if (object.ReferenceEquals(_session, newSession))
            {
                return;
            }

            // It shouldn't be possible for us to receive an event from a session in a different app.
            // However, we should fail outright if it does happen as that could cause unexpected behavior
            // for the user since all handling for new app sessions is through the CurrentSessionChanged
            // event handler on the session manager.
            if (_session.SourceAppUserModelId != newSession.SourceAppUserModelId)
            {
                throw new InvalidOperationException("Uh-oh spaghettio!");
            }

            var oldSession = Interlocked.Exchange(ref _session, newSession);

            // Only swap our internal event subscriptions to the new session if we have any subscribers to our own events
            if (MediaPropertiesChangedInternal.HasSubscribers())
            {
                _session.MediaPropertiesChanged += OnMediaPropertiesChanged;
                oldSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            }

            if (PlaybackInfoChangedInternal.HasSubscribers())
            {
                _session.PlaybackInfoChanged += OnPlaybackInfoChanged;
                oldSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            }

            if (TimelinePropertiesChangedInternal.HasSubscribers())
            {
                _session.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
                oldSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
            }
        }
        #endregion Helpers

        #region Equality
        /// <inheritdoc/>
        /// <remarks>
        /// This is effectively a reference equals, mirroring the behavior of equality checks directly
        /// between instances of <see cref="GlobalSystemMediaTransportControlsSession"/>.
        /// <br/><br/>
        /// The <see cref="SourceAppUserModelId"/> is intentionally not used when determining equality
        /// as different sessions may have the same value if a previous instance is retained after the
        /// current session changes. This is because only one active session exists at a time in the
        /// <see cref="GlobalSystemMediaTransportControlsSessionManager"/> per <see cref="SourceAppUserModelId"/>
        /// even if the source app has multiple active audio streams (such as a web browser).
        /// </remarks>
        public bool Equals(IGlobalSystemMediaTransportControlsSessionWrapper other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return WrappedInstance.Equals(other.WrappedInstance);
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="Equals(IGlobalSystemMediaTransportControlsSessionWrapper)"/>
        public override bool Equals(object other)
        {
            return Equals(other as GlobalSystemMediaTransportControlsSessionWrapper);
        }

        public override int GetHashCode()
        {
            return WrappedInstance.GetHashCode();
        }

        public static bool Equals(GlobalSystemMediaTransportControlsSessionWrapper object1, GlobalSystemMediaTransportControlsSessionWrapper object2)
        {
            if (object.ReferenceEquals(object1, null))
            {
                return object.ReferenceEquals(object2, null);
            }

            return object1.Equals(object2);
        }

        public static bool operator ==(GlobalSystemMediaTransportControlsSessionWrapper object1, IGlobalSystemMediaTransportControlsSessionManagerWrapper object2) => Equals(object1, object2);

        public static bool operator !=(GlobalSystemMediaTransportControlsSessionWrapper object1, IGlobalSystemMediaTransportControlsSessionManagerWrapper object2) => !(object1 == object2);
        #endregion Equality
    }
}
