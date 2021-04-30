using System.Collections.Generic;
using Windows.Media;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="GlobalSystemMediaTransportControlsSessionMediaProperties"/> to support mocking.
    /// </remarks>
    public interface IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper : IInstanceWrapper<GlobalSystemMediaTransportControlsSessionMediaProperties>
    {
        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.AlbumArtist"/>
        string AlbumArtist { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.AlbumTitle"/>
        string AlbumTitle { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.AlbumTrackCount"/>
        int AlbumTrackCount { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.Artist"/>
        string Artist { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.Genres"/>
        IReadOnlyList<string> Genres { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.PlaybackType"/>
        MediaPlaybackType? PlaybackType { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.Subtitle"/>
        string Subtitle { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.Thumbnail"/>
        IRandomAccessStreamReference Thumbnail { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.Title"/>
        string Title { get; }

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionMediaProperties.TrackNumber"/>
        int TrackNumber { get; }
    }
}
