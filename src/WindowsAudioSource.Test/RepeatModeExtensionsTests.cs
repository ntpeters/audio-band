using AudioBand.AudioSource;
using Windows.Media;
using WindowsAudioSource.Extensions;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class RepeatModeExtensionsTests
    {
        [Theory]
        [InlineData(null, RepeatMode.Off)]
        [InlineData(MediaPlaybackAutoRepeatMode.None, RepeatMode.Off)]
        [InlineData(MediaPlaybackAutoRepeatMode.Track, RepeatMode.RepeatTrack)]
        [InlineData(MediaPlaybackAutoRepeatMode.List, RepeatMode.RepeatContext)]
        [InlineData((MediaPlaybackAutoRepeatMode)(-1), RepeatMode.Off)]
        public void ToMediaRepeatMode(MediaPlaybackAutoRepeatMode? fromRepeatMode, RepeatMode expectedRepeatMode)
        {
            Assert.Equal(expectedRepeatMode, fromRepeatMode.ToRepeatMode());
        }

        [Theory]
        [InlineData(RepeatMode.Off, MediaPlaybackAutoRepeatMode.None)]
        [InlineData(RepeatMode.RepeatTrack, MediaPlaybackAutoRepeatMode.Track)]
        [InlineData(RepeatMode.RepeatContext, MediaPlaybackAutoRepeatMode.List)]
        [InlineData((RepeatMode)(-1), MediaPlaybackAutoRepeatMode.None)]
        public void ToMediaPlaybackAutoRepeatMode(RepeatMode fromRepeatMode, MediaPlaybackAutoRepeatMode expectedRepeatMode)
        {
            Assert.Equal(expectedRepeatMode, fromRepeatMode.ToMediaPlaybackAutoRepeatMode());
        }
    }
}
