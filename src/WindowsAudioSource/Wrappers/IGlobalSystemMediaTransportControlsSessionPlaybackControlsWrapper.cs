using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="GlobalSystemMediaTransportControlsSessionPlaybackControls"/> to support mocking.
    /// </remarks>
    public interface IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper : IWrapper<GlobalSystemMediaTransportControlsSessionPlaybackControls>
    {
        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsChannelDownEnabled"/>
        bool IsChannelDownEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsChannelUpEnabled"/>
        bool IsChannelUpEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsFastForwardEnabled"/>
        bool IsFastForwardEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsNextEnabled"/>
        bool IsNextEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsPauseEnabled"/>
        bool IsPauseEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsPlayEnabled"/>
        bool IsPlayEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsPlayPauseToggleEnabled"/>
        bool IsPlayPauseToggleEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsPlaybackPositionEnabled"/>
        bool IsPlaybackPositionEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsPlaybackRateEnabled"/>
        bool IsPlaybackRateEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsPreviousEnabled"/>
        bool IsPreviousEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsRecordEnabled"/>
        bool IsRecordEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsRepeatEnabled"/>
        bool IsRepeatEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsRewindEnabled"/>
        bool IsRewindEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsShuffleEnabled"/>
        bool IsShuffleEnabled { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionPlaybackControls.IsStopEnabled"/>
        bool IsStopEnabled { get; }
    }
}
