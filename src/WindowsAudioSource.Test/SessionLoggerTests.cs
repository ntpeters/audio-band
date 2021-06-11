using AudioBand.AudioSource;
using Moq;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class SessionLoggerTests
    {
        #region Setup
        /// <summary>
        /// Used to determine the expected value of the caller attribution in the log message.
        /// </summary>
        public enum ExpectedCallerType
        {
            /// <summary>
            /// Denotes that the default value provided by the <see cref="CallerMemberNameAttribute"/> should be used.
            /// </summary>
            Default,

            /// <summary>
            /// Denotes that the value explicitly passed into the log message for the caller parameter should be used.
            /// </summary>
            Custom,

            /// <summary>
            /// Denotes that no caller attribution should be used.
            /// </summary>
            Null
        }

        /// <summary>
        /// Test cases for string parameters.
        /// </summary>
        private static readonly string[] StringTestCases = { null, string.Empty, "    ", "Hello there!" };
        #endregion Setup

        #region Constructor Tests
        [Fact]
        public void SessionLogger_CreateWithNonNullParameters_Success()
        {
            Assert.NotNull(new SessionLogger(() => null, () => null));
        }

        [Fact]
        public void SessionLogger_CreateWithNullAudioSourceLoggerGetter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SessionLogger(null, () => null));
        }

        [Fact]
        public void SessionLogger_CreateWithNullCurrentSessionNameGetter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SessionLogger(() => null, null));
        }
        #endregion Constructor Tests

        #region Log Debug Tests
        [Theory]
        [MemberData(nameof(LogStringTestCases))]
        public void SessionLogger_DebugString(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, string messageToLog)
        {
            var expectedCallerName = SelectExpectedCaller(expectedCallerType);
            var expectedMessage = ComposeExpectedLogMessage(messageToLog, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Debug(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Debug(messageToLog);
            }
            else
            {
                logger.Debug(messageToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Debug(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }

        [Theory]
        [MemberData(nameof(LogObjectTestCases))]
        public void SessionLogger_DebugObject(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, object objectToLog, string expectedBaseMessage)
        {
            var expectedCallerName = SelectExpectedCaller(expectedCallerType);
            var expectedMessage = ComposeExpectedLogMessage(expectedBaseMessage, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Debug(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Debug(objectToLog);
            }
            else
            {
                logger.Debug(objectToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Debug(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }
        #endregion Log Debug Tests


        #region Log Info Tests
        [Theory]
        [MemberData(nameof(LogStringTestCases))]
        public void SessionLogger_InfoString(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, string messageToLog)
        {
#if DEBUG
            // The caller is only included at this log level in debug builds
            string expectedCallerName = SelectExpectedCaller(expectedCallerType);
#else
            string expectedCallerName = null;
#endif
            var expectedMessage = ComposeExpectedLogMessage(messageToLog, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Info(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Info(messageToLog);
            }
            else
            {
                logger.Info(messageToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Info(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }

        [Theory]
        [MemberData(nameof(LogObjectTestCases))]
        public void SessionLogger_InfoObject(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, object objectToLog, string expectedBaseMessage)
        {
#if DEBUG
            // The caller is only included at this log level in debug builds
            string expectedCallerName = SelectExpectedCaller(expectedCallerType);
#else
            string expectedCallerName = null;
#endif
            var expectedMessage = ComposeExpectedLogMessage(expectedBaseMessage, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Info(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Info(objectToLog);
            }
            else
            {
                logger.Info(objectToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Info(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }
        #endregion Log Info Tests

        #region Log Warn Tests
        [Theory]
        [MemberData(nameof(LogStringTestCases))]
        public void SessionLogger_WarnString(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, string messageToLog)
        {
#if DEBUG
            // The caller is only included at this log level in debug builds
            string expectedCallerName = SelectExpectedCaller(expectedCallerType);
#else
            string expectedCallerName = null;
#endif
            var expectedMessage = ComposeExpectedLogMessage(messageToLog, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Warn(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Warn(messageToLog);
            }
            else
            {
                logger.Warn(messageToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Warn(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }

        [Theory]
        [MemberData(nameof(LogObjectTestCases))]
        public void SessionLogger_WarnObject(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, object objectToLog, string expectedBaseMessage)
        {
#if DEBUG
            // The caller is only included at this log level in debug builds
            string expectedCallerName = SelectExpectedCaller(expectedCallerType);
#else
            string expectedCallerName = null;
#endif
            var expectedMessage = ComposeExpectedLogMessage(expectedBaseMessage, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Warn(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Warn(objectToLog);
            }
            else
            {
                logger.Warn(objectToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Warn(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }
        #endregion Log Warn Tests

        #region Log Error Tests
        [Theory]
        [MemberData(nameof(LogStringTestCases))]
        public void SessionLogger_ErrorString(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, string messageToLog)
        {
#if DEBUG
            // The caller is only included at this log level in debug builds
            string expectedCallerName = SelectExpectedCaller(expectedCallerType);
#else
            string expectedCallerName = null;
#endif
            var expectedMessage = ComposeExpectedLogMessage(messageToLog, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Error(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Error(messageToLog);
            }
            else
            {
                logger.Error(messageToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Error(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }

        [Theory]
        [MemberData(nameof(LogObjectTestCases))]
        public void SessionLogger_ErrorObject(bool isBaseLoggerNull, ExpectedCallerType expectedCallerType, string expectedSessionName, object objectToLog, string expectedBaseMessage)
        {
#if DEBUG
            // The caller is only included at this log level in debug builds
            string expectedCallerName = SelectExpectedCaller(expectedCallerType);
#else
            string expectedCallerName = null;
#endif
            var expectedMessage = ComposeExpectedLogMessage(expectedBaseMessage, expectedSessionName, expectedCallerName);
            var mockAudioSourceLogger = new Mock<IAudioSourceLogger>();
            mockAudioSourceLogger.Setup(mock => mock.Error(It.IsAny<string>()));
            var logger = new SessionLogger(() => isBaseLoggerNull ? null : mockAudioSourceLogger.Object, () => expectedSessionName);

            if (expectedCallerType == ExpectedCallerType.Default)
            {
                logger.Error(objectToLog);
            }
            else
            {
                logger.Error(objectToLog, expectedCallerName);
            }

            var expectedInvocations = isBaseLoggerNull ? Times.Never() : Times.Once();
            mockAudioSourceLogger.Verify(mock => mock.Error(It.Is<string>(actualMessage => actualMessage == expectedMessage)),
                expectedInvocations);
        }
        #endregion Log Error Tests

        #region Helpers
        /// <summary>
        /// Generates test cases for the log methods that accept strings.
        /// </summary>
        /// <returns>
        /// A <see cref="TheoryData{T1, T2, T3, T4}"/> containing all possible combinations of the following test parameters:
        /// <list type="bullet">
        /// <item>
        /// <see langword="bool"/>: Whether the <see cref="IAudioSourceLogger"/> called by <see cref="SessionLogger"/> is null
        /// </item>
        /// <item>
        /// <see cref="ExpectedCallerType"/>: Whether the caller is expected to be the default value, a custom value, or null
        /// </item>
        /// <item>
        /// <see langword="string"/>: The session name as a non-empty string, an empty string, a whitespace string, or null
        /// </item>
        /// <item>
        /// <see langword="string"/>: The log message as  a non-empty string, an empty string, a whitespace string, or null
        /// </item>
        /// </list>
        /// </returns>
        public static TheoryData<bool, ExpectedCallerType, string, string> LogStringTestCases()
        {
            
            var data = new TheoryData<bool, ExpectedCallerType, string, string>();
            foreach (var expectedCallerType in Enum.GetValues(typeof(ExpectedCallerType)).Cast<ExpectedCallerType>())
            {
                foreach (var expectedSessionName in StringTestCases)
                {
                    foreach (var messageToLog in StringTestCases)
                    {
                        data.Add(true, expectedCallerType, expectedSessionName, messageToLog);
                        data.Add(false, expectedCallerType, expectedSessionName, messageToLog);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Generates test cases for the log methods that accept objects.
        /// </summary>
        /// <returns>
        /// A <see cref="TheoryData{T1, T2, T3, T4, T5}"/> containing all possible combinations of the following test parameters:
        /// <list type="bullet">
        /// <item>
        /// <see langword="bool"/>: Whether the <see cref="IAudioSourceLogger"/> called by <see cref="SessionLogger"/> is null
        /// </item>
        /// <item>
        /// <see cref="ExpectedCallerType"/>: Whether the caller is expected to be the default value, a custom value, or null
        /// </item>
        /// <item>
        /// <see langword="string"/>: The session name as a non-empty string, an empty string, a whitespace string, or null
        /// </item>
        /// <item>
        /// <see langword="object"/>: The log object as either non-null or null
        /// </item>
        /// <item>
        /// <see langword="string"/>: The string expected to be logged representing the logged object
        /// </item>
        /// </list>
        /// </returns>
        public static TheoryData<bool, ExpectedCallerType, string, object, string> LogObjectTestCases()
        {
            var objectToLog = new Exception("Uh-oh!");
            var expectedMessage = objectToLog.ToString();
            var data = new TheoryData<bool, ExpectedCallerType, string, object, string>();
            foreach (var expectedCallerType in Enum.GetValues(typeof(ExpectedCallerType)).Cast<ExpectedCallerType>())
            {
                foreach (var expectedSessionName in StringTestCases)
                {
                    data.Add(true, expectedCallerType, expectedSessionName, objectToLog, expectedMessage);
                    data.Add(false, expectedCallerType, expectedSessionName, objectToLog, expectedMessage);
                    data.Add(true, expectedCallerType, expectedSessionName, null, string.Empty);
                    data.Add(false, expectedCallerType, expectedSessionName, null, string.Empty);
                }
            }
            return data;
        }

        /// <summary>
        /// Composes the expected complete log message including the session source, call source, and message.
        /// </summary>
        /// <param name="expectedMessage">The message expected after the log source prefixes.</param>
        /// <param name="expectedSessionName">The session name expected as part of the session source attribution.</param>
        /// <param name="expectedCaller">The caller name expected as part of the call source attribution.</param>
        /// <returns>The complete expected log message containing the given components.</returns>
        private static string ComposeExpectedLogMessage(string expectedMessage, string expectedSessionName, string expectedCaller)
        {
            expectedSessionName = string.IsNullOrWhiteSpace(expectedSessionName) ? "null" : expectedSessionName;
            var expectedLogMessage = $"SessionSource({expectedSessionName})|";
            if (expectedCaller != null)
            {
                expectedLogMessage += $"CallSource({expectedCaller})|";
            }
            expectedLogMessage += $"{expectedMessage}";
            return expectedLogMessage;
        }

        /// <summary>
        /// Gets a value representing the caller expected in the call source attribution of a log message based on the given type.
        /// </summary>
        /// <param name="expectedCallerType">The type of the expected caller value.</param>
        /// <param name="defaultCaller">The value to use when <paramref name="expectedCallerType"/> is <see cref="ExpectedCallerType.Default"/>.</param>
        /// <returns>A value representing the expected caller specified by <paramref name="expectedCallerType"/>.</returns>
        private static string SelectExpectedCaller(ExpectedCallerType expectedCallerType, [CallerMemberName] string defaultCaller = null)
        {
            switch (expectedCallerType)
            {
                case ExpectedCallerType.Custom: return "CallMeMaybe";
                case ExpectedCallerType.Null: return null;
                case ExpectedCallerType.Default: // Fallthrough
                default: return defaultCaller;
            }
        }
        #endregion Helpers
    }
}
