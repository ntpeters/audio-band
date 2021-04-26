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
        [Fact]
        public void WindowsAudioSource_Create()
        {
            Assert.NotNull(new WindowsAudioSource());
        }

        [Fact]
        public void WindowsAudioSource_CreateWithSessionManager()
        {
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            Assert.NotNull(new WindowsAudioSource(mockSessionManager.Object));

            mockSessionManager.Verify(mock => mock.InitializeAsync(It.IsAny<IAudioSourceLogger>()), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Never);

            mockSessionManager.Verify(mock => mock.Unintialize(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Never);
        }

        [Fact]
        public async Task WindowsAudioSource_ActivateAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.ActivateAsync();

            mockSessionManager.Verify(mock => mock.InitializeAsync(It.Is<IAudioSourceLogger>(logger => logger == mockLogger.Object)), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Once);

            mockSessionManager.Verify(mock => mock.Unintialize(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Never);
        }

        [Fact]
        public async Task WindowsAudioSource_DeactivateAsyncBeforeActivateAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.DeactivateAsync();

            mockSessionManager.Verify(mock => mock.InitializeAsync(It.IsAny<IAudioSourceLogger>()), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Never);
            mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Never);

            mockSessionManager.Verify(mock => mock.Unintialize(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_DeactivateAsyncAfterActivateAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
            {
                Logger = mockLogger.Object
            };
            await audioSource.ActivateAsync();
            await audioSource.DeactivateAsync();

            mockSessionManager.Verify(mock => mock.InitializeAsync(It.Is<IAudioSourceLogger>(logger => logger == mockLogger.Object)), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.SettingChanged += It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.TrackInfoChanged += It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.IsPlayingChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.TrackProgressChanged += It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.VolumeChanged += It.IsAny<EventHandler<float>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.ShuffleChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyAdd(mock => mock.RepeatModeChanged += It.IsAny<EventHandler<RepeatMode>>(), Times.Once);

            mockSessionManager.Verify(mock => mock.Unintialize(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.SettingChanged -= It.IsAny<EventHandler<SettingChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.TrackInfoChanged -= It.IsAny<EventHandler<TrackInfoChangedEventArgs>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.IsPlayingChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.TrackProgressChanged -= It.IsAny<EventHandler<TimeSpan>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.VolumeChanged -= It.IsAny<EventHandler<float>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.ShuffleChanged -= It.IsAny<EventHandler<bool>>(), Times.Once);
            mockSessionManager.VerifyRemove(mock => mock.RepeatModeChanged -= It.IsAny<EventHandler<RepeatMode>>(), Times.Once);
        }

        [Fact]
        public async Task WindowsAudioSource_PlayTrackAsync()
        {
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSystemSessionManager = new Mock<IGlobalSystemMediaTransportControlsSessionWrapper>();
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
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
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
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
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
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
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
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
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
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
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
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
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSession)
                .Returns(mockSystemSessionManager.Object);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
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
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSessionSource)
                .Returns(expectedSessionSource);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
            {
                Logger = mockLogger.Object
            };
            audioSource.CurrentSessionSource = expectedSessionSource;
            var actualSessionSource = audioSource.CurrentSessionSource;

            mockSessionManager.VerifySet(mock => mock.CurrentSessionSource = It.Is<string>(sessionSource => sessionSource == expectedSessionSource), Times.Once);
            mockSessionManager.VerifyGet(mock => mock.CurrentSessionSource, Times.Once);
            Assert.Equal(expectedSessionSource, actualSessionSource);
        }

        [Fact]
        public void WindowsAudioSource_CurrentSessionType()
        {
            var expectedSessionType = "Music";
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.CurrentSessionType)
                .Returns(expectedSessionType);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
            {
                Logger = mockLogger.Object
            };
            audioSource.CurrentSessionType = expectedSessionType;
            var actualSessionType = audioSource.CurrentSessionType;

            mockSessionManager.VerifySet(mock => mock.CurrentSessionType = It.Is<string>(sessionType => sessionType == expectedSessionType), Times.Once);
            mockSessionManager.VerifyGet(mock => mock.CurrentSessionType, Times.Once);
            Assert.Equal(expectedSessionType, actualSessionType);
        }

        [Fact]
        public void WindowsAudioSource_SessionSourceDisallowList()
        {
            var expectedSessionSourceDisallowList = "App1,App2,App3";
            var mockLogger = new Mock<IAudioSourceLogger>();
            var mockSessionManager = new Mock<IWindowsAudioSessionManager>();
            mockSessionManager.SetupGet(mock => mock.SessionSourceDisallowList)
                .Returns(expectedSessionSourceDisallowList);

            var audioSource = new WindowsAudioSource(mockSessionManager.Object)
            {
                Logger = mockLogger.Object
            };
            audioSource.SessionSourceDisallowList = expectedSessionSourceDisallowList;
            var actualSessionSourceDisallowList = audioSource.SessionSourceDisallowList;

            mockSessionManager.VerifySet(mock => mock.SessionSourceDisallowList = It.Is<string>(sessionSourceDisallowList => sessionSourceDisallowList == expectedSessionSourceDisallowList), Times.Once);
            mockSessionManager.VerifyGet(mock => mock.SessionSourceDisallowList, Times.Once);
            Assert.Equal(expectedSessionSourceDisallowList, actualSessionSourceDisallowList);
        }
    }
}
