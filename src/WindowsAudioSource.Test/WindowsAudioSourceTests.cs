using Xunit;

namespace WindowsAudioSource.Test
{
    public class WindowsAudioSourceTests
    {
        [Fact]
        public void WindowsAudioSource_Create()
        {
            Assert.NotNull(new WindowsAudioSource());
        }
    }
}
