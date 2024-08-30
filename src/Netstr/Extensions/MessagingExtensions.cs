using Microsoft.EntityFrameworkCore;
using Netstr.Messaging;
using Netstr.Messaging.Events.Handlers;
using Netstr.Messaging.Events.Handlers.Replaceable;
using Netstr.Messaging.Events.Validators;
using Netstr.Messaging.MessageHandlers;
using Netstr.Messaging.Subscriptions.Validators;
using Netstr.Messaging.WebSockets;
using Netstr.Middleware;

namespace Netstr.Extensions
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketAdapterFactory>();
            services.AddSingleton<IWebSocketAdapterCollection, WebSocketAdapterCollection>();

            // message
            services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
            services.AddSingleton<IMessageHandler, EventMessageHandler>();
            services.AddSingleton<IMessageHandler, SubscribeMessageHandler>();
            services.AddSingleton<IMessageHandler, UnsubscribeMessageHandler>();
            services.AddSingleton<IMessageHandler, AuthMessageHandler>();

            // event
            services.AddSingleton<IEventDispatcher, EventDispatcher>();
            services.AddSingleton<IEventHandler, DeleteEventHandler>();
            services.AddSingleton<IEventHandler, RegularEventHandler>();
            services.AddSingleton<IEventHandler, ReplaceableEventHandler>();
            services.AddSingleton<IEventHandler, EphemeralEventHandler>();
            services.AddSingleton<IEventHandler, AddressableEventHandler>();

            services.AddEventValidators();
            services.AddSubscriptionValidators();

            return services;
        }

        public static IServiceCollection AddEventValidators(this IServiceCollection services)
        {
            services.AddSingleton<IEventValidator, EventHashValidator>();
            services.AddSingleton<IEventValidator, EventSignatureValidator>();
            services.AddSingleton<IEventValidator, EventPowValidator>();
            services.AddSingleton<IEventValidator, EventCreatedAtValidator>();
            services.AddSingleton<IEventValidator, ProtectedEventValidator>();

            return services;
        }

        public static IServiceCollection AddSubscriptionValidators(this IServiceCollection services)
        {
            services.AddSingleton<ISubscriptionRequestValidator, SubscriptionLimitsValidator>();
            services.AddSingleton<ISubscriptionRequestValidator, AuthProtectedKindsValidator>();

            return services;
        }

        public static IApplicationBuilder AcceptWebSocketsConnections(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebSocketsMiddleware>();

            return app;
        }
    }
}
