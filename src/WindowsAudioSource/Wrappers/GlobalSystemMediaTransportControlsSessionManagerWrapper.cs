using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.Media.Control;
using WindowsAudioSource.Extensions;
using WindowsAudioSource.Wrappers;

namespace WindowsAudioSource
{
    /// <inheritdoc cref="IGlobalSystemMediaTransportControlsSessionManagerWrapper"/>
    public class GlobalSystemMediaTransportControlsSessionManagerWrapper : IGlobalSystemMediaTransportControlsSessionManagerWrapper
    {
        #region Public Events
        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, CurrentSessionChangedEventArgs> CurrentSessionChanged
        {
            add
            {
                // Lazily subscribe to the session manager event once someone subscribes to this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                if (!CurrentSessionChangedInternal.HasSubscribers())
                {
                    _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                }
                CurrentSessionChangedInternal += value;
            }

            remove
            {
                // Lazily unsubscribe from the session manager event once the last subscriber unsubscribes from this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                CurrentSessionChangedInternal -= value;
                if (!CurrentSessionChangedInternal.HasSubscribers())
                {
                    _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
                }
            }
        }

        public event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, SessionsChangedEventArgs> SessionsChanged
        {
            add
            {
                // Lazily subscribe to the session manager event once someone subscribes to this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                if (!SessionsChangedInternal.HasSubscribers())
                {
                    _sessionManager.SessionsChanged += OnSessionsChanged;
                }
                SessionsChangedInternal += value;
            }

            remove
            {
                // Lazily unsubscribe from the session manager event once the last subscriber unsubscribes from this event.
                // This way we don't have to worry about doing anything special to cleanup our own event subscriptions here to prevent leaks.
                SessionsChangedInternal -= value;
                if (!SessionsChangedInternal.HasSubscribers())
                {
                    _sessionManager.SessionsChanged -= OnSessionsChanged;
                }
            }
        }
        #endregion Public Events

        #region Public Properties
        public GlobalSystemMediaTransportControlsSessionManager WrappedInstance => _sessionManager;
        #endregion Public Properties

        #region Internal Events
        // Internal events used to allow lazy subscribing to the session manager events only when needed
        private event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, CurrentSessionChangedEventArgs> CurrentSessionChangedInternal;
        private event TypedEventHandler<IGlobalSystemMediaTransportControlsSessionManagerWrapper, SessionsChangedEventArgs> SessionsChangedInternal;
        #endregion Internal Events

        #region Instance Variables
        private GlobalSystemMediaTransportControlsSessionManager _sessionManager;
        #endregion Instance Variables

        #region Constructors
        public GlobalSystemMediaTransportControlsSessionManagerWrapper(GlobalSystemMediaTransportControlsSessionManager sessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }
        #endregion Constructors

        #region Wrapped Methods
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
            _sessionManager.GetSessions()?.Select(session => new GlobalSystemMediaTransportControlsSessionWrapper(session)).ToList();
        #endregion Wrapped Methods

        #region Event Handler Delegates
        /// <summary>
        /// Event handler delegate that bridges events between <see cref="GlobalSystemMediaTransportControlsSessionManager.CurrentSessionChanged"/>
        /// and <see cref="CurrentSessionChanged"/>.
        /// If the sender instance sent with the event is not the same instance as the wrapped instance, we set the wrapped instance to the sender.
        /// </summary>
        /// <param name="sender">Instance of <see cref="GlobalSystemMediaTransportControlsSessionManager"/> emitting the event.</param>
        /// <param name="args">Arguments sent with the event.</param>
        private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            SetSessionManager(sender);
            CurrentSessionChangedInternal?.Invoke(this, args);
        }

        /// <summary>
        /// Event handler delegate that bridges events between <see cref="GlobalSystemMediaTransportControlsSessionManager.SessionsChanged"/>
        /// and <see cref="SessionsChanged"/>.
        /// If the sender instance sent with the event is not the same instance as the wrapped instance, we set the wrapped instance to the sender.
        /// </summary>
        /// <param name="sender">Instance of <see cref="GlobalSystemMediaTransportControlsSessionManager"/> emitting the event.</param>
        /// <param name="args">Arguments sent with the event.</param>
        private void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            SetSessionManager(sender);
            SessionsChangedInternal?.Invoke(this, args);
        }
        #endregion Event Handler Delegates

        #region Helpers
        /// <summary>
        /// Sets the wrapped session manager instance if the given session manager refers to a different instance.
        /// Internal event subscriptions are moved over to the new instance for the events that have subscribers.
        /// </summary>
        /// <param name="newSessionManager">Session manager instance to replace the wrapped instance with if it's not the same instance.</param>
        private void SetSessionManager(GlobalSystemMediaTransportControlsSessionManager newSessionManager)
        {
            // No need to update if the sender is still the same instance as the one we're holding
            if (object.ReferenceEquals(_sessionManager, newSessionManager))
            {
                return;
            }

            var oldSessionManager = Interlocked.Exchange(ref _sessionManager, newSessionManager);

            // Only swap our internal event subscriptions to the new session manager if we have any subscribers to our own events
            if (CurrentSessionChangedInternal.HasSubscribers())
            {
                _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                oldSessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
            }

            if (SessionsChangedInternal.HasSubscribers())
            {
                _sessionManager.SessionsChanged += OnSessionsChanged;
                oldSessionManager.SessionsChanged -= OnSessionsChanged;
            }
        }
        #endregion Helpers

        #region Equality
        /// <inheritdoc/>
        /// <remarks>
        /// This is effectively a reference equals, mirroring the behavior of equality checks directly
        /// between instances of <see cref="GlobalSystemMediaTransportControlsSessionManager"/>.
        /// </remarks>
        public bool Equals(IGlobalSystemMediaTransportControlsSessionManagerWrapper other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return WrappedInstance.Equals(other.WrappedInstance);
        }

        /// <inheritdoc/>
        /// <inheritdoc cref="Equals(IGlobalSystemMediaTransportControlsSessionManagerWrapper)"/>
        public override bool Equals(object other)
        {
            return Equals(other as GlobalSystemMediaTransportControlsSessionManagerWrapper);
        }

        public override int GetHashCode()
        {
            return WrappedInstance.GetHashCode();
        }

        public static bool Equals(GlobalSystemMediaTransportControlsSessionManagerWrapper object1, GlobalSystemMediaTransportControlsSessionManagerWrapper object2)
        {
            if (object.ReferenceEquals(object1, null))
            {
                return object.ReferenceEquals(object2, null);
            }

            return object1.Equals(object2);
        }

        public static bool operator ==(GlobalSystemMediaTransportControlsSessionManagerWrapper object1, IGlobalSystemMediaTransportControlsSessionManagerWrapper object2) => Equals(object1, object2);

        public static bool operator !=(GlobalSystemMediaTransportControlsSessionManagerWrapper object1, IGlobalSystemMediaTransportControlsSessionManagerWrapper object2) => !(object1 == object2);
        #endregion Equality
    }
}
