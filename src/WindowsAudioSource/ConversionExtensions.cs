using AudioBand.AudioSource;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage.Streams;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    public static class ConversionExtensions
    {
        public static RepeatMode ToRepeatMode(this MediaPlaybackAutoRepeatMode? repeatMode)
        {
            switch (repeatMode)
            {
                case null:  // Fallthrough intentional
                case MediaPlaybackAutoRepeatMode.None: return RepeatMode.Off;
                case MediaPlaybackAutoRepeatMode.Track: return RepeatMode.RepeatTrack;
                case MediaPlaybackAutoRepeatMode.List: return RepeatMode.RepeatContext;
                default: return RepeatMode.Off;
            }
        }

        public static MediaPlaybackAutoRepeatMode ToMediaPlaybackAutoRepeatMode(this RepeatMode repeatMode)
        {
            switch (repeatMode)
            {
                case RepeatMode.Off: return MediaPlaybackAutoRepeatMode.None;
                case RepeatMode.RepeatTrack: return MediaPlaybackAutoRepeatMode.Track;
                case RepeatMode.RepeatContext: return MediaPlaybackAutoRepeatMode.List;
                default: return MediaPlaybackAutoRepeatMode.None;
            }
        }

        public static async Task<TrackInfoChangedEventArgs> ToTrackInfoChangedEventArgsAsync(this IGlobalSystemMediaTransportControlsSessionMediaPropertiesWrapper mediaProperties, bool includeAlbumArt = true, IAudioSourceLogger logger = null)
        {
            // Only attempt converting album art if it was requested
            // TODO: Album art sometimes changes even when we choose not to update the track info. Is this being treated as a reference? Do we need to hold a copy?
            Image albumArt = null;
            if (includeAlbumArt)
            {
                if (mediaProperties.Thumbnail != null)
                {
                    albumArt = await mediaProperties.Thumbnail.ToImageAsync(logger);
                }
                else
                {
                    logger?.Debug("Unable to update album art: No thumbnail reference provided");
                }
            }
            else
            {
                logger?.Debug($"Skipping update of album art, since one was not requested");
            }

            return new TrackInfoChangedEventArgs
            {
                TrackName = mediaProperties.Title,
                Artist = mediaProperties.Artist,
                Album = mediaProperties.AlbumTitle,
                AlbumArt = albumArt
            };
        }

        public static async Task<Image> ToImageAsync(this IRandomAccessStreamReference randomAccessImageStreamRef, IAudioSourceLogger logger = null)
        {
            try
            {
                Image newImage = null;
                using (var randomAccessImageStream = await randomAccessImageStreamRef.OpenReadAsync())
                using (var imageStream = randomAccessImageStream.AsStreamForRead())
                {
                    newImage = Image.FromStream(imageStream);
                }
                return newImage;
            }
            catch (Exception e)
            {
                logger?.Error(e);
                return null;
            }
        }

        public static TGroupElement FirstInGroupOrDefault<TGroupKey, TGroupElement>(this IEnumerable<IGrouping<TGroupKey, TGroupElement>> groupedElements, TGroupKey key)
            where TGroupKey : struct
        {
            var keyGroup = groupedElements.FirstOrDefault(group => group.Key.Equals(key));
            if (keyGroup == null)
            {
                return default(TGroupElement);
            }
            return keyGroup.FirstOrDefault();
        }
    }
}
