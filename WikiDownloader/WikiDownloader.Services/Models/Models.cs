namespace WikiDownloader.Services.Models
{
    public class WikiApiQueryResult<T>
    {
        public WikiApiQueryContinue Continue { get; set; }

        public T Query { get; set; }
    }

    public class WikiApiQueryContinue
    {
        public string ApContinue { get; set; }
    }

    public class WikiApiAllPages
    {
        public WikiApiAllPagesEntry[] AllPages { get; set; }
    }

    public class WikiApiAllPagesEntry
    {
        public long PageId { get; set; }

        public string Title { get; set; }
    }
}
