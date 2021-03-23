using System.Collections.Generic;
using Windows.Foundation;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    public interface IGlobalSystemMediaTransportControlsSessionManagerWrapper
    {
        IGlobalSystemMediaTransportControlsSessionWrapper GetCurrentSession();
        IReadOnlyList<IGlobalSystemMediaTransportControlsSessionWrapper> GetSessions();

        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, CurrentSessionChangedEventArgs> CurrentSessionChanged;
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, SessionsChangedEventArgs> SessionsChanged;
    }
}
