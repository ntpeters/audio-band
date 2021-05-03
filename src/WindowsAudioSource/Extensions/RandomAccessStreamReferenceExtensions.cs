using AudioBand.AudioSource;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WindowsAudioSource.Extensions
{
    public static class RandomAccessStreamReferenceExtensions
    {
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
    }
}
