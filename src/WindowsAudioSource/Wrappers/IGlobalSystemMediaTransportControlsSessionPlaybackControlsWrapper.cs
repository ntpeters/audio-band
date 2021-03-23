namespace WindowsAudioSource.Wrappers
{
    public interface IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper
    {
        bool IsChannelDownEnabled { get; }

        bool IsChannelUpEnabled { get; }

        bool IsFastForwardEnabled { get; }

        bool IsNextEnabled { get; }

        bool IsPauseEnabled { get; }

        bool IsPlayEnabled { get; }

        bool IsPlayPauseToggleEnabled { get; }

        bool IsPlaybackPositionEnabled { get; }

        bool IsPlaybackRateEnabled { get; }

        bool IsPreviousEnabled { get; }

        bool IsRecordEnabled { get; }

        bool IsRepeatEnabled { get; }

        bool IsRewindEnabled { get; }

        bool IsShuffleEnabled { get; }

        bool IsStopEnabled { get; }
    }
}
