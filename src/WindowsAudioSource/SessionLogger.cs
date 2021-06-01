using AudioBand.AudioSource;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace WindowsAudioSource
{
    /// <summary>
    /// Logger that wraps calls to an instance of <see cref="IAudioSourceLogger"/> including attribution of the current session and caller.
    /// </summary>
    public class SessionLogger
    {
        private readonly Func<IAudioSourceLogger> _getBaseLogger;
        private readonly Func<string> _getCurrentSessionName;

        /// <summary>
        /// Instantiates a new <see cref="SessionLogger"/> with the provided getters for the
        /// <see cref="IAudioSourceLogger"/> being wrapped and the current session name.
        /// </summary>
        /// <param name="audioSourceLoggerGetter">Function to get the underlaying <see cref="IAudioSourceLogger"/> to wrap log calls for.</param>
        /// <param name="currentSessionNameGetter">Function to get the current session name.</param>
        public SessionLogger(Func<IAudioSourceLogger> audioSourceLoggerGetter, Func<string> currentSessionNameGetter)
        {
            _getBaseLogger = () =>
            {
                try
                {
                    return audioSourceLoggerGetter();
                }
                catch (Exception)
                {
                    return null;
                }
            };

            _getCurrentSessionName = () =>
            {
                string sessionName;
                try
                {
                    sessionName = currentSessionNameGetter();
                }
                catch (Exception)
                {
                    sessionName = null;
                }
                return sessionName ?? "null";
            };
        }

        /// <inheritdoc cref="IAudioSourceLogger.Debug(string)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method.
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Debug(string message, [CallerMemberName] string caller = null)
        {
            _getBaseLogger()?.Debug(ComposeMessageWithSessionAttribution(message, caller));
        }

        /// <inheritdoc cref="IAudioSourceLogger.Debug(object)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method.
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Debug(object value, [CallerMemberName] string caller = null)
        {
            Debug(value?.ToString(), caller);
        }

        /// <inheritdoc cref="IAudioSourceLogger.Error(string)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method (caller included in debug builds only).
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Error(string message, [CallerMemberName] string caller = null)
        {
#if !DEBUG
            // Only include caller at this log level in debug builds
            caller = null;
#endif
            _getBaseLogger()?.Error(ComposeMessageWithSessionAttribution(message, caller));
        }

        /// <inheritdoc cref="IAudioSourceLogger.Error(object)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method (caller included in debug builds only).
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Error(object value, [CallerMemberName] string caller = null)
        {
            Error(value?.ToString(), caller);
        }

        /// <inheritdoc cref="IAudioSourceLogger.Info(string)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method (caller included in debug builds only).
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Info(string message, [CallerMemberName] string caller = null)
        {
#if !DEBUG
            // Only include caller at this log level in debug builds
            caller = null;
#endif
            _getBaseLogger()?.Info(ComposeMessageWithSessionAttribution(message, caller));
        }

        /// <inheritdoc cref="IAudioSourceLogger.Info(object)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method (caller included in debug builds only).
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Info(object value, [CallerMemberName] string caller = null)
        {
            Info(value?.ToString(), caller);
        }

        /// <inheritdoc cref="IAudioSourceLogger.Warn(string)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method (caller included in debug builds only).
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Warn(string message, [CallerMemberName] string caller = null)
        {
#if !DEBUG
            // Only include caller at this log level in debug builds
            caller = null;
#endif
            _getBaseLogger()?.Warn(ComposeMessageWithSessionAttribution(message, caller));
        }

        /// <inheritdoc cref="IAudioSourceLogger.Warn(object)"/>
        /// <remarks>
        /// Includes attribution of the current session and calling method (caller included in debug builds only).
        /// </remarks>
        /// <param name="caller">The calling method.</param>
        public void Warn(object value, [CallerMemberName] string caller = null)
        {
            Warn(value?.ToString(), caller);
        }

        /// <summary>
        /// Composes a log message including a prefix identifying the current session and the original caller that invoked the log method, if provided.
        /// </summary>
        /// <remarks>
        /// The attribution prefix is in the same format of the the 'AudioSource()|' prefix included in logs emitted by <see cref="IAudioSourceLogger"/>.
        /// </remarks>
        /// <param name="message">Message to log.</param>
        /// <param name="originalCaller">The original calling method.</param>
        /// <returns></returns>
        private string ComposeMessageWithSessionAttribution(string message, string originalCaller = null)
        {
            var stringBuilder = new StringBuilder($"CurrentSession({_getCurrentSessionName()})|");
            if (originalCaller != null)
            {
                stringBuilder.Append($"{originalCaller}|");
            }
            stringBuilder.Append(message);
            return stringBuilder.ToString();
        }
    }
}
