namespace JGWPersonalWebsiteBlogAPI
{
    public class Article(string Title, string Authors, string HTMLSnippet)
    {
        public uint Id { get; private set; } //PK
        public string Title { get; private set; } = Title;
        public string Authors { get; private set; } = Authors;
        public string HTMLSnippet { get; private set; } = HTMLSnippet;
        public DateTime PostedAt { get; private set; } = DateTime.Now;
    }
}