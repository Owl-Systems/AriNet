namespace AriNetClient.WebSockets.NewFolder.Configuration
{
    /// <summary>
    /// خيارات تهيئة المزود
    /// </summary>
    public class ProviderOptions
    {
        /// <summary>
        /// نوع المزود النشط
        /// </summary>
        public TelephonyProvider ActiveProvider { get; set; } = TelephonyProvider.Wazo;

        /// <summary>
        /// هل يتم السماح بمزودين متعددين في نفس الوقت؟
        /// </summary>
        public bool AllowMultipleProviders { get; set; } = false;

        /// <summary>
        /// قائمة المزودين المسجلين
        /// </summary>
        public List<TelephonyProvider> RegisteredProviders { get; set; } = new();

        /// <summary>
        /// تكوينات خاصة بكل مزود
        /// </summary>
        public Dictionary<TelephonyProvider, ProviderConfiguration> ProviderConfigurations { get; set; }
            = new();
    }
}
