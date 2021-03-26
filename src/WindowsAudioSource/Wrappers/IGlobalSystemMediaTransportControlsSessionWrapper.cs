using System;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="GlobalSystemMediaTransportControlsSession"/> to support mocking.
    /// </remarks>
    public interface IGlobalSystemMediaTransportControlsSessionWrapper : IWrapper<GlobalSystemMediaTransportControlsSession>, IEquatable<IGlobalSystemMediaTransportControlsSessionWrapper>
    {
        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.MediaPropertiesChanged"/>
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, MediaPropertiesChangedEventArgs> MediaPropertiesChanged;

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.PlaybackInfoChanged"/>
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, PlaybackInfoChangedEventArgs> PlaybackInfoChanged;

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TimelinePropertiesChanged"/>
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionWrapper, TimelinePropertiesChangedEventArgs> TimelinePropertiesChanged;

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.SourceAppUserModelId"/>
        string SourceAppUserModelId { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryGetMediaPropertiesAsync"/>
        /// <remarks>
        /// Return value is wrapped as an <see cref="IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper"/>.
        /// </remarks>
        IAsyncOperation<IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper> TryGetMediaPropertiesAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.GetTimelineProperties"/>
        /// <remarks>
        /// Return value is wrapped as an <see cref="IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper"/>.
        /// </remarks>
        IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper GetTimelineProperties();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.GetPlaybackInfo"/>
        /// <remarks>
        /// Return value is wrapped as an <see cref="IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper"/>.
        /// </remarks>
        IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper GetPlaybackInfo();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession."/>
        IAsyncOperation<bool> TryPlayAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryPauseAsync"/>
        IAsyncOperation<bool> TryPauseAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryStopAsync"/>
        IAsyncOperation<bool> TryStopAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryRecordAsync"/>
        IAsyncOperation<bool> TryRecordAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryFastForwardAsync"/>
        IAsyncOperation<bool> TryFastForwardAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryRewindAsync"/>
        IAsyncOperation<bool> TryRewindAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TrySkipNextAsync"/>
        IAsyncOperation<bool> TrySkipNextAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TrySkipPreviousAsync"/>
        IAsyncOperation<bool> TrySkipPreviousAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryChangeChannelUpAsync"/>
        IAsyncOperation<bool> TryChangeChannelUpAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryChangeChannelDownAsync"/>
        IAsyncOperation<bool> TryChangeChannelDownAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryTogglePlayPauseAsync"/>
        IAsyncOperation<bool> TryTogglePlayPauseAsync();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryChangeAutoRepeatModeAsync"/>
        IAsyncOperation<bool> TryChangeAutoRepeatModeAsync(MediaPlaybackAutoRepeatMode requestedAutoRepeatMode);

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryChangePlaybackRateAsync"/>
        IAsyncOperation<bool> TryChangePlaybackRateAsync(double requestedPlaybackRate);

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryChangeShuffleActiveAsync"/>
        IAsyncOperation<bool> TryChangeShuffleActiveAsync(bool requestedShuffleState);

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSession.TryChangePlaybackPositionAsync"/>
        IAsyncOperation<bool> TryChangePlaybackPositionAsync(long requestedPlaybackPosition);
    }
}
