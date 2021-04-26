namespace WindowsAudioSource
{
    /// <summary>
    /// Names and descriptions of Windows audio source settings.
    /// </summary>
    public static class SettingConstants
    {
        public const string CurrentSessionSourceName = "Current Session Source";
        public const string CurrentSessionSourceDescription = "The AppUserModelId of the session currently being controlled.";

        public const string CurrentSessionTypeName = "Current Session Type";
        public const string CurrentSessionTypeDescription = "The type of the session currently being controlled, as reported by that session.";

        public const string SessionSourceDisallowListName = "Session Source Disallow List";
        public const string SessionSourceDisallowListDescription = "Comma separated list of AppUserModelIds to block from being controlled.";

        public const string CurrentSessionPlayPauseCapabilityName = "Current Session Play/Pause Capable";
        public const string CurrentSessionPlayPauseCapabilityDescription = "Whether the session currently being controlled supports play or pause actions.";

        public const string CurrentSessionNextPreviousCapabilityName = "Current Session Next/Previous Capable";
        public const string CurrentSessionNextPreviousCapabilityDescription = "Whether the session currently being controlled supports next or previous actions.";

        public const string CurrentSessionPlaybackPositionCapabilityName = "Current Session Playback Position Capable";
        public const string CurrentSessionPlaybackPositionCapabilityDescription = "Whether the session currently being controlled supports setting the playback position.";
    }
}
