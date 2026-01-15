namespace AriNetClient.WebSockets.NewFolder.Models.DomainEvents
{
    public class PhoneNumber
    {
        public string Value { get; }
        public PhoneNumber(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}
