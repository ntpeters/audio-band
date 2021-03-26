using System;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper"/>
    public class GlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper : IGlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper
    {
        public GlobalSystemMediaTransportControlsSessionTimelineProperties WrappedInstance => _timelineProperties;

        private readonly GlobalSystemMediaTransportControlsSessionTimelineProperties _timelineProperties;

        public GlobalSystemMediaTransportControlsSessionTimelinePropertiesWrapper(GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties)
        {
            _timelineProperties = timelineProperties ?? throw new ArgumentNullException(nameof(timelineProperties));
        }

        public TimeSpan EndTime => _timelineProperties.EndTime;

        public DateTimeOffset LastUpdatedTime => _timelineProperties.LastUpdatedTime;

        public TimeSpan MaxSeekTime => _timelineProperties.MaxSeekTime;

        public TimeSpan MinSeekTime => _timelineProperties.MinSeekTime;

        public TimeSpan Position => _timelineProperties.Position;

        public TimeSpan StartTime => _timelineProperties.StartTime;
    }
}
