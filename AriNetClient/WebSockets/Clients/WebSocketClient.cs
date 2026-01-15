using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Clients.Connection;
using AriNetClient.WebSockets.Configuration;
using AriNetClient.WebSockets.Events;
using AriNetClient.WebSockets.Events.Call;
using AriNetClient.WebSockets.Services.EventHandlers;
using AriNetClient.WebSockets.Services.EventHandlers.Call;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AriNetClient.WebSockets.Clients
{
    /// <summary>
    /// العميل الرئيسي لـ WebSocket يدير الاتصال ومعالجة الأحداث بشكل متكامل
    /// </summary>
    public class WebSocketClient : IAsyncDisposable
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IEventHandlerRegistry _eventHandlerRegistry;
        private readonly WebSocketOptions _options;
        private readonly ILogger<WebSocketClient> _logger;
        private readonly IServiceProvider _serviceProvider;

        private bool _isInitialized;
        private bool _isSubscribed;
        private bool _isDisposed;

        public event EventHandler<BaseEvent> EventReceived;
        public event EventHandler Connected;
        public event EventHandler<string> Disconnected;
        public event EventHandler<Exception> ErrorOccurred;

        public bool IsConnected => _connectionManager.IsConnected;
        public bool IsInitialized => _isInitialized;
        public bool IsSubscribed => _isSubscribed;
        public string ApplicationName => _options.ApplicationName;

        public WebSocketClient(
            IConnectionManager connectionManager,
            IEventDispatcher eventDispatcher,
            IEventHandlerRegistry eventHandlerRegistry,
            IOptions<WebSocketOptions> options,
            ILogger<WebSocketClient> logger,
            IServiceProvider serviceProvider)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _eventHandlerRegistry = eventHandlerRegistry ?? throw new ArgumentNullException(nameof(eventHandlerRegistry));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _connectionManager.Connected += OnConnectionManagerConnected;
            _connectionManager.Disconnected += OnConnectionManagerDisconnected;
            _connectionManager.MessageReceived += OnConnectionManagerMessageReceived;
            _connectionManager.ErrorOccurred += OnConnectionManagerErrorOccurred;
        }

        /// <summary>
        /// تهيئة العميل وتسجيل المعالجات الأساسية
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_isInitialized)
            {
                _logger.LogWarning("WebSocketClient is already initialized");
                return;
            }

            try
            {
                _logger.LogInformation("Initializing WebSocketClient");

                // تسجيل المعالجات الأساسية للأحداث الثلاثة
                RegisterCoreEventHandlers();

                // تسجيل معالج الأحداث العامة (لتسجيل جميع الأحداث)
                RegisterGlobalEventHandlers();

                _isInitialized = true;
                _logger.LogInformation("WebSocketClient initialized successfully with {HandlerCount} handlers",
                    _eventHandlerRegistry.GetTotalHandlerCount());

                // الاتصال التلقائي إذا كان مفعلاً في الخيارات
                if (_options.AutoReconnect)
                {
                    await ConnectAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize WebSocketClient");
                throw;
            }
        }

        private void RegisterCoreEventHandlers()
        {
            // تسجيل المعالجات للأحداث الثلاثة الأساسية
            _eventDispatcher.RegisterHandler<CallStartedEvent,
                CallStartedEventHandler>();

            _eventDispatcher.RegisterHandler<CallUpdatedEvent,
                CallUpdatedEventHandler>();

            _eventDispatcher.RegisterHandler<CallEndedEvent,
                CallEndedEventHandler>();

            _logger.LogDebug("Registered core event handlers for call events");
        }

        private void RegisterGlobalEventHandlers()
        {
            try
            {
                var globalLogger = _serviceProvider.GetService<GlobalEventLogger>();
                if (globalLogger != null)
                {
                    _eventDispatcher.RegisterGlobalHandler(globalLogger);
                    _logger.LogDebug("Registered global event logger");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to register global event logger");
            }
        }

        /// <summary>
        /// الاتصال بخادم WebSocket والاشتراك في الأحداث
        /// </summary>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
            {
                await InitializeAsync(cancellationToken);
            }

            if (IsConnected)
            {
                _logger.LogWarning("Already connected to WebSocket");
                return;
            }

            try
            {
                _logger.LogInformation("Connecting WebSocketClient");

                // الاتصال بخادم WebSocket
                await _connectionManager.ConnectAsync(cancellationToken);

                // لا حاجة لإرسال رسالة اشتراك منفصلة في Wazo
                // الاشتراك يتم عبر معلمة app في URL

                _isSubscribed = true;
                _logger.LogInformation("WebSocketClient connected and subscribed to events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect WebSocketClient");
                throw;
            }
        }

        /// <summary>
        /// قطع الاتصال بخادم WebSocket
        /// </summary>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                _logger.LogWarning("Not connected to WebSocket");
                return;
            }

            try
            {
                _logger.LogInformation("Disconnecting WebSocketClient");
                await _connectionManager.DisconnectAsync(cancellationToken);
                _isSubscribed = false;
                _logger.LogInformation("WebSocketClient disconnected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disconnect WebSocketClient");
                throw;
            }
        }

        /// <summary>
        /// إعادة الاتصال بخادم WebSocket
        /// </summary>
        public async Task<bool> ReconnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Reconnecting WebSocketClient");
                var result = await _connectionManager.ReconnectAsync(cancellationToken);

                if (result)
                {
                    _isSubscribed = true;
                    _logger.LogInformation("WebSocketClient reconnected successfully");
                }
                else
                {
                    _logger.LogWarning("WebSocketClient reconnection failed");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect WebSocketClient");
                return false;
            }
        }

        /// <summary>
        /// تسجيل معالج حدث جديد
        /// </summary>
        public void RegisterEventHandler<TEvent, THandler>()
            where TEvent : BaseEvent
            where THandler : IEventHandler<TEvent>
        {
            _eventDispatcher.RegisterHandler<TEvent, THandler>();
            _logger.LogInformation("Registered new event handler {Handler} for event {Event}",
                typeof(THandler).Name, typeof(TEvent).Name);
        }

        /// <summary>
        /// إرسال رسالة إلى الخادم عبر WebSocket
        /// </summary>
        public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                throw new Exceptions.WebSocketDisconnectedException(_options.Url, System.Net.WebSockets.WebSocketState.Closed);
            }

            await _connectionManager.SendAsync(message, cancellationToken);
        }

        private void OnConnectionManagerConnected(object sender, EventArgs e)
        {
            _logger.LogInformation("WebSocket connection established");
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionManagerDisconnected(object sender, string reason)
        {
            _logger.LogInformation("WebSocket disconnected: {Reason}", reason);
            Disconnected?.Invoke(this, reason);
            _isSubscribed = false;
        }

        private void OnConnectionManagerErrorOccurred(object sender, Exception exception)
        {
            _logger.LogError(exception, "WebSocket error occurred");
            ErrorOccurred?.Invoke(this, exception);
        }

        private async void OnConnectionManagerMessageReceived(object sender, string message)
        {
            try
            {
                var @event = ParseEventMessage(message);
                if (@event != null)
                {
                    await ProcessIncomingEventAsync(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process WebSocket message");
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        private BaseEvent? ParseEventMessage(string message)
        {
            try
            {
                // تحليل الرسالة كـ JSON للحصول على نوع الحدث
                using var jsonDoc = JsonDocument.Parse(message);
                var eventType = jsonDoc.RootElement.GetProperty("event_type").GetString();

                if (string.IsNullOrEmpty(eventType))
                {
                    _logger.LogWarning("Received message without event_type: {Message}", message);
                    return null;
                }

                // التحقق مما إذا كان الحدث مدرجاً في قائمة الاستبعاد
                if (_options.ExcludeEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Event {EventType} is excluded, skipping", eventType);
                    return null;
                }

                // التحقق مما إذا كان الاشتراك محدداً ولم يكن الحدث مدرجاً
                if (_options.SubscribeToEvents.Any() &&
                    !_options.SubscribeToEvents.Contains(eventType, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Event {EventType} is not in subscription list, skipping", eventType);
                    return null;
                }

                // إنشاء النموذج المناسب حسب نوع الحدث
                var @event = CreateEventInstance(eventType, message);
                if (@event != null)
                {
                    _logger.LogWarning("Unknown event type received: {EventType}", eventType);

                    // يمكن إنشاء حدث عام للأنواع غير المعروفة
                    @event = JsonSerializer.Deserialize<BaseEvent>(message);
                }

                return @event;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse WebSocket message as JSON: {Message}", message);
                throw new Exceptions.UnknownEventException("Invalid JSON", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error parsing event message");
                throw;
            }
        }

        private BaseEvent? CreateEventInstance(string eventType, string message)
        {
            // التعرف على نوع الحدث وإنشاء النموذج المناسب
            return eventType.ToLower() switch
            {
                "started" or "call_started" or "callstarted"
                    => JsonSerializer.Deserialize<CallStartedEvent>(message),

                "updated" or "call_updated" or "callupdated"
                    => JsonSerializer.Deserialize<CallUpdatedEvent>(message),

                "ended" or "call_ended" or "callended"
                    => JsonSerializer.Deserialize<CallEndedEvent>(message),

                // يمكن إضافة المزيد من الأنواع هنا في المستقبل
                _ => null
            };
        }

        private async Task ProcessIncomingEventAsync(BaseEvent @event)
        {
            try
            {
                // التحقق من صحة الحدث إذا كان مفعلاً في الخيارات
                if (_options.ValidateEvents && !ValidateEvent(@event))
                {
                    _logger.LogWarning("Event validation failed for {EventType}", @event.EventType);
                    return;
                }

                // إطلاق الحدث للمشتركين
                EventReceived?.Invoke(this, @event);

                // توزيع الحدث إلى المعالجات
                await DispatchEventToHandlersAsync(@event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process event: {EventType}", @event.EventType);
                throw;
            }
        }

        private bool ValidateEvent(BaseEvent @event)
        {
            // التحقق الأساسي من صحة الحدث
            if (string.IsNullOrEmpty(@event.EventType))
                return false;

            if (@event.Timestamp == default)
                return false;

            // يمكن إضافة المزيد من قواعد التحقق هنا
            return true;
        }

        private async Task DispatchEventToHandlersAsync(BaseEvent @event)
        {
            // استخدام النوع الديناميكي للحدث لاستدعاء المناسب
            switch (@event)
            {
                case CallStartedEvent callStarted:
                    await _eventDispatcher.DispatchAsync(callStarted);
                    break;

                case CallUpdatedEvent callUpdated:
                    await _eventDispatcher.DispatchAsync(callUpdated);
                    break;

                case CallEndedEvent callEnded:
                    await _eventDispatcher.DispatchAsync(callEnded);
                    break;

                default:
                    _logger.LogWarning("No specific handler for event type: {EventType}", @event.EventType);

                    // توزيع الحدث إلى المعالجات العامة فقط
                    var globalHandlers = _eventHandlerRegistry.GetGlobalHandlers();
                    foreach (var handler in globalHandlers)
                    {
                        if (handler.CanHandle(@event.EventType))
                        {
                            await handler.HandleAsync(@event);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات الاتصال
        /// </summary>
        public ConnectionStatistics GetStatistics()
        {
            var connectionManager = _connectionManager as WebSocketConnectionManager;

            return new ConnectionStatistics
            {
                IsConnected = IsConnected,
                IsInitialized = _isInitialized,
                IsSubscribed = _isSubscribed,
                TotalHandlers = _eventHandlerRegistry.GetTotalHandlerCount(),
                Uptime = connectionManager?.Uptime ?? TimeSpan.Zero,
                ReconnectionAttempts = connectionManager?.ReconnectionAttempts ?? 0
            };
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                // قطع الاتصال
                if (IsConnected)
                {
                    await DisconnectAsync();
                }

                // تنظيف المعالجات
                if (_eventHandlerRegistry is EventHandling.EventHandlerRegistry registry)
                {
                    registry.Clear();
                }

                // إلغاء ربط الأحداث
                _connectionManager.Connected -= OnConnectionManagerConnected;
                _connectionManager.Disconnected -= OnConnectionManagerDisconnected;
                _connectionManager.MessageReceived -= OnConnectionManagerMessageReceived;
                _connectionManager.ErrorOccurred -= OnConnectionManagerErrorOccurred;

                _logger.LogInformation("WebSocketClient disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing WebSocketClient");
            }
        }
    }
}
