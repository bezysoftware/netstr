using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netstr.Data;
using Netstr.Messaging;
using Netstr.Messaging.Events.Handlers;
using Netstr.Messaging.Events.Handlers.Replaceable;
using Netstr.Messaging.Models;
using Netstr.Messaging.WebSockets;
using Netstr.Options;
using System.Net.WebSockets;
using System.Text.Json;

namespace Netstr.Tests.Events
{
    public class EventHandlersTests : IDisposable
    {
        private readonly SqliteConnection connection;
        private readonly Mock<IDbContextFactory<NetstrDbContext>> dbFactoryMock;
        private readonly Mock<WebSocket> ws;
        private readonly WebSocketAdapter adapter;
        private readonly WebSocketAdapterCollection clients;
        private readonly EventDispatcher dispatcher;

        public EventHandlersTests()
        {
            (this.connection, var context, var options) = TestDbContext.InitializeAndSeed();

            this.dbFactoryMock = new Mock<IDbContextFactory<NetstrDbContext>>();
            this.dbFactoryMock.Setup(x => x.CreateDbContext()).Returns(() => new TestDbContext(options));

            context.Dispose();

            this.ws = new Mock<WebSocket>();

            // receiver is a client with 2 registered subscriptions
            this.adapter = new WebSocketAdapter(
                Mock.Of<ILogger<WebSocketAdapter>>(),
                Mock.Of<IOptions<ConnectionOptions>>(),
                Mock.Of<IOptions<LimitsOptions>>(),
                Mock.Of<IOptions<AuthOptions>>(),
                Mock.Of<IMessageDispatcher>(),
                CancellationToken.None,
                this.ws.Object,
                Mock.Of<IHeaderDictionary>());

            this.clients = new WebSocketAdapterCollection();
            var handlers = new IEventHandler[]
            {
                new RegularEventHandler(Mock.Of<ILogger<RegularEventHandler>>(), this.clients, this.dbFactoryMock.Object),
                new EphemeralEventHandler(Mock.Of<ILogger<EphemeralEventHandler>>(), this.clients),
                new ReplaceableEventHandler(Mock.Of<ILogger<ReplaceableEventHandler>>(), this.clients, this.dbFactoryMock.Object),
                new AddressableEventHandler(Mock.Of<ILogger<ReplaceableEventHandler>>(), this.clients, this.dbFactoryMock.Object)
            };
            this.dispatcher = new EventDispatcher(Mock.Of<ILogger<EventDispatcher>>(), handlers);
        }

        public void Dispose()
        {
            this.connection.Dispose();
        }

        [Fact]
        public async Task MessageIsBroadcastTest()
        {
            var sender = Mock.Of<IWebSocketAdapter>();
            var receiver = this.adapter;

            receiver.AddSubscription("sub1", [new SubscriptionFilter { Ids = ["blah"] }]);
            receiver.AddSubscription("sub2", [new SubscriptionFilter { Ids = ["904559949fe0a7dcc43166545c765b4af823a63ef9f8177484596972478b662c"] }]);
            this.clients.Add(receiver);

            var e = new Event
            {
                Id = "904559949fe0a7dcc43166545c765b4af823a63ef9f8177484596972478b662c",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721741818),
                Kind = 1,
                Tags = [],
                Content = "Hello!",
                Signature = "33f42d22335842cd02372340feb6cd14fb5e438d49fe9f6bdecd5baa683b8dd8b4501da35026f4f29f03137f2766942d6795c491a83145b431ee0f3477039a5c"
            };

            // event should be broadcast to the receiver 'sub1' subscription, but not the other 'sub2'
            await this.dispatcher.DispatchEventAsync(sender, e);

            var expected = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Event, "sub2", e });
            var unexpected = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Event, "sub1", e });

            this.ws.Verify(x => x.SendAsync(expected, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());
            this.ws.Verify(x => x.SendAsync(unexpected, WebSocketMessageType.Text, true, CancellationToken.None), Times.Never());
        }

        [Fact]
        public async Task EphemeralEventHandlerTest()
        {
            var e = new Event
            {
                Id = "f0c7d21a937532e4f40f07f86bada09589c8c6b322c4ca985f4b13c9aa7813e7",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721741818),
                Kind = 20000,
                Tags = [],
                Content = "Hello!",
                Signature = "13ed2547682705b02dfb4444c9b263988b63d556d97636c6f811246321d679439243028f131f162cbcbc9f82e3af2145a9bb750c3801e16faaab6d5d09acbc3e"
            };

            await this.dispatcher.DispatchEventAsync(this.adapter, e);

            var expected = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e.Id, true, "" });
            this.ws.Verify(x => x.SendAsync(expected, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());

            // verify event not saved in DB
            using var db = this.dbFactoryMock.Object.CreateDbContext();

            db.Events
                .Include(x => x.Tags)
                .Count(x => x.EventId == e.Id)
                .Should()
                .Be(0);
        }

        [Fact]
        public async Task RegularEventHandlerTest()
        {
            var e = new Event
            {
                Id = "904559949fe0a7dcc43166545c765b4af823a63ef9f8177484596972478b662c",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721741818),
                Kind = 1,
                Tags = [],
                Content = "Hello!",
                Signature = "33f42d22335842cd02372340feb6cd14fb5e438d49fe9f6bdecd5baa683b8dd8b4501da35026f4f29f03137f2766942d6795c491a83145b431ee0f3477039a5c"
            };

            await this.dispatcher.DispatchEventAsync(this.adapter, e);

            // verify send OK
            var expected = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e.Id, true, "" });
            this.ws.Verify(x => x.SendAsync(expected, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());

            // verify event saved in DB
            using var db = this.dbFactoryMock.Object.CreateDbContext();

            db.Events
                .Include(x => x.Tags)
                .First(x => x.EventId == e.Id)
                .Should()
                .BeEquivalentTo(
                    e.ToEntity(DateTimeOffset.Now),
                    x => x.Excluding(ex => ex.FirstSeen).Excluding(ex => ex.Id));
        }

        [Fact]
        public async Task RegularEventHandlerDuplicateTest()
        {
            var e = new Event
            {
                Id = "904559949fe0a7dcc43166545c765b4af823a63ef9f8177484596972478b662c",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721741818),
                Kind = 1,
                Tags = [],
                Content = "Hello!",
                Signature = "33f42d22335842cd02372340feb6cd14fb5e438d49fe9f6bdecd5baa683b8dd8b4501da35026f4f29f03137f2766942d6795c491a83145b431ee0f3477039a5c"
            };

            await this.dispatcher.DispatchEventAsync(this.adapter, e);
            await this.dispatcher.DispatchEventAsync(this.adapter, e);

            // verify send OK duplicate
            var expected = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e.Id, true, Messages.DuplicateEvent });
            this.ws.Verify(x => x.SendAsync(expected, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());

            // verify event saved in DB
            using var db = this.dbFactoryMock.Object.CreateDbContext();

            db.Events
                .Include(x => x.Tags)
                .Count(x => x.EventId == e.Id)
                .Should()
                .Be(1);
        }

        [Fact]
        public async Task ReplaceableEventHandlerTest()
        {
            var e1 = new Event
            {
                Id = "399c0248a3410af1d7011b3799a5654c7eedfc9c38f561fb0e0c986738568fef",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721741818),
                Kind = 0,
                Tags = [],
                Content = "Hello!",
                Signature = "a56518b0f599a198fbdafa16f0ce69c4757e23dae8d9badfeab3ce44ad5ebc85e05a6e28492b3d3be99abc7e69799d877dfb2f1d5ff48198719be4b963c133e0"
            };

            var e2 = new Event
            {
                Id = "970a71261bc5c6eb773bbf432b0128ad2105b65a06b991ec4bfcdfda35e269f8",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721930239),
                Kind = 0,
                Tags = [],
                Content = "New Hello!",
                Signature = "4b332833caa0c1df89fd514326bc4570abfff5dc7d1551a099966d6da04332073fd3a4b1aae7b5de88d333c3b48b0b503e3dd40b286d8ee3f8d1babca64fe8f5"
            };

            var e3 = new Event
            {
                Id = "5eb1656abfb203ede9fd157c8bd8a36282b2364044ab351a7be3ef1b89e079b6",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1716659880),
                Kind = 0,
                Tags = [],
                Content = "Old Hello!",
                Signature = "99b2fb9089045f5ee9ad35812f8acab543a3d0c8977eea38c58219450da62fe225b915d5802bff6085405c35030982b200b1aa9c86e1c5e924bdc8c9fa6faf0d"
            };

            await this.dispatcher.DispatchEventAsync(this.adapter, e1);
            await this.dispatcher.DispatchEventAsync(this.adapter, e2);
            await this.dispatcher.DispatchEventAsync(this.adapter, e3);

            // verify send OK 2x, duplicate for the last one
            var expected1 = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e1.Id, true, "" });
            var expected2 = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e2.Id, true, "" });
            var expected3 = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e3.Id, true, Messages.DuplicateReplaceableEvent });
            
            this.ws.Verify(x => x.SendAsync(expected1, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());
            this.ws.Verify(x => x.SendAsync(expected2, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());
            this.ws.Verify(x => x.SendAsync(expected3, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());

            // verify original event is replaced by new one, but not old one, in DB
            using var db = this.dbFactoryMock.Object.CreateDbContext();

            db.Events.Count(x => x.EventId == e1.Id).Should().Be(0);
            db.Events.Count(x => x.EventId == e3.Id).Should().Be(0);
            db.Events.Single(x => x.EventId == e2.Id).EventContent.Should().Be(e2.Content);
        }

        [Fact]
        public async Task AddressableEventHandlerTest()
        {
            var e1 = new Event
            {
                Id = "27a7e752ee9c034cd7d5f83cb2a8510dd6339d51a09926f295d59e155cdd052e",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1716659880),
                Kind = 30000,
                Tags = [[ "d", "there is no sppon" ]],
                Content = "Hello!",
                Signature = "5f7f358a6fdd123174c2fb8ecd0090d419d993568839aa650b86d5aa3fe1a00ab8491b9acfd42b00dd7b34dbc50ad44a8cd6ed065899bfd563d852714c41049b"
            };

            var e2 = new Event
            {
                Id = "8163940856f9b0475f394e95924ee48e2393fa723c1a8409d3d6dd5f3f7f82ba",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1719331080),
                Kind = 30000,
                Tags = [[ "d", "there is no sppon" ]],
                Content = "New Hello!",
                Signature = "f07aa8692ca8dc1a1792b6a4406f6372f8d11befbb969343d01096d51f80f7a1ccd702fcde6f60bc0e072c1c1f7b5d38380a3afbff98c50a01daddba3a4ad60f"
            };

            var e3 = new Event
            {
                Id = "c67c9cd19c0679497884f1da64d1316fc3b31a8309dbf00d5a8248d7c6761356",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721923080),
                Kind = 30000,
                Tags = [[ "d", "fix the spoon" ]],
                Content = "New Hello!",
                Signature = "b20ebaf80860017873b70f8164d57a8552c1a633a7083490fe6859ca74edb9464da9842255b125099327fd8a1eea46afbea5e7776c547c760a6c8a33fe3c8157"
            };

            await this.dispatcher.DispatchEventAsync(this.adapter, e1);
            await this.dispatcher.DispatchEventAsync(this.adapter, e2);
            await this.dispatcher.DispatchEventAsync(this.adapter, e3);

            // verify send OK 3x
            var expected1 = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e1.Id, true, "" });
            var expected2 = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e2.Id, true, "" });
            var expected3 = JsonSerializer.SerializeToUtf8Bytes(new object[] { MessageType.Ok, e3.Id, true, "" });

            this.ws.Verify(x => x.SendAsync(expected1, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());
            this.ws.Verify(x => x.SendAsync(expected2, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());
            this.ws.Verify(x => x.SendAsync(expected3, WebSocketMessageType.Text, true, CancellationToken.None), Times.Once());

            // verify original event is replaced by second one, but last is added as brand new
            using var db = this.dbFactoryMock.Object.CreateDbContext();

            db.Events.Count(x => x.EventId == e1.Id).Should().Be(0);
            db.Events.Single(x => x.EventId == e2.Id).EventContent.Should().Be(e2.Content);
            db.Events.Single(x => x.EventId == e3.Id).EventContent.Should().Be(e3.Content);
        }
    }
}
