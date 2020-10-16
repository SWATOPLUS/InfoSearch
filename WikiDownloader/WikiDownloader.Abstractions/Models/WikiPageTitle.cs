namespace WikiDownloader.Abstractions.Models
{
    public class WikiPageTitle
    {
        public string Name { get; set; }

        public string ReferenceName { get; set; }
        
        public bool IsProcessed { get; set; }
    }
}
