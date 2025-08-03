namespace WeddingApp.UI.Models
{
    public class CachedUpload
    {
        public string FileName { get; set; }
        //public string Base64 { get; set; }
        public byte[] FileBytes { get; set; } //  Base64 yerine byte[]

        public string ContentType { get; set; }
        public string Ip { get; set; }
        public string Device { get; set; }
        public DateTime ReceivedAt { get; set; }

    }
}
