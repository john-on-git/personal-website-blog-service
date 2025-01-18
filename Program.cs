using JGWPersonalWebsiteBlogAPI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddDbContext<BlogContext>(options => {
    options
        .UseSqlite("Data Source=blog.dat;Cache=Shared")
        .UseSeeding((ctx, _) =>
        {
            BlogContext blogCtx = (BlogContext)ctx;
            if(!blogCtx.Articles.Any()) //insert test articles if they don't already exist
            {
                blogCtx.Articles.Add(new Article("Test1", "John", "<p>Hello World!</p>"));
                blogCtx.Articles.Add(new Article("Test2", "John", "<p>Hello Birds!</p>"));
                blogCtx.Articles.Add(new Article("Test3", "John", "<p>Hello Sky!!!</p>"));
                blogCtx.SaveChanges();
            }
        });
});
var app = builder.Build();

app.Services.CreateScope().ServiceProvider.GetRequiredService<BlogContext>()?.Database.EnsureCreated(); //create the database with test values if it doesn't already exist

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();


//set up endpoints
app.MapGet(
    $"/article/index",
    async (uint? offset, BlogContext db) =>
    {
        return offset==null ? 
            await db.Articles //no offset
                .OrderBy(x => x.PostedAt)
                .ToListAsync()
            :
            await db.Articles //with offset
                .OrderBy(x => x.PostedAt)
                .Skip((int)offset)
                .ToListAsync();
    }
);

app.MapGet(
    $"/article/detail",
    (uint id, BlogContext db) => db.Articles
        .Where(x => x.Id==id)
        .Single()
);




app.Run();