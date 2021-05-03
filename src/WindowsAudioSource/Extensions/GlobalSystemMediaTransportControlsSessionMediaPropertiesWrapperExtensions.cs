using AudioBand.AudioSource;
using System.Drawing;
using System.Threading.Tasks;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource.Extensions
{
    public static class GlobalSystemMediaTransportControlsSessionMediaPropertiesWrapperExtensions
    {
        public static async Task<TrackInfoChangedEventArgs> ToTrackInfoChangedEventArgsAsync(this IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper mediaProperties, IAudioSourceLogger logger = null)
        {
            // TODO: Album art sometimes changes even when we choose not to update the track info. Is this being treated as a reference? Do we need to hold a copy?
            Image albumArt = null;
            if (mediaProperties.Thumbnail != null)
            {
                albumArt = await mediaProperties.Thumbnail.ToImageAsync(logger);
            }
            else
            {
                logger?.Debug("Unable to update album art: No thumbnail reference provided");
            }

            return new TrackInfoChangedEventArgs
            {
                TrackName = mediaProperties.Title,
                Artist = mediaProperties.Artist,
                Album = mediaProperties.AlbumTitle,
                AlbumArt = albumArt
            };
        }
    }
}
