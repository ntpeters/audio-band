using System;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    public class GlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper : IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper
    {
        private GlobalSystemMediaTransportControlsSessionPlaybackControls _playbackControls;

        public GlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper(GlobalSystemMediaTransportControlsSessionPlaybackControls playbackControls)
        {
            _playbackControls = playbackControls ?? throw new ArgumentNullException(nameof(playbackControls));
        }

        public bool IsChannelDownEnabled => _playbackControls.IsChannelDownEnabled;

        public bool IsChannelUpEnabled => _playbackControls.IsChannelUpEnabled;

        public bool IsFastForwardEnabled => _playbackControls.IsFastForwardEnabled;

        public bool IsNextEnabled => _playbackControls.IsNextEnabled;

        public bool IsPauseEnabled => _playbackControls.IsPauseEnabled;

        public bool IsPlayEnabled => _playbackControls.IsPlayEnabled;

        public bool IsPlayPauseToggleEnabled => _playbackControls.IsPlayPauseToggleEnabled;

        public bool IsPlaybackPositionEnabled => _playbackControls.IsPlaybackPositionEnabled;

        public bool IsPlaybackRateEnabled => _playbackControls.IsPlaybackRateEnabled;

        public bool IsPreviousEnabled => _playbackControls.IsPreviousEnabled;

        public bool IsRecordEnabled => _playbackControls.IsRecordEnabled;

        public bool IsRepeatEnabled => _playbackControls.IsRepeatEnabled;

        public bool IsRewindEnabled => _playbackControls.IsRewindEnabled;

        public bool IsShuffleEnabled => _playbackControls.IsShuffleEnabled;

        public bool IsStopEnabled => _playbackControls.IsStopEnabled;
    }
}
