using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netstr.Data;
using Netstr.Messaging;
using Netstr.Messaging.Events;
using Netstr.Messaging.MessageHandlers;
using Netstr.Options;

namespace Netstr.Tests
{
    public class MessageDispatcherTests
    {
        private readonly IMessageHandler[] handlers;
        private readonly MessageDispatcher dispatcher;

        public MessageDispatcherTests()
        {
            var eventDispatcher = new Mock<IEventDispatcher>();
            
            this.handlers =
            [
                new EventMessageHandler(Mock.Of<ILogger<EventMessageHandler>>(), eventDispatcher.Object, [], Mock.Of<IOptions<AuthOptions>>()),
                new SubscribeMessageHandler(Mock.Of<IDbContextFactory<NetstrDbContext>>(), [], Mock.Of<IOptions<LimitsOptions>>(), Mock.Of<IOptions<AuthOptions>>()),
                new UnsubscribeMessageHandler(),
            ];

            this.dispatcher = new MessageDispatcher(Mock.Of<ILogger<MessageDispatcher>>(), this.handlers);
        }

        [Theory]
        [InlineData("EVENT", 0)]
        [InlineData("REQ", 1)]
        [InlineData("CLOSE", 2)]
        public void EventMessageHandlerTest(string messageType, int handlerIndex)
        {
            var message = $"[\"{messageType}\", {{}}]";

            var (handler, _) = this.dispatcher.FindHandler(message);

            handler.Should().Be(this.handlers[handlerIndex]);
        }

        [Fact]
        public void UnknownEventTest()
        {
            var message = $"[\"UNKNOWN\", {{}}]";

            Assert.Throws<MessageProcessingException>(() => this.dispatcher.FindHandler(message));
        }
    }
}