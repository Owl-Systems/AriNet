using AriNetClient.WebSockets.NewFolder.Models.DomainEvents;
using AriNetClient.WebSockets.NewFolder.Models.ServerEvents;

namespace AriNetClient.WebSockets.NewFolder.Abstracts
{
    /// <summary>
    /// واجهة لمحول حدث الخادم
    /// كل مزود (Wazo, Asterisk, etc.) له محوله الخاص
    /// </summary>
    public interface IServerEventAdapter
    {
        /// <summary>
        /// اسم المزود الذي يدعمه هذا المحول (مثال: "Wazo", "Asterisk")
        /// </summary>
        string SupportedProvider { get; }

        /// <summary>
        /// يتحقق مما إذا كان يمكن لهذا المحول معالجة حدث خادم معين
        /// </summary>
        bool CanHandle(RawServerEvent rawEvent);

        /// <summary>
        /// يحول حدث الخادم الأولي إلى حدث نطاق موحد
        /// </summary>
        /// <returns>قائمة أحداث النطاق (قد يكون هناك أكثر من واحد)</returns>
        Task<IReadOnlyList<DomainEvent>> AdaptAsync(
            RawServerEvent rawEvent,
            CancellationToken cancellationToken = default);
    }
}
