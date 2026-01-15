namespace AriNetClient.WebSocket
{
    /// <summary>
    /// واجهة معالج الأحداث العامة (لجميع الأحداث)
    /// </summary>
    public interface IGlobalEventHandler : IAutoEventHandler
    {
        // يمكن إضافة خصائص أو دوال إضافية للمعالجات العامة
        // يتم توريث كل شيء من IAutoEventHandler
    }
}
