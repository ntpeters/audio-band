using System;
using Windows.Media;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper"/>
    public class GlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper : IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper
    {
        public GlobalSystemMediaTransportControlsSessionPlaybackInfo WrappedInstance => _playbackInfo;

        private readonly GlobalSystemMediaTransportControlsSessionPlaybackInfo _playbackInfo;

        public GlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper(GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
        {
            _playbackInfo = playbackInfo ?? throw new ArgumentNullException(nameof(playbackInfo));
        }

        public MediaPlaybackAutoRepeatMode? AutoRepeatMode => _playbackInfo.AutoRepeatMode;

        public IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper Controls
        {
            get
            {
                var controls = _playbackInfo.Controls;
                if (controls == null)
                {
                    return null;
                }

                return new GlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper(controls);
            }
        }

        public bool? IsShuffleActive => _playbackInfo.IsShuffleActive;

        public double? PlaybackRate => _playbackInfo.PlaybackRate;

        public GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackStatus => _playbackInfo.PlaybackStatus;

        public MediaPlaybackType? PlaybackType => _playbackInfo.PlaybackType;
    }
}
