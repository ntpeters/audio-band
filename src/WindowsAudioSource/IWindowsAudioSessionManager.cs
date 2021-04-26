using AudioBand.AudioSource;
using System;
using System.Threading.Tasks;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    public interface IWindowsAudioSessionManager
    {
        string CurrentSessionSource { get; set; }
        string CurrentSessionType { get; set; }
        string SessionSourceDisallowList { get; set; }
        string CurrentSessionPlayPauseCapability { get; set; }
        string CurrentSessionNextPreviousCapability { get; set; }
        string CurrentSessionPlaybackPositionCapability { get; set; }
        IGlobalSystemMediaTransportControlsSessionWrapper CurrentSession { get; }

        event EventHandler<SettingChangedEventArgs> SettingChanged;
        event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;
        event EventHandler<bool> IsPlayingChanged;
        event EventHandler<TimeSpan> TrackProgressChanged;
        event EventHandler<float> VolumeChanged;
        event EventHandler<bool> ShuffleChanged;
        event EventHandler<RepeatMode> RepeatModeChanged;

        Task InitializeAsync(IAudioSourceLogger logger);
        void Unintialize();
    }
}
