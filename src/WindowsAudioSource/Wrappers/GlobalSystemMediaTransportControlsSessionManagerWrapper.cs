using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Media.Control;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    public class GlobalSystemMediaTransportControlsSessionManagerWrapper : IGlobalSystemMediaTransportControlsSessionManagerWrapper
    {
        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, CurrentSessionChangedEventArgs> CurrentSessionChanged;
        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, SessionsChangedEventArgs> SessionsChanged;

        private GlobalSystemMediaTransportControlsSessionManager _sessionManager;

        public GlobalSystemMediaTransportControlsSessionManagerWrapper(GlobalSystemMediaTransportControlsSessionManager sessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _sessionManager.CurrentSessionChanged += WrapCurrentSessionChanged;
            _sessionManager.SessionsChanged += WrapSessionsChanged;
        }

        ~GlobalSystemMediaTransportControlsSessionManagerWrapper()
        {
            _sessionManager.CurrentSessionChanged -= WrapCurrentSessionChanged;
            _sessionManager.SessionsChanged -= WrapSessionsChanged;
            _sessionManager = null;
        }

        public IGlobalSystemMediaTransportControlsSessionWrapper GetCurrentSession()
        {
            var currentSession = _sessionManager?.GetCurrentSession();
            if (currentSession == null)
            {
                return null;
            }

            return new GlobalSystemMediaTransportControlsSessionWrapper(currentSession);
        }

        public IReadOnlyList<IGlobalSystemMediaTransportControlsSessionWrapper> GetSessions() =>
            _sessionManager.GetSessions().Select(session => new GlobalSystemMediaTransportControlsSessionWrapper(session)).ToList();

        private void WrapCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            IGlobalSystemMediaTransportControlsSessionManagerWrapper wrappedSender = null;
            if (sender != null)
            {
                wrappedSender = new GlobalSystemMediaTransportControlsSessionManagerWrapper(sender);
            }
            CurrentSessionChanged?.Invoke(wrappedSender, args);
        }

        private void WrapSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            IGlobalSystemMediaTransportControlsSessionManagerWrapper wrappedSender = null;
            if (sender != null)
            {
                wrappedSender = new GlobalSystemMediaTransportControlsSessionManagerWrapper(sender);
            }
            SessionsChanged?.Invoke(wrappedSender, args);
        }
    }
}
