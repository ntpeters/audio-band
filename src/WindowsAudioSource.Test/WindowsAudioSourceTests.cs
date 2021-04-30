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
        private Mock<IWindowsAudioSessionManager> _mockSessionManager;
        private Mock<IApiInformationProvider> _mockApiInformationProvider;


        public WindowsAudioSourceTests()
        {
            _mockSessionManager = new Mock<IWindowsAudioSessionManager>();
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
            Assert.NotNull(new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object));

            _mockSessionManager.Verify(mock => mock.InitializeAsync(It.IsAny<IAudioSourceLogger>()), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Never);

            _mockSessionManager.Verify(mock => mock.Unintialize(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Never);
        }

        [Theory]
        [InlineData(true, "Windows")]
        [InlineData(false, "Windows (Not Supported)")]
        public void WindowsAudioSource_GetName(bool isApiContractPresent, string expectedName)
        {
            _mockApiInformationProvider.Setup(mock => mock.IsApiContractPresent(It.IsAny<string>(), It.IsAny<ushort>()))
                .Returns(isApiContractPresent);
            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object);

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
            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.ActivateAsync();

            _mockSessionManager.Verify(mock => mock.InitializeAsync(It.Is<IAudioSourceLogger>(logger => logger == mockLogger.Object)), Times.Exactly(expectedEventAddInvocations));
            _mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Exactly(expectedEventAddInvocations));
            _mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Exactly(expectedEventAddInvocations));
            _mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Exactly(expectedEventAddInvocations));
            _mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Exactly(expectedEventAddInvocations));
            _mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Exactly(expectedEventAddInvocations));
            _mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Exactly(expectedEventAddInvocations));
            _mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Exactly(expectedEventAddInvocations));

            _mockSessionManager.Verify(mock => mock.Unintialize(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Never);

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
            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.DeactivateAsync();

            _mockSessionManager.Verify(mock => mock.InitializeAsync(It.IsAny<IAudioSourceLogger>()), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            _mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Never);

            _mockSessionManager.Verify(mock => mock.Unintialize(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_DeactivateAsyncAfterActivateAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.ActivateAsync();
            await audioSource.DeactivateAsync();

            _mockSessionManager.Verify(mock => mock.InitializeAsync(It.Is<IAudioSourceLogger>(logger => logger == mockLogger.Object)), Times.Once);
            _mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Once);
            _mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Once);
            _mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
            _mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
            _mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Once);
            _mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
            _mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Once);

            _mockSessionManager.Verify(mock => mock.Unintialize(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            _mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_PlayTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();
            _mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.PlayTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TryPlayAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_PauseTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();
            _mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.PauseTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TryPauseAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_NextTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();
            _mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.NextTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TrySkipNextAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_PreviousTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();
            _mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.PreviousTrackAsync();

            mockSystemSessionManager.Verify(mock => mock.TrySkipPreviousAsync(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_SetPlaybackProgressAsync()
        {
            var expectedProgress = TimeSpan.FromMinutes(5);
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();
            _mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
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
            _mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
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
            _mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.SetRepeatModeAsync(testRepeatMode);

            mockSystemSessionManager.Verify(mock =>
                mock.TryChangeAutoRepeatModeAsync(It.Is<MediaPlaybackAutoRepeatMode>(actualRepeatMode => actualRepeatMode == expectedRepeatMode)), Times.Once);
        }

        [Fact]
        public void WindowsAudioSource_CurrentSessionSource()
        {
            var expectedSessionSource = "TestApp";
            var mockLogger = new Mock<IAudioSourceLogger>();
            _mockSessionManager.SetupGet(mock => mock.CurrentSessionSource)
                .Returns(expectedSessionSource);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            audioSource.CurrentSessionSource = expectedSessionSource;
            var actualSessionSource = audioSource.CurrentSessionSource;

            _mockSessionManager.VerifySet(mock => mock.CurrentSessionSource = It.Is<string>(sessionSource => sessionSource == expectedSessionSource), Times.Once);
            _mockSessionManager.VerifyGet(mock => mock.CurrentSessionSource, Times.Once);
            Assert.Equal(expectedSessionSource, actualSessionSource);
        }

        [Fact]
        public void WindowsAudioSource_CurrentSessionType()
        {
            var expectedSessionType = "Music";
            var mockLogger = new Mock<IAudioSourceLogger>();
            _mockSessionManager.SetupGet(mock => mock.CurrentSessionType)
                .Returns(expectedSessionType);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            audioSource.CurrentSessionType = expectedSessionType;
            var actualSessionType = audioSource.CurrentSessionType;

            _mockSessionManager.VerifySet(mock => mock.CurrentSessionType = It.Is<string>(sessionType => sessionType == expectedSessionType), Times.Once);
            _mockSessionManager.VerifyGet(mock => mock.CurrentSessionType, Times.Once);
            Assert.Equal(expectedSessionType, actualSessionType);
        }

        [Fact]
        public void WindowsAudioSource_SessionSourceDisallowList()
        {
            var expectedSessionSourceDisallowList = "App1,App2,App3";
            var mockLogger = new Mock<IAudioSourceLogger>();
            _mockSessionManager.SetupGet(mock => mock.SessionSourceDisallowList)
                .Returns(expectedSessionSourceDisallowList);

            var audioSource = new WindowsAudioSource(_mockSessionManager.Object, _mockApiInformationProvider.Object)
            {
                Logger = mockLogger.Object
            };
            audioSource.SessionSourceDisallowList = expectedSessionSourceDisallowList;
            var actualSessionSourceDisallowList = audioSource.SessionSourceDisallowList;

            _mockSessionManager.VerifySet(mock => mock.SessionSourceDisallowList = It.Is<string>(sessionSourceDisallowList => sessionSourceDisallowList == expectedSessionSourceDisallowList), Times.Once);
            _mockSessionManager.VerifyGet(mock => mock.SessionSourceDisallowList, Times.Once);
            Assert.Equal(expectedSessionSourceDisallowList, actualSessionSourceDisallowList);
        }
    }
}
