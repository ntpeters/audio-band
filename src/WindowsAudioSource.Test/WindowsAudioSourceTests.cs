using AudioBand.AudioSource;
using Moq;
using System.Threading.Tasks;
using WindowsAudioSource.Wrappers;
using Xunit;

namespace WindowsAudioSource.Test
{
    // TODO: Write some tests, yo!
    // - Creation => DONE
    // - Name property => DONE
    // - Controls
    // - Playback info changed
    // - Timeline properties changed
    // - Media properties changed
    // - Session changed
    // - Sessions changed
    // - Music sessions only setting changed
    // - Session disallow list setting changed
    // - Activate
    // - Deacivate
    public class WindowsAudioSourceTests
    {
        private Mock<IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory> _mockSessionManagerFactory;
        private Mock<IApiInformationProvider> _mockApiInformationProvider;
        private Mock<IAudioSourceLogger> _mockLogger;
        private WindowsAudioSource _audioSource;

        public WindowsAudioSourceTests()
        {
            _mockSessionManagerFactory = new Mock<IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory>();

            _mockApiInformationProvider = new Mock<IApiInformationProvider>();
            _mockApiInformationProvider.Setup(mock => mock.IsApiContractPresent(It.IsAny<string>(), It.IsAny<ushort>()))
                .Returns(true);

            _mockLogger = new Mock<IAudioSourceLogger>();

            _audioSource = new WindowsAudioSource(_mockSessionManagerFactory.Object, _mockApiInformationProvider.Object);
            _audioSource.Logger = _mockLogger.Object;
        }

        #region Creation
        [Fact]
        public void WindowsAudioSource_Create()
        {
            Assert.NotNull(new WindowsAudioSource());
        }

        [Fact]
        public void WindowsAudioSource_CreateWithDependencies()
        {
            // Use separate mocks here with no setup, since we expect the constructor not to call any methods on them
            var mockSessionManagerFactory = new Mock<IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory>();
            var mockApiInformationProvider = new Mock<IApiInformationProvider>();

            Assert.NotNull(new WindowsAudioSource(mockSessionManagerFactory.Object, mockApiInformationProvider.Object));
        }
        #endregion Creation

        #region Name Property
        [Theory]
        [InlineData(true, "Windows")]
        [InlineData(false, "Windows (Not Supported)")]
        public void WindowsAudioSource_GetName(bool isApiContractPresent, string expectedName)
        {
            _mockApiInformationProvider.Setup(mock => mock.IsApiContractPresent(It.IsAny<string>(), It.IsAny<ushort>()))
                .Returns(isApiContractPresent);

            Assert.Equal(expectedName, _audioSource.Name);
            _mockApiInformationProvider.Verify(
                mock => mock.IsApiContractPresent(
                    It.Is<string>(contractName => contractName == "Windows.Foundation.UniversalApiContract"),
                    It.Is<ushort>(majorVersion => majorVersion == 7)),
                Times.Once);
        }
        #endregion Name Property

        // TODO: Activate tests:
        // - API contract not present => nothing happens
        // - Session manager set:
        //      - Session manager events subscribed
        //      - Session set (non-null):
        //          - Current session not allowed based on user settings => nothing happens
        //          - Current session events subscribed
        //          - CurrentSessionSource setting updated
        //          - Playback info updated:
        //              - IsPlayingChanged event raised
        //              - ShuffleChanged event raised
        //              - RepeatModeChanged event raised
        //              - CurrentSessionType setting updated
        //              - CurrentSessionCapabilities setting updated
        //          - Timeline properties updated:
        //              - TrackProgressChanged event raised
        //                  - IsPlaybackPositionEnabled => timeline position used
        //                  - !IsPlaybackPositionEnabled => reset to zero
        //          - Media properties updated:
        //              - TrackInfoChanged event raised
        //                  - IsPlaybackPositionEnabled => timeline end time used
        //                  - !IsPlaybackPositionEnabled => set to zero
        //      - Session set (null):
        //          - Nothing happens
        #region ActivateAsync
        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public async Task WindowsAudioSource_ActivateAsync(bool isApiContractPresent, int expectedEventAddInvocations)
        {
            Assert.False(true, "Implement Me!");
        }
        #endregion ActivateAsync

        // TODO: Deactivate tests:
        // - Session manager set to null:
        //      - Session manager events unsubscribed
        //      - Session set to null:
        //          - Current session events unsubscribed
        //          - CurrentSessionSource setting cleared
        //      - Playback info reset:
        //          - IsPlayingChanged event raised with value of false
        //          - ShuffleChanged event raised with value of false
        //          - RepeatModeChanged event raised with value of off
        //          - CurrentSessionType setting cleared
        //          - CurrentSessionCapabilities setting cleared
        //      - Timeline properties reset:
        //          - TrackProgressChanged event raised with value of zero
        //      - Media properties reset:
        //          - TrackInfoChanged event raised with empty values and null art
        #region DeactivateAsync
        [Fact]
        public async Task WindowsAudioSource_DeactivateAsyncBeforeActivateAsync()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public async Task WindowsAudioSource_DeactivateAsyncAfterActivateAsync()
        {
            Assert.False(true, "Implement Me!");
        }
        #endregion DeactivateAsync

        // TODO: Controls tests:
        // - Everything:
        //      - Non-null current session => successfully execute command
        //      - Null current session => Nothing
        // - Set playback progress:
        //      - IsPlaybackPositionEnabled => successfully execute command
        //      - !IsPlaybackPositionEnabled => raise TrackProgressChanged event with value of zero
        // - Set volume => nothing
        #region Controls
        [Fact]
        public async Task WindowsAudioSource_PlayTrackAsync()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public async Task WindowsAudioSource_PauseTrackAsync()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public async Task WindowsAudioSource_NextTrackAsync()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public async Task WindowsAudioSource_PreviousTrackAsync()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public async Task WindowsAudioSource_SetPlaybackProgressAsync()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public async Task WindowsAudioSource_SetRepeatModeAsync()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public async Task WindowsAudioSource_SetShuffleAsync()
        {
            Assert.False(true, "Implement Me!");
        }
        #endregion Controls

        // TODO: Settings tests:
        // - Music sessions only setting changed:
        //      - Enabled => see OnSessionsChanged test cases
        //      - Disabled => see OnCurrentSessionChanged test cases
        // - Session disallow list setting changed:
        //      - Non-empty => see OnSessionsChanged test cases
        //          - If any empty values/whitespace exist => they're removed and SettingChanged event raised
        //      - Empty => see OnCurrentSessionChanged test cases
        #region Settings
        [Fact]
        public void WindowsAudioSource_CurrentSessionSource()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public void WindowsAudioSource_CurrentSessionType()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public void WindowsAudioSource_SessionSourceDisallowList()
        {
            Assert.False(true, "Implement Me!");
        }
        #endregion Settings

        // TODO: Session manager state change tests:
        // - Current session changed:
        //      - Session set (non-null):
        //          - Current session not allowed based on user settings => nothing happens
        //          - Current session events subscribed
        //          - CurrentSessionSource setting updated
        //          - Playback info updated:
        //              - IsPlayingChanged event raised
        //              - ShuffleChanged event raised
        //              - RepeatModeChanged event raised
        //              - CurrentSessionType setting updated
        //              - CurrentSessionCapabilities setting updated
        //          - Timeline properties updated:
        //              - TrackProgressChanged event raised
        //                  - IsPlaybackPositionEnabled => timeline position used
        //                  - !IsPlaybackPositionEnabled => reset to zero
        //          - Media properties updated:
        //              - TrackInfoChanged event raised
        //                  - IsPlaybackPositionEnabled => timeline end time used
        //                  - !IsPlaybackPositionEnabled => set to zero
        //      - Session set (null):
        //              - Current session events unsubscribed
        //              - CurrentSessionSource setting cleared
        //          - Playback info reset:
        //              - IsPlayingChanged event raised with value of false
        //              - ShuffleChanged event raised with value of false
        //              - RepeatModeChanged event raised with value of off
        //              - CurrentSessionType setting cleared
        //              - CurrentSessionCapabilities setting cleared
        //          - Timeline properties reset:
        //              - TrackProgressChanged event raised with value of zero
        //          - Media properties reset:
        //              - TrackInfoChanged event raised with empty values and null art
        // - Sessions changed:
        //      - Neither music sessions only or session disallow list settings set
        //          - Nothing happens
        //      - Either music sessions only or session disallow list settings set
        //          - No valid session found:
        //              - Current session events unsubscribed
        //              - CurrentSessionSource setting cleared
        //              - Playback info reset:
        //                  - IsPlayingChanged event raised with value of false
        //                  - ShuffleChanged event raised with value of false
        //                  - RepeatModeChanged event raised with value of off
        //                  - CurrentSessionType setting cleared
        //                  - CurrentSessionCapabilities setting cleared
        //              - Timeline properties reset:
        //                  - TrackProgressChanged event raised with value of zero
        //              - Media properties reset:
        //                  - TrackInfoChanged event raised with empty values and null art
        //          - Valid session found:
        //              - Current session not allowed based on user settings => nothing happens
        //              - Current session events subscribed
        //              - CurrentSessionSource setting updated
        //              - Playback info updated:
        //                  - IsPlayingChanged event raised
        //                  - ShuffleChanged event raised
        //                  - RepeatModeChanged event raised
        //                  - CurrentSessionType setting updated
        //                  - CurrentSessionCapabilities setting updated
        //              - Timeline properties updated:
        //                  - TrackProgressChanged event raised
        //                      - IsPlaybackPositionEnabled => timeline position used
        //                      - !IsPlaybackPositionEnabled => reset to zero
        //              - Media properties updated:
        //                  - TrackInfoChanged event raised
        //                      - IsPlaybackPositionEnabled => timeline end time used
        //                      - !IsPlaybackPositionEnabled => set to zero
        #region Session Manager State
        [Fact]
        public void WindowsAudioSource_OnCurrentSessionChanged()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public void WindowsAudioSource_OnSessionsChanged()
        {
            Assert.False(true, "Implement Me!");
        }
        #endregion Session Manager State

        // TODO: Session state change tests:
        // - Playback info changed:
        //      - Is playing state
        //          - Changed => raise IsPlayingChanged
        //          - Not Changed => nothing
        //      - Shuffle state
        //          - Changed => raise ShuffleChanged
        //          - Not Changed => nothing
        //      - Repeat state
        //          - Changed => raise RepeatModeChanged
        //          - Not Changed => nothing
        //      - CurrentSessionType setting updated
        //      - CurrentSessionCapabilities setting updated
        //      - IsPlaybackPositionEnabled capability changed
        //          - Call OnTimelinePropertiesChanged and OnMediaPropertiesChanged
        // - Timeline properties changed:
        //      - Changed
        //          - IsPlaybackPositionEnabled => timeline position used
        //          - !IsPlaybackPositionEnabled => reset to zero
        //      - Not Changed => nothing
        //      - Changed
        //          - IsPlaybackPositionEnabled => timeline end time used
        //          - !IsPlaybackPositionEnabled => set to zero
        //      - Not Changed => nothing
        // - Media properties changed:
        //      - Raise TrackInfoChanged
        //          - IsPlaybackPositionEnabled => track length == timeline position
        //          - !IsPlaybackPositionEnabled => track length == zero
        #region Session State
        [Fact]
        public void WindowsAudioSource_OnPlaybackInfoChanged()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public void WindowsAudioSource_OnTimelinePropertiesChanged()
        {
            Assert.False(true, "Implement Me!");
        }

        [Fact]
        public void WindowsAudioSource_OnMediaPropertiesChanged()
        {
            Assert.False(true, "Implement Me!");
        }
        #endregion Session State
    }
}
