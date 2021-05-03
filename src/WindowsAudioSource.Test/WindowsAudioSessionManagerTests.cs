using AudioBand.AudioSource;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Control;
using WindowsAudioSource.Wrappers;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class WindowsAudioSessionManagerTests
    {
        private Mock<IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory> _mockSystemSessionManagerFactory;

        public WindowsAudioSessionManagerTests()
        {
            _mockSystemSessionManagerFactory = new Mock<IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory>();
        }

        [Fact]
        public void WindowsAudioSessionManager_Create()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var sessionManager = new WindowsAudioSessionManager(_mockSystemSessionManagerFactory.Object);
            Assert.NotNull(sessionManager);
        }
    }
}