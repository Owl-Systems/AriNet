using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;

namespace AriNetClient.WebSockets.Clients.Connection
{
    /// <summary>
    /// مدير اتصال WebSocket المسؤول عن الاتصال الفعلي وإدارة دورة الحياة
    /// </summary>
    public class WebSocketConnectionManager : IConnectionManager
    {
        private readonly ILogger<WebSocketConnectionManager> _logger;
        private readonly WebSocketOptions _options;
        private readonly IReconnectionStrategy _reconnectionStrategy;

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _receiveCancellationTokenSource;
        private CancellationTokenSource _globalCancellationTokenSource;
        private bool _isDisposed;
        private int _reconnectionAttempts;
        private DateTime _lastConnectionTime;

        //Events
        public event EventHandler Connected;
        public event EventHandler<string> Disconnected;
        public event EventHandler<string> MessageReceived;
        public event EventHandler<Exception> ErrorOccurred;

        //Properties
        public WebSocketState State => _webSocket?.State ?? WebSocketState.None;
        public bool IsConnected => State == WebSocketState.Open;
        public int ReconnectionAttempts => _reconnectionAttempts;
        public TimeSpan Uptime => IsConnected ? DateTime.UtcNow - _lastConnectionTime : TimeSpan.Zero;

        public WebSocketConnectionManager(
            ILogger<WebSocketConnectionManager> logger,
            IOptions<WebSocketOptions> options,
            IReconnectionStrategy reconnectionStrategy)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _reconnectionStrategy = reconnectionStrategy ?? throw new ArgumentNullException(nameof(reconnectionStrategy));

            _webSocket = new ClientWebSocket();
            _globalCancellationTokenSource = new CancellationTokenSource();

            _logger.LogDebug("WebSocketConnectionManager initialized");
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            // 1. التحقق من عدم الاتصال مسبقاً
            if (IsConnected)
            {
                _logger.LogWarning("WebSocket is already connected");
                return;
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _globalCancellationTokenSource.Token);

            try
            {
                _logger.LogInformation("Connecting to WebSocket at {Url}", _options.Url);

                // 2. إعداد رأس المصادقة
                _webSocket.Options.SetRequestHeader("X-Auth-Token", _options.AuthToken);

                // 3. الاتصال بالخادم
                var uri = new Uri(_options.Url);
                await _webSocket.ConnectAsync(uri, linkedCts.Token);

                _lastConnectionTime = DateTime.UtcNow;
                _reconnectionAttempts = 0;
                _reconnectionStrategy.Reset();

                _logger.LogInformation("WebSocket connected successfully to {Url}", _options.Url);

                // 4. بدء استقبال الرسائل
                StartReceivingMessages();

                // 5. إطلاق حدث الاتصال الناجح
                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to WebSocket at {Url}", _options.Url);
                ErrorOccurred?.Invoke(this, ex);
                throw new Exceptions.WebSocketConnectionException(_options.Url, 0, 1, ex);
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (!IsConnected && State != WebSocketState.Connecting)
            {
                _logger.LogWarning("WebSocket is not connected");
                return;
            }

            try
            {
                _logger.LogInformation("Disconnecting WebSocket");

                // إلغاء استقبال الرسائل
                _receiveCancellationTokenSource?.Cancel();

                if (IsConnected)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client requested disconnect",
                        cancellationToken);
                }

                _logger.LogInformation("WebSocket disconnected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while disconnecting WebSocket");
                ErrorOccurred?.Invoke(this, ex);
            }
            finally
            {
                Disconnected?.Invoke(this, "Manual disconnect");
            }
        }

        public async Task SendAsync(string message, CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                _logger.LogError("Cannot send message: WebSocket is not connected");
                throw new Exceptions.WebSocketDisconnectedException(_options.Url, State);
            }

            try
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                var segment = new ArraySegment<byte>(buffer);

                await _webSocket.SendAsync(
                    segment,
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken);

                _logger.LogDebug("Message sent successfully ({Length} bytes)", buffer.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send WebSocket message");
                ErrorOccurred?.Invoke(this, ex);
                throw;
            }
        }

        public async Task<bool> ReconnectAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.AutoReconnect)
            {
                _logger.LogWarning("Auto-reconnect is disabled");
                return false;
            }

            if (!_reconnectionStrategy.CanRetry())
            {
                _logger.LogError("Maximum reconnection attempts reached");
                return false;
            }

            _reconnectionAttempts++;
            var delay = _reconnectionStrategy.GetNextDelay();

            _logger.LogInformation("Reconnecting attempt {Attempt}/{Max} in {Delay}ms",
                _reconnectionAttempts, _reconnectionStrategy.MaxRetryCount, delay.TotalMilliseconds);

            try
            {
                // الانتظار قبل إعادة المحاولة
                await Task.Delay(delay, cancellationToken);

                // قطع الاتصال الحالي إذا كان موجوداً
                await DisconnectAsync(cancellationToken);

                // إنشاء WebSocket جديد
                _webSocket?.Dispose();
                _webSocket = new ClientWebSocket();

                // إعادة الاتصال
                await ConnectAsync(cancellationToken);

                _logger.LogInformation("Reconnected successfully on attempt {Attempt}", _reconnectionAttempts);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnection attempt {Attempt} failed", _reconnectionAttempts);
                ErrorOccurred?.Invoke(this, new Exceptions.WebSocketReconnectionFailedException(
                    _options.Url, _reconnectionStrategy.GetNextDelay(), ex));

                return false;
            }
        }

        private void StartReceivingMessages()
        {
            _receiveCancellationTokenSource = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _receiveCancellationTokenSource.Token,
                _globalCancellationTokenSource.Token);

            _ = Task.Run(() => ReceiveMessagesLoopAsync(linkedCts.Token), linkedCts.Token);
        }

        private async Task ReceiveMessagesLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[_options.ReceiveBufferSize];
            var messageBuilder = new StringBuilder();

            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messagePart = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(messagePart);

                        if (result.EndOfMessage)
                        {
                            var fullMessage = messageBuilder.ToString();
                            messageBuilder.Clear();

                            _logger.LogDebug("Message received: {Message}", fullMessage);
                            MessageReceived?.Invoke(this, fullMessage);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket closed by server");
                        await HandleServerDisconnectAsync(cancellationToken);
                        break;
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // تم إلغاء العملية بشكل متعمد
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving WebSocket message");
                    ErrorOccurred?.Invoke(this, ex);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await HandleConnectionErrorAsync(ex, cancellationToken);
                    }

                    break;
                }
            }
        }

        private async Task HandleServerDisconnectAsync(CancellationToken cancellationToken)
        {
            Disconnected?.Invoke(this, "Server closed connection");

            if (_options.AutoReconnect)
            {
                await ReconnectAsync(cancellationToken);
            }
        }

        private async Task HandleConnectionErrorAsync(Exception error, CancellationToken cancellationToken)
        {
            _logger.LogWarning("WebSocket connection error: {Message}", error.Message);
            Disconnected?.Invoke(this, $"Connection error: {error.Message}");

            if (_options.AutoReconnect)
            {
                await ReconnectAsync(cancellationToken);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _logger.LogDebug("Disposing WebSocketConnectionManager");

                    // إلغاء جميع العمليات
                    _globalCancellationTokenSource?.Cancel();
                    _receiveCancellationTokenSource?.Cancel();

                    // إغلاق WebSocket
                    try
                    {
                        if (IsConnected)
                        {
                            _webSocket?.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                "Client disposing",
                                CancellationToken.None).Wait(TimeSpan.FromSeconds(5));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error closing WebSocket during disposal");
                    }
                    finally
                    {
                        _webSocket?.Dispose();
                        _globalCancellationTokenSource?.Dispose();
                        _receiveCancellationTokenSource?.Dispose();
                    }

                    _logger.LogInformation("WebSocketConnectionManager disposed");
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~WebSocketConnectionManager()
        {
            Dispose(disposing: false);
        }
    }
}
