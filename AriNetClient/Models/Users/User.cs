namespace AriNetClient.Models.Users
{
    // نموذج المستخدم
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string MobilePhoneNumber { get; set; }
        public bool Enabled { get; set; }
        public List<string> Lines { get; set; }
        public List<string> Extensions { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
