using Microsoft.EntityFrameworkCore;

namespace JGWPersonalWebsiteBlogAPI
{
    public class BlogContext(DbContextOptions<BlogContext> options) : DbContext(options)
    {
        public DbSet<Article> Articles => Set<Article>();
    }
}
