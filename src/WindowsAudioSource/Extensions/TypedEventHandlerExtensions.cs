using Windows.Foundation;

namespace WindowsAudioSource.Extensions
{
    public static class TypedEventHandlerExtensions
    {
        /// <summary>
        /// Checks if the event handler has any subscribers.
        /// </summary>
        /// <typeparam name="TSender">Type of the event sender.</typeparam>
        /// <typeparam name="TEventArgs">Type of the event arguments.</typeparam>
        /// <param name="eventHandler">Event handler to check for subscribers.</param>
        /// <returns>True if the event handler has any subscribers, false otherwise.</returns>
        public static bool HasSubscribers<TSender, TEventArgs>(this TypedEventHandler<TSender, TEventArgs> eventHandler) =>
            eventHandler?.GetInvocationList().Length > 0;
    }
}
