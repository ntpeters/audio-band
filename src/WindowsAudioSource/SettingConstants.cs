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
        public const string SessionSourceDisallowListDescription =
            "[Experimental] Using this setting may result in unexpected behavior.\n" +
            "Comma separated list of AppUserModelIds to block from being controlled.\n" +
            "Entries in this list should come from the value(s) shown in the 'Current Session Source'\n" +
            "setting. Only use this setting if one or more apps are frequently being controlled that\n" +
            "you do not wish to be.";
            

        public const string CurrentSessionCapabilitiesName = "Current Session Capabilities";
        public const string CurrentSessionCapabilitiesDescription = "Whether the session currently being controlled supports each of the listed capabilities.";

        public const string MusicSessionsOnlyName = "Music Sessions Only";
        public const string MusicSessionsOnlyDescription =
            "[Experimental] Using this setting may result in unexpected behavior.\n" +
            "Whether to restrict the type of sessions controlled to only music sessions.\n" +
            "Only enable this setting if you are frequently encountering apps you do not wish\n" +
            "to control which report their type as something other than 'Music', as shown by the\n" +
            "'Current Session Type' setting.";
    }
}
