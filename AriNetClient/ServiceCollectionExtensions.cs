using AriNetClient.Abstracts;
using AriNetClient.Clients;
using AriNetClient.Configuration;
using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Clients;
using AriNetClient.WebSockets.Clients.Connection;
using AriNetClient.WebSockets.Clients.EventHandling;
using AriNetClient.WebSockets.Clients.Reconnection;
using AriNetClient.WebSockets.Configuration;
using AriNetClient.WebSockets.Events;
using AriNetClient.WebSockets.Services.EventHandlers;
using AriNetClient.WebSockets.Services.EventHandlers.Call;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AriNetClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWazoNet(this IServiceCollection services, Action<WazoOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(services);

            ArgumentNullException.ThrowIfNull(configureOptions);

            // تسجيل الإعدادات
            services.Configure(configureOptions);

            // تسجيل HttpClient
            services.AddHttpClient("WazoClient")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        (message, cert, chain, errors) => true // لأغراض التطوير فقط
                });

            // تسجيل العملاء
            services.TryAddScoped<IAuthClient, AuthClient>();
            services.TryAddScoped<IUserClient, UserClient>();
            services.TryAddScoped<ICallClient, CallClient>();

            // تسجيل العميل الرئيسي
            services.TryAddScoped<IWazoNetClient, WazoNetClient>();

            return services;
        }

        public static IServiceCollection AddWazoNet(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddWazoNet(options =>
                configuration.GetSection(WazoOptions.SectionName).Bind(options));
        }



        //// New Code For The WebSocket
        public static IServiceCollection AddWebSocketClient(this IServiceCollection services, Action<WebSocketOptions> configureOptions)
        {
            ArgumentNullException.ThrowIfNull(services);

            ArgumentNullException.ThrowIfNull(configureOptions);

            // تسجيل خيارات WebSocket
            services.Configure(configureOptions);

            // تسجيل الاستراتيجيات
            services.TryAddSingleton<IReconnectionStrategy>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<WebSocketOptions>>().Value;
                return new ExponentialBackoffReconnectionStrategy(options);
            });

            // تسجيل مدير الاتصال
            services.TryAddSingleton<IConnectionManager, WebSocketConnectionManager>();

            // تسجيل سجل المعالجات
            services.TryAddSingleton<IEventHandlerRegistry, EventHandlerRegistry>();

            // تسجيل موزع الأحداث
            services.TryAddSingleton<IEventDispatcher, EventDispatcher>();

            // تسجيل معالجات الأحداث الأساسية
            services.TryAddScoped<CallStartedEventHandler>();
            services.TryAddScoped<CallUpdatedEventHandler>();
            services.TryAddScoped<CallEndedEventHandler>();
            services.TryAddScoped<GlobalEventLogger>();

            // تسجيل العميل الرئيسي
            services.TryAddScoped<WebSocketClient>();

            return services;
        }

        /// <summary>
        /// تسجيل خدمات WebSocketClient مع التكوين من IConfiguration
        /// </summary>
        public static IServiceCollection AddWebSocketClient(this IServiceCollection services, IConfiguration configuration, string sectionName = "WebSocket")
        {
            return services.AddWebSocketClient(options =>
                configuration.GetSection(sectionName).Bind(options));
        }

        /// <summary>
        /// تسجيل خدمات WebSocketClient مع الخيارات الافتراضية
        /// </summary>
        public static IServiceCollection AddWebSocketClient(this IServiceCollection services)
        {
            return services.AddWebSocketClient(options =>
            {
                // يمكن تعيين القيم الافتراضية هنا
                options.ApplicationName = "ari-net-client";
                options.AutoReconnect = true;
                options.MaxReconnectionAttempts = 10;
                options.InitialReconnectDelayMs = 1000;
            });
        }

        /// <summary>
        /// تسجيل معالج حدث مخصص
        /// </summary>
        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : BaseEvent
            where THandler : class, IEventHandler<TEvent>
        {
            services.TryAddScoped<THandler>();

            // تسجيل المعالج في EventDispatcher عند بناء الخدمة
            services.AddScoped(sp =>
            {
                var dispatcher = sp.GetRequiredService<IEventDispatcher>();
                dispatcher.RegisterHandler<TEvent, THandler>();
                return dispatcher;
            });

            return services;
        }

        /// <summary>
        /// تسجيل معالج أحداث عام مخصص
        /// </summary>
        public static IServiceCollection AddGlobalEventHandler<THandler>(this IServiceCollection services)
            where THandler : class, IGlobalEventHandler
        {
            services.TryAddScoped<THandler>();

            services.AddScoped(sp =>
            {
                var dispatcher = sp.GetRequiredService<IEventDispatcher>();
                var handler = sp.GetRequiredService<THandler>();
                dispatcher.RegisterGlobalHandler(handler);
                return dispatcher;
            });

            return services;
        }
    }
}
