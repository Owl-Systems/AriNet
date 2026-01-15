namespace AriNetClient.WebSockets.Abstracts
{
    /// <summary>
    /// واجهة استراتيجية إعادة الاتصال
    /// </summary>
    public interface IReconnectionStrategy
    {
        /// <summary>
        /// أقصى عدد محاولات إعادة الاتصال
        /// </summary>
        int MaxRetryCount { get; }

        /// <summary>
        /// الوقت الحالي بين المحاولات
        /// </summary>
        TimeSpan CurrentDelay { get; }

        /// <summary>
        /// الحصول على التأخير للمحاولة التالية
        /// </summary>
        TimeSpan GetNextDelay();

        /// <summary>
        /// إعادة تعيين الاستراتيجية
        /// </summary>
        void Reset();

        /// <summary>
        /// التحقق مما إذا كان يمكن إعادة المحاولة
        /// </summary>
        bool CanRetry();
    }
}
