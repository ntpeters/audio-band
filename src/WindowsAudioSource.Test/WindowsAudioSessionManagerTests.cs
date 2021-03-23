using AudioBand.AudioSource;
using Moq;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsAudioSource.Wrappers;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class WindowsAudioSessionManagerTests
    {
        [Fact]
        public async Task WindowsAudioSessionManager_CreateInstance()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var sessionManager = await WindowsAudioSessionManager.CreateInstance(mockLogger.Object);
            Assert.NotNull(sessionManager);
        }

        [Fact]
        public void WindowsAudioSessionManager_Create()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionManagerWrapper>();
            Assert.NotNull(new WindowsAudioSessionManager(mockLogger.Object, mockSystemSessionManager.Object));
        }
    }
}
