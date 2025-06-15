using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JGWPersonalWebsiteBlogAPI
{
    public class Article
    {
        public uint? Id { get; private set; } //PK
        public string Title { get; private set; }
        public string Authors { get; private set; }
        public string HTMLSnippet { get; private set; }
        public DateTime PostedAt { get; private set; }


        [JsonConstructor]
        public Article(uint? Id, string Title, string Authors, string HTMLSnippet, DateTime PostedAt)
        {
            this.Id = Id;
            this.Title = Title;
            this.Authors = Authors;
            this.HTMLSnippet = HTMLSnippet;
            this.PostedAt = PostedAt;
        }
        public Article(uint Id, string Title, string Authors, string HTMLSnippet)
        {
            this.Id = Id;
            this.Title = Title;
            this.Authors = Authors;
            this.HTMLSnippet = HTMLSnippet;
            this.PostedAt = DateTime.Now;
        }
        public Article(string Title, string Authors, string HTMLSnippet)
        {
            this.Id = null;
            this.Title = Title;
            this.Authors = Authors;
            this.HTMLSnippet = HTMLSnippet;
            this.PostedAt = DateTime.Now;
        }
    }
}