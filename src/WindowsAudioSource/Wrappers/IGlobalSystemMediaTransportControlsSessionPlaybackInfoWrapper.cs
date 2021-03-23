using Windows.Media;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    public interface IGlobalSystemMediaTransportControlsSessionPlaybackInfoWrapper
    {
        MediaPlaybackAutoRepeatMode? AutoRepeatMode { get; }

        IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper Controls { get; }

        bool? IsShuffleActive { get; }

        double? PlaybackRate { get; }

        GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackStatus { get; }

        MediaPlaybackType? PlaybackType { get; }
    }
}
