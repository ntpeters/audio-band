using System;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionTimelineProperties"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="GlobalSystemMediaTransportControlsSessionTimelineProperties"/> to support mocking.
    /// </remarks>
    public interface IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper : IInstanceWrapper<GlobalSystemMediaTransportControlsSessionTimelineProperties>
    {
        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionTimelineProperties.EndTime"/>
        TimeSpan EndTime { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionTimelineProperties.LastUpdatedTime"/>
        DateTimeOffset LastUpdatedTime { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionTimelineProperties.MaxSeekTime"/>
        TimeSpan MaxSeekTime { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionTimelineProperties.MinSeekTime"/>
        TimeSpan MinSeekTime { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionTimelineProperties.Position"/>
        TimeSpan Position { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionTimelineProperties.StartTime"/>
        TimeSpan StartTime { get; }
    }
}
