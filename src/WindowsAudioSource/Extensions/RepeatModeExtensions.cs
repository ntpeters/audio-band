using AudioBand.AudioSource;
using Windows.Media;

namespace WindowsAudioSource.Extensions
{
    public static class RepeatModeExtensions
    {
        public static RepeatMode ToRepeatMode(this MediaPlaybackAutoRepeatMode? repeatMode)
        {
            switch (repeatMode)
            {
                case null:  // Fallthrough
                case MediaPlaybackAutoRepeatMode.None: return RepeatMode.Off;
                case MediaPlaybackAutoRepeatMode.Track: return RepeatMode.RepeatTrack;
                case MediaPlaybackAutoRepeatMode.List: return RepeatMode.RepeatContext;
                default: return RepeatMode.Off;
            }
        }

        public static MediaPlaybackAutoRepeatMode ToMediaPlaybackAutoRepeatMode(this RepeatMode repeatMode)
        {
            switch (repeatMode)
            {
                case RepeatMode.Off: return MediaPlaybackAutoRepeatMode.None;
                case RepeatMode.RepeatTrack: return MediaPlaybackAutoRepeatMode.Track;
                case RepeatMode.RepeatContext: return MediaPlaybackAutoRepeatMode.List;
                default: return MediaPlaybackAutoRepeatMode.None;
            }
        }
    }
}
