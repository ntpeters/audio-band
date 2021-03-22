using Xunit;

namespace NowPlayingAudioSource.Test
{
    public class AudioSourceTests
    {
        [Fact]
        public void AudioSource_Create()
        {
            Assert.NotNull(new NowPlayingAudioSource.AudioSource());
        }
    }
}
