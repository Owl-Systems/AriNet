using AriNetClient.WebSockets.NewFolder.Abstracts;
using AriNetClient.WebSockets.NewFolder.Adapters;
using AriNetClient.WebSockets.NewFolder.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AriNetClient.WebSockets.NewFolder.Providers
{
    /// <summary>
    /// مصنع لإنشاء عمليات اتصال بالمزودين
    /// مشابه لمصانع EF Core أو RabbitMQ
    /// </summary>
    public interface ITelephonyProviderFactory
    {
        /// <summary>
        /// إنشاء عميل اتصال للمزود المحدد
        /// </summary>
        Task<ITelephonyConnection> CreateConnectionAsync(
            TelephonyProvider provider,
            ProviderConfiguration configuration,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// الحصول على المحولات المسجلة لمزود معين
        /// </summary>
        IEnumerable<IServerEventAdapter> GetAdaptersForProvider(TelephonyProvider provider);

        /// <summary>
        /// تسجيل مزود جديد
        /// </summary>
        void RegisterProvider(
            TelephonyProvider provider,
            Type connectionType,
            Type adapterType);
    }

    /// <summary>
    /// واجهة اتصال المزود
    /// </summary>
    public interface ITelephonyConnection : IAsyncDisposable
    {
        TelephonyProvider Provider { get; }
        bool IsConnected { get; }

        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        event EventHandler<string> RawMessageReceived;
        Task SendAsync(string message, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// تنفيذ المصنع
    /// </summary>
    public class TelephonyProviderFactory : ITelephonyProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelephonyProviderFactory> _logger;

        // سجل المزودين المسجلين
        private readonly Dictionary<TelephonyProvider, (Type ConnectionType, Type AdapterType)>
            _providerRegistry = new();

        public TelephonyProviderFactory(
            IServiceProvider serviceProvider,
            ILogger<TelephonyProviderFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // التسجيل الافتراضي للمزودين
            RegisterDefaultProviders();
        }

        private void RegisterDefaultProviders()
        {
            // تسجيل Wazo
            RegisterProvider(
                TelephonyProvider.Wazo,
                typeof(WazoConnection),
                typeof(WazoEventAdapter));

            // يمكن تسجيل مزودين آخرين هنا
            // RegisterProvider(TelephonyProvider.Asterisk, ...);
            // RegisterProvider(TelephonyProvider.FreeSwitch, ...);
        }

        public void RegisterProvider(
            TelephonyProvider provider,
            Type connectionType,
            Type adapterType)
        {
            if (!typeof(ITelephonyConnection).IsAssignableFrom(connectionType))
                throw new ArgumentException($"Connection type must implement ITelephonyConnection", nameof(connectionType));

            if (!typeof(IServerEventAdapter).IsAssignableFrom(adapterType))
                throw new ArgumentException($"Adapter type must implement IServerEventAdapter", nameof(adapterType));

            _providerRegistry[provider] = (connectionType, adapterType);
            _logger.LogInformation("Registered provider: {Provider} with connection {Connection} and adapter {Adapter}",
                provider, connectionType.Name, adapterType.Name);
        }

        public async Task<ITelephonyConnection> CreateConnectionAsync(
            TelephonyProvider provider,
            ProviderConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            if (!_providerRegistry.TryGetValue(provider, out var providerInfo))
            {
                throw new InvalidOperationException($"Provider {provider} is not registered");
            }

            try
            {
                // إنشاء الاتصال باستخدام DI
                var connection = (ITelephonyConnection)ActivatorUtilities.CreateInstance(
                    _serviceProvider, providerInfo.ConnectionType);

                // تهيئة الاتصال (يمكن أن تكون هذه خطوة منفصلة)
                await InitializeConnectionAsync(connection, configuration, cancellationToken);

                _logger.LogInformation("Created connection for provider: {Provider}", provider);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create connection for provider: {Provider}", provider);
                throw;
            }
        }

        private async Task InitializeConnectionAsync(
            ITelephonyConnection connection,
            ProviderConfiguration configuration,
            CancellationToken cancellationToken)
        {
            // يمكن إضافة منطق تهيئة هنا
            // مثال: تحقق من صحة التكوين، إعداد الرؤوس، إلخ.
            await Task.CompletedTask;
        }

        public IEnumerable<IServerEventAdapter> GetAdaptersForProvider(TelephonyProvider provider)
        {
            if (!_providerRegistry.TryGetValue(provider, out var providerInfo))
            {
                return Enumerable.Empty<IServerEventAdapter>();
            }

            // إنشاء المحول باستخدام DI
            var adapter = (IServerEventAdapter)ActivatorUtilities.CreateInstance(
                _serviceProvider, providerInfo.AdapterType);

            return new[] { adapter };
        }
    }
}
