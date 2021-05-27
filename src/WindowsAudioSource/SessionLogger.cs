using AudioBand.AudioSource;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace WindowsAudioSource
{
    public class SessionLogger
    {
        private readonly Func<IAudioSourceLogger> _getBaseLogger;
        private readonly Func<string> _getCurrentSessionName;

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
                try
                {
                    return currentSessionNameGetter();
                }
                catch (Exception)
                {
                    return null;
                }
            };
        }

        public void Debug(string message, [CallerMemberName] string caller = null)
        {
            _getBaseLogger()?.Debug(ComposeMessageWithSessionAttribution(message, caller));
        }

        public void Debug(object value, [CallerMemberName] string caller = null)
        {
            Debug(value.ToString(), caller);
        }

        public void Error(string message)
        {
            _getBaseLogger()?.Error(ComposeMessageWithSessionAttribution(message));
        }

        public void Error(object value)
        {
            Error(value.ToString());
        }

        public void Info(string message)
        {
            _getBaseLogger()?.Info(ComposeMessageWithSessionAttribution(message));
        }

        public void Info(object value)
        {
            Info(value.ToString());
        }

        public void Warn(string message)
        {
            _getBaseLogger()?.Warn(ComposeMessageWithSessionAttribution(message));
        }

        public void Warn(object value)
        {
            Warn(value.ToString());
        }

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
