using JGWPersonalWebsiteBlogAPI;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;


if(!File.Exists("api_key.txt"))
    throw new FileNotFoundException("No API key provided: create an api_key.txt in root directory containing a secure password.");


var builder = WebApplication.CreateBuilder(args);

string correctAPIKey = File.ReadAllText("api_key.txt");


// Add services to the container.
builder.Services.AddDbContext<BlogContext>(options => {
    options
        .UseSqlite("Data Source=blog.dat;Cache=Shared");
});
builder.Services.AddCors(options => {
    options.AddPolicy(name: "myCorsPolicy", policy => policy.WithOrigins(
        //insert allowed services here
        "http://127.0.0.1:3000"
    ));
});
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(
    "fixed",
    options =>
    {
        options.QueueLimit = 1;
        options.PermitLimit = 1;
        options.Window = TimeSpan.FromSeconds(2); //TODO change me
    }
));
var app = builder.Build();
app.Services.CreateScope().ServiceProvider.GetRequiredService<BlogContext>()?.Database.EnsureCreated(); //create the database with test values if it doesn't already exist
app.UseCors("myCorsPolicy");
app.UseRateLimiter();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();


//set up endpoints

//fetch articles in groups of 12
//TODO convert to viewmodel (just id, title, postedAt)
const int PER_PAGE = 12;
app.MapGet(
    $"/article/index",
    async (uint? offset, BlogContext db) =>
    {
        offset ??= 0; //assign zero if null

        return await db.Articles //with offset
            .OrderByDescending(x => x.PostedAt)
            .Skip((int)offset * PER_PAGE)
            .Take(PER_PAGE)
            .ToListAsync();
    }
).RequireRateLimiting("fixed");

app.MapGet(
    $"/article/details",
    async (uint id, BlogContext db) => await db.Articles
        .Where(x => x.Id == id)
        .SingleOrDefaultAsync()
).RequireRateLimiting("fixed");

app.MapPost(
    $"/article/create",
    async (string apiKey, string title, string authors, string HTMLSnippet, BlogContext db) =>
    {
        if (apiKey == correctAPIKey) //ensure the client's authenticated
        {
            //insert the new article
            db.Articles.Add(new Article(title, authors, HTMLSnippet));
            await db.SaveChangesAsync();
            return Results.Created();
        }
        else
        {
            return Results.Unauthorized();
        }
    }
).RequireRateLimiting("fixed");
app.MapPut(
    $"/article/update",
    async (string apiKey, uint id, string title, string authors, string HTMLSnippet, BlogContext db) =>
    {
        if (apiKey == correctAPIKey) //ensure the client's authenticated
        {
            //update the record if it exists
            if (db.Articles.Where(x => x.Id == id).Any())
            {
                db.Articles.Update(new Article(id, title, authors, HTMLSnippet));
                await db.SaveChangesAsync();
                return Results.Ok();
            }
            else
                return Results.BadRequest();
        }
        else
        {
            return Results.Forbid();
        }
    }
).RequireRateLimiting("fixed");

app.MapDelete(
    $"/article/delete",
    async (string apiKey, uint id, BlogContext db) =>
    {
        if (apiKey == correctAPIKey) //ensure the client's authenticated
        {
            //update the record if it exists
            if (db.Articles.Where(x => x.Id == id).Any())
            {
                db.Articles.Remove(new Article(id, "", "", ""));
                await db.SaveChangesAsync();
                return Results.Ok();
            }
            else
                return Results.BadRequest();
        }
        else
        {
            return Results.Forbid();
        }
    }
).RequireRateLimiting("fixed");


app.Run();