using Moq;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using WindowsAudioSource.Extensions;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class RandomAccessStreamReferenceExtensionsTests
    {
        private readonly string _testArtPath1 = Path.GetFullPath("Assets/test_art_1.png");
        private readonly string _testArtPath2 = Path.GetFullPath("Assets/test_art_2.png");
        private readonly string _testTextFilePath = Path.GetFullPath("Assets/not_an_image.txt");

        [Fact]
        public async Task ToImageAsync_WithValidImageStream_Success()
        {
            var imageFile = await StorageFile.GetFileFromPathAsync(_testArtPath1);
            var (actualImage, actualErrorMessage) = await imageFile.TryToImageAsync();

            using (actualImage)
            using (var expectedImage = Image.FromFile(_testArtPath1))
            {
                Assert.NotNull(actualImage);
                Assert.Null(actualErrorMessage);
                Assert.Equal(expectedImage.RawFormat, actualImage.RawFormat);
                var imageConverter = new ImageConverter();
                var expectedImageBytes = GetImageBytes(expectedImage, imageConverter);
                var actualImageBytes = GetImageBytes(actualImage, imageConverter);
                Assert.Equal(expectedImageBytes, actualImageBytes);
            }
        }

        /// <summary>
        /// Admittedly, this test is covering a *very* edge case that is unlikely to ever hit under unit testing conditions.
        /// The only time this particular issue would occur when creating the image via <see cref="Image.FromStream(Stream)"/>
        /// is if internally GDI+ decides to either defer decoding the image from the stream, or if it decided to release the
        /// image to re-decode later when needed.
        /// </summary>
        [Fact]
        public async Task ToImageAsync_WithStreamImageSwap_Success()
        {
            Image actualImage = null;
            string actualErrorMessage;
            using (var imageFileStream = File.Open(_testArtPath1, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var randomAccessImageStream = new InMemoryRandomAccessStream())
            using (var randomAccessImageOutputStream = randomAccessImageStream.GetOutputStreamAt(0))
            {
                // Populate a RandomAccessStream with the image data
                await RandomAccessStream.CopyAndCloseAsync(imageFileStream.AsInputStream(), randomAccessImageOutputStream);
                var randomAccessImageStreamRef = RandomAccessStreamReference.CreateFromStream(randomAccessImageStream);

                // Create the image from the stream
                (actualImage, actualErrorMessage) = await randomAccessImageStreamRef.TryToImageAsync();

                // Write another image to the stream
                using (var imageFileStream2 = File.Open(_testArtPath2, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var randomAccessImageOutputStream2 = randomAccessImageStream.GetOutputStreamAt(0))
                {
                    await RandomAccessStream.CopyAndCloseAsync(imageFileStream2.AsInputStream(), randomAccessImageOutputStream2);
                }
            }

            using (actualImage)
            using (var expectedImage = Image.FromFile(_testArtPath1))
            {
                Assert.NotNull(actualImage);
                Assert.Null(actualErrorMessage);
                Assert.Equal(expectedImage.RawFormat, actualImage.RawFormat);
                var imageConverter = new ImageConverter();
                var expectedImageBytes = GetImageBytes(expectedImage, imageConverter);
                var actualImageBytes = GetImageBytes(actualImage, imageConverter);
                Assert.Equal(expectedImageBytes, actualImageBytes);
            }
        }

        [Fact]
        public async Task ToImageAsync_StreamThrowsException_Fails()
        {
            var expectedException = new NotSupportedException("Oh no!");
            var mockImageStream = new Mock<IRandomAccessStreamReference>();
            mockImageStream.Setup(mock => mock.OpenReadAsync())
                .Throws(expectedException);

            var (actualImage, actualErrorMessage) = await mockImageStream.Object.TryToImageAsync();

            Assert.Null(actualImage);
            Assert.Equal(expectedException.Message, actualErrorMessage);
        }

        [Fact]
        public async Task ToImageAsync_WithNullStreamRef_Fails()
        {
            var expectedErrorMessage = "Stream reference is null";
            IRandomAccessStreamReference imageStream = null;
            var (actualImage, actualErrorMessage) = await imageStream.TryToImageAsync();

            Assert.Null(actualImage);
            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }

        [Fact]
        public async Task ToImageAsync_WithNonImageStream_Fails()
        {
            var expectedErrorMessage = $"Stream content type is not supported: 'text/plain'. Only image content types are supported.";
            var imageFile = await StorageFile.GetFileFromPathAsync(_testTextFilePath);
            var (actualImage, actualErrorMessage) = await imageFile.TryToImageAsync();

            Assert.Null(actualImage);
            Assert.Equal(expectedErrorMessage, actualErrorMessage);
        }

        /// <summary>
        /// Gets the raw bytes of the given image.
        /// </summary>
        /// <param name="image">Image to get the bytes of.</param>
        /// <param name="imageConverter">Optional image converter to use instead of creating a new instance.</param>
        /// <returns>A byte array representing the image.</returns>
        private static byte[] GetImageBytes(Image image, ImageConverter imageConverter = null)
        {
            if (image == null)
            {
                return null;
            }

            imageConverter = imageConverter ?? new ImageConverter();
            using (var imageCopy = new Bitmap(image))
            {
                return imageConverter.ConvertTo(imageCopy, typeof(byte[])) as byte[];
            }
        }
    }
}
