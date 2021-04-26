using System.Threading.Tasks;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <summary>
    /// Factory that produces an instance of <see cref="IGlobalSystemMediaTransportControlsSessionManagerWrapper"/>.
    /// </summary>
    /// <remarks>
    /// This is a thin wrapper around the <see cref="GlobalSystemMediaTransportControlsSessionManager.RequestAsync"/> to support mocking, since that method is static.
    /// </remarks>
    public interface IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory
    {
        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionManager.RequestAsync"/>
        /// <remarks>
        /// Return value is wrapped as an <see cref="IGlobalSystemMediaTransportControlsSessionManagerWrapper"/>.
        /// </remarks>
        Task<IGlobalSystemMediaTransportControlsSessionManagerWrapper> GetInstanceAsync();
    }
}
