using AriNetClient.WebSockets.Abstracts;
using AriNetClient.WebSockets.Configuration;

namespace AriNetClient.WebSockets.Clients.Reconnection
{
    /// <summary>
    /// استراتيجية إعادة الاتصال باستخدام زيادة تأخر أُسية (Exponential Backoff)
    /// </summary>
    public class ExponentialBackoffReconnectionStrategy : IReconnectionStrategy
    {
        private readonly WebSocketOptions _options;
        private int _retryCount;
        private TimeSpan _currentDelay;

        public int MaxRetryCount => _options.MaxReconnectionAttempts;
        public int CurrentRetryCount => _retryCount;
        public TimeSpan CurrentDelay => _currentDelay;

        public ExponentialBackoffReconnectionStrategy(WebSocketOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Reset();
        }

        public TimeSpan GetNextDelay()
        {
            if (_retryCount == 0)
            {
                _currentDelay = TimeSpan.FromMilliseconds(_options.InitialReconnectDelayMs);
            }
            else
            {
                // حساب التأخير الجديد باستخدام الزيادة الأُسية
                double multiplier = Math.Pow(_options.ReconnectDelayMultiplier, _retryCount);
                double nextDelayMs = _options.InitialReconnectDelayMs * multiplier;

                // تطبيق الحد الأقصى للتأخير
                nextDelayMs = Math.Min(nextDelayMs, _options.MaxReconnectDelayMs);

                _currentDelay = TimeSpan.FromMilliseconds(nextDelayMs);
            }

            _retryCount++;
            return _currentDelay;
        }

        public void Reset()
        {
            _retryCount = 0;
            _currentDelay = TimeSpan.Zero;
        }

        public bool CanRetry()
        {
            return _retryCount < _options.MaxReconnectionAttempts;
        }

        public TimeSpan GetEstimatedTotalDelay()
        {
            TimeSpan total = TimeSpan.Zero;
            int tempRetryCount = 0;

            while (tempRetryCount < _retryCount)
            {
                double multiplier = Math.Pow(_options.ReconnectDelayMultiplier, tempRetryCount);
                double delayMs = Math.Min(
                    _options.InitialReconnectDelayMs * multiplier,
                    _options.MaxReconnectDelayMs);

                total += TimeSpan.FromMilliseconds(delayMs);
                tempRetryCount++;
            }

            return total;
        }
    }
}
