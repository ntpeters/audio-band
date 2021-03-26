using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Media.Control;

namespace WindowsAudioSource.Wrappers
{
    /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionManager"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="GlobalSystemMediaTransportControlsSessionManager"/> to support mocking.
    /// </remarks>
    public interface IGlobalSystemMediaTransportControlsSessionManagerWrapper : IWrapper<GlobalSystemMediaTransportControlsSessionManager>, IEquatable<IGlobalSystemMediaTransportControlsSessionManagerWrapper>
    {
        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionManager.CurrentSessionChanged"/>
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, CurrentSessionChangedEventArgs> CurrentSessionChanged;

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionManager.SessionsChanged"/>
        event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, SessionsChangedEventArgs> SessionsChanged;

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionManager.GetCurrentSession"/>
        /// <remarks>
        /// Return value is wrapped as an <see cref="IGlobalSystemMediaTransportControlsSessionWrapper"/>.
        /// </remarks>
        IGlobalSystemMediaTransportControlsSessionWrapper GetCurrentSession();

        /// <inheritdoc cref="GlobalSystemMediaTransportControlsSessionManager.GetSessions"/>
        /// <remarks>
        /// Returned list elements are each wrapped as an <see cref="IGlobalSystemMediaTransportControlsSessionWrapper"/>.
        /// </remarks>
        IReadOnlyList<IGlobalSystemMediaTransportControlsSessionWrapper> GetSessions();
    }
}
