using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WindowsAudioSource.Extensions
{
    public static class RandomAccessStreamReferenceExtensions
    {
        /// <summary>
        /// Attempts to create an image from the stream.
        /// </summary>
        /// <remarks>
        /// This function copies the data from the stream and attempts to create an image from that copy.
        /// This is needed to work around some edge-cases in the associated APIs rather than using <see cref="Image.FromStream(Stream)"/>.
        /// <br></br><br></br>
        /// The Windows APIs providing the image stream reference seem to reuse the stream, and creating an image from a stream is not guaranteed
        /// to copy the data from the stream as the stream must be left open for the lifetime of the image (see remarks on <see cref="Image.FromStream(Stream)"/>).
        /// These two behaviors combined make it possible for the album art to sometimes be loaded for a different session than
        /// the one that is currently being controlled.
        /// <br></br><br></br>
        /// This issue is potentially further compounded in certain scenarios (which have been seen transiently during manual testing):
        /// <br></br>
        /// 1. When an app contains multiple sessions, since the Windows APIs only expose a single session per unique AppUserModelId.
        /// <br></br>
        /// 2. When any user settings are enabled that restrict the current session making our session differ from what Windows considers to be the current session.
        /// </remarks>
        /// <param name="randomAccessImageStreamRef">The stream to create an image from.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yeilding a tuple containing a mutually exclusive image and error message.
        /// When image creation succeeds the error message will be null, and on failure the image will be null and the error message will be populated.
        /// </returns>
        public static async Task<(Image Image, string Error)> ToImageAsync(this IRandomAccessStreamReference randomAccessImageStreamRef)
        {
            if (randomAccessImageStreamRef == null)
            {
                return (null, "Stream reference is null");
            }

            try
            {
                using (var randomAccessImageStream = await randomAccessImageStreamRef.OpenReadAsync())
                {
                    if (!randomAccessImageStream.ContentType.StartsWith("image/", true, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        return (null, $"Stream content type is not supported: '{randomAccessImageStream.ContentType}'. Only image content types are supported.");
                    }

                    using (var imageStream = randomAccessImageStream.AsStreamForRead())
                    using (var memoryImageStream = new MemoryStream())
                    {
                        // Copy the stream
                        // While this may appear less optimal to just using `RandomAccessStream.ReadAsync`
                        // due to a double copy here, in manual testing to compare them this implementation
                        // consistently performed several milliseconds faster.
                        await imageStream.CopyToAsync(memoryImageStream);
                        var imageBytes = memoryImageStream.ToArray();

                        // Convert the stream contents to an image
                        var newImage = new ImageConverter().ConvertFrom(imageBytes) as Image;
                        return (newImage, null);
                    }
                }
            }
            catch (Exception e)
            {
                return (null, e.Message);
            }
        }
    }
}
