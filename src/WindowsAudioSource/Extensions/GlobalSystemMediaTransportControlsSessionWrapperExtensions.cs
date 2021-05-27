using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource.Extensions
{
    public static class GlobalSystemMediaTransportControlsSessionWrapperExtensions
    {
        public static bool TryGetIsPlayPauseCapable(this IGlobalSystemMediaTransportControlsSessionWrapper session, out bool isCapable)
        {
            if (session.TryGetControls(out var controls))
            {
                isCapable = controls.IsPlayEnabled || controls.IsPauseEnabled || controls.IsPlayPauseToggleEnabled;
                return true;
            }

            isCapable = false;
            return false;
        }

        public static bool TryGetIsNextPreviousCapable(this IGlobalSystemMediaTransportControlsSessionWrapper session, out bool isCapable)
        {
            if (session.TryGetControls(out var controls))
            {
                isCapable = controls.IsNextEnabled || controls.IsPreviousEnabled;
                return true;
            }

            isCapable = false;
            return false;
        }

        public static bool TryGetIsPlaybackPositionCapable(this IGlobalSystemMediaTransportControlsSessionWrapper session, out bool isCapable)
        {
            if (session.TryGetControls(out var controls))
            {
                isCapable = controls.IsPlaybackPositionEnabled;
                return true;
            }

            isCapable = false;
            return false;
        }

        public static bool TryGetIsShuffleCapable(this IGlobalSystemMediaTransportControlsSessionWrapper session, out bool isCapable)
        {
            if (session.TryGetControls(out var controls))
            {
                isCapable = controls.IsShuffleEnabled;
                return true;
            }

            isCapable = false;
            return false;
        }

        public static bool TryGetIsRepeatCapable(this IGlobalSystemMediaTransportControlsSessionWrapper session, out bool isCapable)
        {
            if (session.TryGetControls(out var controls))
            {
                isCapable = controls.IsRepeatEnabled;
                return true;
            }

            isCapable = false;
            return false;
        }

        public static bool TryGetControls(this IGlobalSystemMediaTransportControlsSessionWrapper session, out IGlobalSystemMediaTransportControlsSessionPlaybackControlsWrapper controls)
        {
            controls = session?.GetPlaybackInfo()?.Controls;
            if (controls != null)
            {
                return true;
            }

            return false;
        }
    }
}
