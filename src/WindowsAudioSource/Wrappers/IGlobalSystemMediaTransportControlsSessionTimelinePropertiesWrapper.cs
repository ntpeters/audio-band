using System;

namespace WindowsAudioSource.Wrappers
{
    public interface IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper
    {
        TimeSpan EndTime { get; }

        DateTimeOffset LastUpdatedTime { get; }

        TimeSpan MaxSeekTime { get; }

        TimeSpan MinSeekTime { get; }

        TimeSpan Position { get; }

        TimeSpan StartTime { get; }
    }
}
