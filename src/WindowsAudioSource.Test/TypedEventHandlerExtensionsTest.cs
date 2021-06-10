using Windows.Foundation;
using WindowsAudioSource.Extensions;
using Xunit;

namespace WindowsAudioSource.Test
{
    public class TypedEventHandlerExtensionsTests
    {
        private event TypedEventHandler<TypedEventHandlerExtensionsTests, string> TestEvent;

        [Fact]
        public void HasSubscribers_WithNoSubscribersAdded_ReturnsFalse()
        {
            Assert.False(TestEvent.HasSubscribers());
        }

        [Fact]
        public void HasSubscribers_WithSubscribersAdded_ReturnsTrue()
        {
            TestEvent += OnTestEvent;
            Assert.True(TestEvent.HasSubscribers());
        }

        [Fact]
        public void HasSubscribers_WithAllSubscribersRemoved_ReturnsFalse()
        {
            TestEvent += OnTestEvent;
            TestEvent -= OnTestEvent;
            Assert.False(TestEvent.HasSubscribers());
        }

        // Dummy event handler for testing
        private void OnTestEvent(TypedEventHandlerExtensionsTests sender, string args)
        {
        }
    }
}
