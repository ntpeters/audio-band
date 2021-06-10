using AudioBand.AudioSource;
using Windows.Media;

namespace WindowsAudioSource.Extensions
{
    public static class RepeatModeExtensions
    {
        /// <summary>
        /// Converts a <see cref="MediaPlaybackAutoRepeatMode"/> to the equivalent <see cref="RepeatMode"/>.
        /// </summary>
        /// <param name="repeatMode">Value to convert.</param>
        /// <returns>The equivalent <see cref="RepeatMode"/> value.</returns>
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

        /// <summary>
        /// Converts a <see cref="RepeatMode"/> to the equivalent <see cref="MediaPlaybackAutoRepeatMode"/>.
        /// </summary>
        /// <param name="repeatMode">Value to convert.</param>
        /// <returns>The equivalent <see cref="MediaPlaybackAutoRepeatMode"/> value.</returns>
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
