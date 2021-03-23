using System.Collections.Generic;
using Windows.Media;
using Windows.Storage.Streams;

namespace WindowsAudioSource.Wrappers
{
    public interface IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper
    {
        string AlbumArtist { get; }

        string AlbumTitle { get; }

        int AlbumTrackCount { get; }

        string Artist { get; }

        IReadOnlyList<string> Genres { get; }

        MediaPlaybackType? PlaybackType { get; }

        string Subtitle { get; }

        IRandomAccessStreamReference Thumbnail { get; }

        string Title { get; }

        int TrackNumber { get; }
    }
}
