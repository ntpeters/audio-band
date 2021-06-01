using AudioBand.AudioSource;
using Moq;
using System;
using System.Threading.Tasks;
using Windows.Media;
using WindowsAudioSource.Wrappers;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class WindowsAudioSourceTests
    {
        private Mock<IApiInformationProvider> _mockApiInformationProvider;

        public WindowsAudioSourceTests()
        {
            _mockApiInformationProvider = new Mock<IApiInformationProvider>();
            _mockApiInformationProvider.Setup(mock => mock.IsApiContractPresent(It.IsAny<string>(), It.IsAny<ushort>()))
                .Returns(true);
        }

        [Fact]
        public void WindowsAudioSource_Create()
        {
            Assert.NotNull(new WindowsAudioSource());
        }

        [Fact]
        public void WindowsAudioSource_CreateWithSessionManager()
        {
            // TODO: Fix me
            Assert.False(true, "Fix me!");
        }

        [Theory]
        [InlineData(true, "Windows")]
        [InlineData(false, "Windows (Not Supported)")]
        public void WindowsAudioSource_GetName(bool isApiContractPresent, string expectedName)
        {
            _mockApiInformationProvider.Setup(mock => mock.IsApiContractPresent(It.IsAny<string>(), It.IsAny<ushort>()))
                .Returns(isApiContractPresent);

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object);

            Assert.Equal(expectedName, audioSource.Name);
            _mockApiInformationProvider.Verify(
                mock => mock.IsApiContractPresent(
                    It.Is<string>(contractName => contractName == "Windows.Foundation.UniversalApiContract"),
                    It.Is<ushort>(majorVersion => majorVersion == 7)),
                Times.Once);
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public async Task WindowsAudioSource_ActivateAsync(bool isApiContractPresent, int expectedEventAddInvocations)
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            _mockApiInformationProvider.Setup(mock => mock.IsApiContractPresent(It.IsAny<string>(), It.IsAny<ushort>()))
                .Returns(isApiContractPresent);

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.ActivateAsync();

            _mockApiInformationProvider.Verify(
                mock => mock.IsApiContractPresent(
                    It.Is<string>(contractName => contractName == "Windows.Foundation.UniversalApiContract"),
                    It.Is<ushort>(majorVersion => majorVersion == 7)),
                Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_DeactivateAsyncBeforeActivateAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.DeactivateAsync();
        }

        [Fact]
        public async Task WindowsAudioSource_DeactivateAsyncAfterActivateAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.ActivateAsync();
            await audioSource.DeactivateAsync();
        }

        [Fact]
        public async Task WindowsAudioSource_PlayTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.PlayTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TryPlayAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_PauseTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.PauseTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TryPauseAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_NextTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.NextTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TrySkipNextAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_PreviousTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.PreviousTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TrySkipPreviousAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_SetPlaybackProgressAsync()
        {
            var expectedProgress = TimeSpan.FromMinutes(5);
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.SetPlaybackProgressAsync(expectedProgress);

            mockSystemSessionManager.Verify(mock =>
                mock.TryChangePlaybackPositionAsync(It.Is<long>(actualTicks => actualTicks == expectedProgress.Ticks)), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_SetRepeatModeAsync()
        {
            var testRepeatMode = RepeatMode.RepeatTrack;
            var expectedRepeatMode = MediaPlaybackAutoRepeatMode.Track;
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.SetRepeatModeAsync(testRepeatMode);

            mockSystemSessionManager.Verify(mock =>
                mock.TryChangeAutoRepeatModeAsync(It.Is<MediaPlaybackAutoRepeatMode>(actualRepeatMode => actualRepeatMode == expectedRepeatMode)), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_SetShuffleAsync()
        {
            var testRepeatMode = RepeatMode.RepeatTrack;
            var expectedRepeatMode = MediaPlaybackAutoRepeatMode.Track;
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            await audioSource.SetRepeatModeAsync(testRepeatMode);

            mockSystemSessionManager.Verify(mock =>
                mock.TryChangeAutoRepeatModeAsync(It.Is<MediaPlaybackAutoRepeatMode>(actualRepeatMode => actualRepeatMode == expectedRepeatMode)), Times.Once);
        }

        [Fact]
        public void WindowsAudioSource_CurrentSessionSource()
        {
            var expectedSessionSource = "TestApp";
            var mockLogger = new Mock<IAudioSourceLogger>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            audioSource.CurrentSessionSource = expectedSessionSource;
            var actualSessionSource = audioSource.CurrentSessionSource;

            Assert.Equal(expectedSessionSource, actualSessionSource);
        }

        [Fact]
        public void WindowsAudioSource_CurrentSessionType()
        {
            var expectedSessionType = "Music";
            var mockLogger = new Mock<IAudioSourceLogger>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            audioSource.CurrentSessionType = expectedSessionType;
            var actualSessionType = audioSource.CurrentSessionType;

            Assert.Equal(expectedSessionType, actualSessionType);
        }

        [Fact]
        public void WindowsAudioSource_SessionSourceDisallowList()
        {
            var expectedSessionSourceDisallowList = "App1,App2,App3";
            var mockLogger = new Mock<IAudioSourceLogger>();

            // TODO: Fix me
            Assert.False(true, "Fix me!");
            var audioSource = new WindowsAudioSource();
            //var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            //{
            //    Logger = mockLogger.Object
            //};
            audioSource.SessionSourceDisallowList = expectedSessionSourceDisallowList;
            var actualSessionSourceDisallowList = audioSource.SessionSourceDisallowList;

            Assert.Equal(expectedSessionSourceDisallowList, actualSessionSourceDisallowList);
        }
    }
}
