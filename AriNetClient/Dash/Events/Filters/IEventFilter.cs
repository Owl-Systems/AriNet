using AriNetClient.Dash.Events;

namespace AriNetClient.Dash.Events.Filters
{
    public interface IEventFilter
    {
        Task<bool> ShouldProcessAsync(WazoEvent @event);
    }

    public class EventTypeFilter : IEventFilter
    {
        private readonly HashSet<string> _allowedEventTypes;

        public EventTypeFilter(params string[] eventTypes)
        {
            _allowedEventTypes = new HashSet<string>(eventTypes, StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> ShouldProcessAsync(WazoEvent @event)
        {
            return Task.FromResult(_allowedEventTypes.Contains(@event.EventType));
        }
    }

    public class ChannelFilter : IEventFilter
    {
        private readonly HashSet<string> _channelIds;

        public ChannelFilter(params string[] channelIds)
        {
            _channelIds = new HashSet<string>(channelIds);
        }

        public Task<bool> ShouldProcessAsync(WazoEvent @event)
        {
            var channelId = GetChannelId(@event);
            return Task.FromResult(string.IsNullOrEmpty(channelId) || _channelIds.Contains(channelId));
        }

        private string GetChannelId(WazoEvent @event)
        {
            return @event switch
            {
                IChannelEvent channelEvent => GetChannelIdFromEvent(channelEvent),
                _ => null
            };
        }

        private string GetChannelIdFromEvent(IChannelEvent channelEvent)
        {
            // Implementation would use reflection or pattern matching
            return null;
        }
    }

    public class CompositeFilter : IEventFilter
    {
        private readonly List<IEventFilter> _filters;
        private readonly FilterMode _mode;

        public enum FilterMode
        {
            And,
            Or
        }

        public CompositeFilter(FilterMode mode = FilterMode.And)
        {
            _filters = new List<IEventFilter>();
            _mode = mode;
        }

        public CompositeFilter AddFilter(IEventFilter filter)
        {
            _filters.Add(filter);
            return this;
        }

        public async Task<bool> ShouldProcessAsync(WazoEvent @event)
        {
            if (_filters.Count == 0) return true;

            var results = new List<bool>();
            foreach (var filter in _filters)
            {
                results.Add(await filter.ShouldProcessAsync(@event));
            }

            return _mode == FilterMode.And
                ? results.All(r => r)
                : results.Any(r => r);
        }
    }
}
