using Windows.Media;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo"/> to support mocking.
    /// </remarks>
    public interface IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper : IWrapper<GlobalSystemMediaTransportControlsSessionPlaybackInfo>
    {
        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo.AutoRepeatMode"/>
        MediaPlaybackAutoRepeatMode? AutoRepeatMode { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo.Controls"/>
        /// <remarks>
        /// Return value is wrapped as an <see cref="IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper"/>.
        /// </remarks>
        IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper Controls { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo.IsShuffleActive"/>
        bool? IsShuffleActive { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo.PlaybackRate"/>
        double? PlaybackRate { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo.PlaybackStatus"/>
        GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackStatus { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackInfo.PlaybackType"/>
        MediaPlaybackType? PlaybackType { get; }
    }
}
