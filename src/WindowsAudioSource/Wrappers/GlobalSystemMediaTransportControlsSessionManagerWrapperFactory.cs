using System;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    /// <inheritdoc cref="IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory"/>
    public class GlobalSystemMediaTransportControlsSessionManagerWrapperFactory : IGlobalSystemMediaTransportControlsSessionManagerWrapperFactory
    {
        public async Task<IGlobalSystemMediaTransportControlsSessionManagerWrapper> GetInstanceAsync()
        {
            var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            return new GlobalSystemMediaTransportControlsSessionManagerWrapper(sessionManager);
        }
    }
}
