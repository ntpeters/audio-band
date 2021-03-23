using System;
using Windows.Media;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    public class GlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper : IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper
    {
        private GlobalSystemMediaTransportControlsSessionPlaybackInfo _playbackInfo;

        public GlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper(GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
        {
            _playbackInfo = playbackInfo ?? throw new ArgumentNullException(nameof(playbackInfo));
        }

        public MediaPlaybackAutoRepeatMode? AutoRepeatMode => throw new System.NotImplementedException();

        public IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper Controls =>
            new GlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper(_playbackInfo.Controls);

        public bool? IsShuffleActive => _playbackInfo.IsShuffleActive;

        public double? PlaybackRate => _playbackInfo.PlaybackRate;

        public GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackStatus => _playbackInfo.PlaybackStatus;

        public MediaPlaybackType? PlaybackType => _playbackInfo.PlaybackType;
    }
}
