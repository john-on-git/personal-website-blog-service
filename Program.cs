using JGWPersonalWebsiteBlogAPI;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<BlogContext>(options => {
    options
        .UseSqlite("Data Source=blog.dat;Cache=Shared");
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
    async (uint id, BlogContext db) => await db.Articles
        .Where(x => x.Id==id)
        .SingleOrDefaultAsync()
);

string correctAPIKey = File.ReadAllText("api_key.txt");
app.MapPost(
    $"/article/create",
    async (string apiKey, string title, string authors, string HTMLSnippet, BlogContext db) =>
    {
        if(apiKey == correctAPIKey)
        {
            db.Articles.Add(new Article(title, authors, HTMLSnippet));
            await db.SaveChangesAsync();
            return Results.Ok();
        }
        else
        {
            return Results.Unauthorized();
        }
    }
);



app.Run();