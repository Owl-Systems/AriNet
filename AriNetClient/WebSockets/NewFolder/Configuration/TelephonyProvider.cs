namespace AriNetClient.WebSockets.NewFolder.Configuration
{
    /// <summary>
    /// أنواع مزودي خدمة الهاتف المدعومين
    /// </summary>
    public enum TelephonyProvider
    {
        /// <summary>
        /// نظام Wazo مفتوح المصدر
        /// </summary>
        Wazo,

        /// <summary>
        /// نظام Asterisk مفتوح المصدر
        /// </summary>
        Asterisk,

        /// <summary>
        /// نظام FreeSWITCH مفتوح المصدر
        /// </summary>
        FreeSwitch,

        /// <summary>
        /// نظام اتصال مخصص (للتطوير والاختبار)
        /// </summary>
        Custom
    }
}
