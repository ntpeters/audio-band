using System;
using System.Collections.Generic;
using Windows.Media;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace WindowsAudioSource.Wrappers
{
    public class GlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper : IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper
    {
        private GlobalSystemMediaTransportControlsSessionMediaProperties _mediaProperties;

        public GlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper(GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            _mediaProperties = mediaProperties ?? throw new ArgumentNullException(nameof(mediaProperties));
        }

        public string AlbumArtist => _mediaProperties.AlbumArtist;

        public string AlbumTitle => _mediaProperties.AlbumTitle;

        public int AlbumTrackCount => _mediaProperties.AlbumTrackCount;

        public string Artist => _mediaProperties.Artist;

        public IReadOnlyList<string> Genres => _mediaProperties.Genres;

        public MediaPlaybackType? PlaybackType => _mediaProperties.PlaybackType;

        public string Subtitle => _mediaProperties.Subtitle;

        public IRandomAccessStreamReference Thumbnail => _mediaProperties.Thumbnail;

        public string Title => _mediaProperties.Title;

        public int TrackNumber => _mediaProperties.TrackNumber;
    }
}
