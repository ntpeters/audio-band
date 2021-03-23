using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Media;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    public interface IGlobalSystemMediaTransportControlsSessionWrapper
    {
        IAsyncOperation<IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper> TryGetMediaPropertiesAsync();

        IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper GetTimelineProperties();

        IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper GetPlaybackInfo();

        [RemoteAsync]
        IAsyncOperation<bool> TryPlayAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryPauseAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryStopAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryRecordAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryFastForwardAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryRewindAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TrySkipNextAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TrySkipPreviousAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryChangeChannelUpAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryChangeChannelDownAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryTogglePlayPauseAsync();

        [RemoteAsync]
        IAsyncOperation<bool> TryChangeAutoRepeatModeAsync(MediaPlaybackAutoRepeatMode requestedAutoRepeatMode);

        IAsyncOperation<bool> TryChangePlaybackRateAsync(double requestedPlaybackRate);

        [RemoteAsync]
        IAsyncOperation<bool> TryChangeShuffleActiveAsync(bool requestedShuffleState);

        [RemoteAsync]
        IAsyncOperation<bool> TryChangePlaybackPositionAsync(long requestedPlaybackPosition);


        string SourceAppUserModelId { get; }

        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, MediaPropertiesChangedEventArgs> MediaPropertiesChanged;
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, PlaybackInfoChangedEventArgs> PlaybackInfoChanged;
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, TimelinePropertiesChangedEventArgs> TimelinePropertiesChanged;
    }
}
