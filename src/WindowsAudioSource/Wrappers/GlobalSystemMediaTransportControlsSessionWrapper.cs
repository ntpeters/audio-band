using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    public class GlobalSystemMediaTransportControlsSessionWrapper : IGlobalSystemMediaTransportControlsSessionWrapper
    {
        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, MediaPropertiesChangedEventArgs> MediaPropertiesChanged;
        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, PlaybackInfoChangedEventArgs> PlaybackInfoChanged;
        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, TimelinePropertiesChangedEventArgs> TimelinePropertiesChanged;

        public string SourceAppUserModelId => _session.SourceAppUserModelId;

        private GlobalSystemMediaTransportControlsSession _session;

        public GlobalSystemMediaTransportControlsSessionWrapper(GlobalSystemMediaTransportControlsSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _session.MediaPropertiesChanged += WrapMediaPropertiesChanged;
            _session.PlaybackInfoChanged += WrapPlaybackInfoChanged;
            _session.TimelinePropertiesChanged += WrapTimelinePropertiesChanged;
        }

        ~GlobalSystemMediaTransportControlsSessionWrapper()
        {
            _session.MediaPropertiesChanged -= WrapMediaPropertiesChanged;
            _session.PlaybackInfoChanged -= WrapPlaybackInfoChanged;
            _session.TimelinePropertiesChanged -= WrapTimelinePropertiesChanged;
            _session = null;
        }

        public IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper GetPlaybackInfo() =>
            new GlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper(_session.GetPlaybackInfo());

        public IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper GetTimelineProperties() =>
            new GlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper(_session.GetTimelineProperties());

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

        // TODO: Just convert all these to Task<T>?
        public IAsyncOperation<IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper> TryGetMediaPropertiesAsync()
        {
            // TODO: Clean this up
            var mediaProperties = _session.TryGetMediaPropertiesAsync().AsTask().GetAwaiter().GetResult();
            var wrappedMediaProperties = new GlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper(mediaProperties);
            return Task.FromResult<IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper>(wrappedMediaProperties).AsAsyncOperation();
        }

        public IAsyncOperation<bool> TryPauseAsync() => _session.TryPauseAsync();

        public IAsyncOperation<bool> TryPlayAsync() => _session.TryPlayAsync();

        public IAsyncOperation<bool> TryRecordAsync() => _session.TryRecordAsync();

        public IAsyncOperation<bool> TryRewindAsync() => _session.TryRewindAsync();

        public IAsyncOperation<bool> TrySkipNextAsync() => _session.TrySkipNextAsync();

        public IAsyncOperation<bool> TrySkipPreviousAsync() => _session.TrySkipPreviousAsync();

        public IAsyncOperation<bool> TryStopAsync() => _session.TryStopAsync();

        public IAsyncOperation<bool> TryTogglePlayPauseAsync() => _session.TryTogglePlayPauseAsync();

        private void WrapMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            IGlobalSystemMediaTransportControlsSessionWrapper wrappedSender = null;
            if (sender != null)
            {
                wrappedSender = new GlobalSystemMediaTransportControlsSessionWrapper(sender);
            }
            MediaPropertiesChanged?.Invoke(wrappedSender, args);
        }

        private void WrapPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            IGlobalSystemMediaTransportControlsSessionWrapper wrappedSender = null;
            if (sender != null)
            {
                wrappedSender = new GlobalSystemMediaTransportControlsSessionWrapper(sender);
            }
            PlaybackInfoChanged?.Invoke(wrappedSender, args);
        }

        private void WrapTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            IGlobalSystemMediaTransportControlsSessionWrapper wrappedSender = null;
            if (sender != null)
            {
                wrappedSender = new GlobalSystemMediaTransportControlsSessionWrapper(sender);
            }
            TimelinePropertiesChanged?.Invoke(wrappedSender, args);
        }
    }
}
