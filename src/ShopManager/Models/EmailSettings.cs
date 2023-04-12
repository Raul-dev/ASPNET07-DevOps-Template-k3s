namespace ShopManager.Models
{
    public class EmailSettings
    {
        public int Port { get; set; }
        public string Server { get; set; }
        public bool EnableSsl { get; set; }
        public string MessageTemplateRegex { get; set; }
        public string TemplateFile { get; set; } 
        public string CredentialsFile { get; set; } //TODO
        public string User { get; set; }
        public string Password { get; set; }
    }
}
