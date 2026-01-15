using AriNetClient.WebSocket;
using AriNetClient.WebSocket.Clients.EventHandling;
using AriNetClient.WebSocket.Services.EventHandlers;
using AriNetClient.WebSockets.Clients;
using AriNetClient.WebSockets.Clients.Connection;
using AriNetClient.WebSockets.Clients.EventHandling;
using AriNetClient.WebSockets.Clients.Reconnection;
using AriNetClient.WebSockets.Configuration;
using AriNetClient.WebSockets.Services.EventHandlers;
using AriNetClient.WebSockets.Services.EventHandlers.Call;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace AriNetClient
{
    public static class ServiceCollectionExtensions
    {
        private static readonly ILogger? _logger;

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
            services.TryAddSingleton<IEventHandlerRegistry, AutoEventHandlerRegistry>();

            // تسجيل موزع الأحداث
            services.TryAddSingleton<IEventDispatcher, SmartEventDispatcher>();

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
            return services.AddWebSocketClient(configuration);
        }

        /// <summary>
        /// تسجيل خدمات WebSocketClient مع الخيارات الافتراضية
        /// </summary>
        public static IServiceCollection AddWebSocketClient(this IServiceCollection services, IConfiguration configuration)
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


        #region Handlers
        /// <summary>
        /// إضافة نظام معالجة الأحداث الذكي
        /// </summary>
        public static IServiceCollection AddSmartEventHandling(this IServiceCollection services)
        {
            // التسجيل كـ Singleton لأن السجل يجب أن يكون واحداً
            services.TryAddSingleton<IEventHandlerRegistry, AutoEventHandlerRegistry>();
            services.TryAddSingleton<IEventDispatcher, SmartEventDispatcher>();

            // تسجيل معالجات النظام الأساسية
            RegisterSystemEventHandlers(services);

            return services;
        }

        /// <summary>
        /// تسجيل معالج حدث واحد (الطريقة البسيطة)
        /// </summary>
        public static IServiceCollection AddEventHandler<THandler>(this IServiceCollection services) where THandler : class
        {
            // 1. تسجيل المعالج في DI
            services.TryAddScoped<THandler>();

            // 2. تسجيل المعالج في النظام عند بدء التشغيل
            services.AddSingleton<EventHandlerRegistrar<THandler>>();

            return services;
        }

        /// <summary>
        /// تسجيل جميع معالجات الأحداث في تجميع معين
        /// </summary>
        public static IServiceCollection AddAllEventHandlersFromAssembly(this IServiceCollection services, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();

            _logger.LogInformation("Scanning assembly {Assembly} for event handlers", assembly.GetName().Name);

            // البحث عن جميع المعالجات
            var handlerTypes = FindEventHandlerTypes(assembly);

            foreach (var handlerType in handlerTypes)
            {
                services.TryAddScoped(handlerType);
                services.AddSingleton(
                    typeof(EventHandlerRegistrar<>).MakeGenericType(handlerType));
            }

            _logger.LogInformation("Registered {Count} event handlers from assembly {Assembly}",
                handlerTypes.Count, assembly.GetName().Name);

            return services;
        }

        /// <summary>
        /// البحث عن أنواع معالجات الأحداث في تجميع
        /// </summary>
        private static List<Type> FindEventHandlerTypes(Assembly assembly)
        {
            var handlerTypes = new List<Type>();

            try
            {
                var types = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .ToList();

                foreach (var type in types)
                {
                    if (IsEventHandlerType(type))
                    {
                        handlerTypes.Add(type);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scan assembly for event handlers");
            }

            return handlerTypes;
        }

        /// <summary>
        /// التحقق مما إذا كان النوع معالج أحداث
        /// </summary>
        private static bool IsEventHandlerType(Type type)
        {
            // التحقق من الواجهات
            var interfaces = type.GetInterfaces();

            // IAutoEventHandler أو IGlobalEventHandler
            if (interfaces.Any(i => i == typeof(IAutoEventHandler) ||
                                  i == typeof(IGlobalEventHandler)))
            {
                return true;
            }

            // IEventHandler<T>
            if (interfaces.Any(i => i.IsGenericType &&
                                  i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            {
                return true;
            }

            // SmartEventHandler<T> أو FlexibleEventHandler
            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType.IsGenericType)
                {
                    var genericDef = baseType.GetGenericTypeDefinition();
                    if (genericDef == typeof(SmartEventHandler<>) ||
                        genericDef == typeof(FlexibleEventHandler))
                    {
                        return true;
                    }
                }
                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// تسجيل معالجات النظام الأساسية
        /// </summary>
        private static void RegisterSystemEventHandlers(IServiceCollection services)
        {
            // تسجيل معالجات الأحداث الأساسية للمكالمات
            services.TryAddScoped<CallStartedEventHandler>();
            services.TryAddScoped<CallUpdatedEventHandler>();
            services.TryAddScoped<CallEndedEventHandler>();

            // تسجيل معالج الأحداث العامة
            services.TryAddScoped<GlobalEventLogger>();

            _logger.LogDebug("Registered system event handlers");
        }

        #endregion

    }

    /// <summary>
    /// مسجل معالجات الأفات (يعمل عند بدء التشغيل)
    /// </summary>
    internal class EventHandlerRegistrar<THandler> where THandler : class
    {
        public EventHandlerRegistrar(IEventDispatcher dispatcher, IServiceProvider serviceProvider)
        {
            try
            {
                dispatcher.RegisterHandler<THandler>();

                var logger = serviceProvider.GetService<ILogger<EventHandlerRegistrar<THandler>>>();
                logger?.LogDebug("Auto-registered event handler: {HandlerType}", typeof(THandler).Name);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetService<ILogger<EventHandlerRegistrar<THandler>>>();
                logger?.LogError(ex, "Failed to auto-register event handler: {HandlerType}", typeof(THandler).Name);
            }
        }

    }
}

