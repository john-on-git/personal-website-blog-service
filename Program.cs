using JGWPersonalWebsiteBlogAPI;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Globalization;

//try get API key
if (!File.Exists("api_key.txt"))
    throw new FileNotFoundException("No API key provided: create an api_key.txt in root directory containing a secure password.");
string correctAPIKey = File.ReadAllText("api_key.txt").Trim();

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.Configure<ForwardedHeadersOptions>(options => //docs say this is necessary when using a reverse proxy (I am), to transfer data between the 
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
builder.Services.AddDbContext<BlogContext>(options => {
    options
        .UseSqlite("Data Source=blog.dat;Cache=Shared;");
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

//use everything we just built
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.Services.CreateScope().ServiceProvider.GetRequiredService<BlogContext>()?.Database.EnsureCreated(); //create the database with test values if it doesn't already exist
app.UseCors("myCorsPolicy");
app.UseRateLimiter();






//set up endpoints

//fetch articles in groups of 12
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

//get an individual article. Not used at the moment, but it's a fairly standard operation so it's likely to come up at some point. future-proofing
app.MapGet(
    $"/article/details",
    async (uint id, BlogContext db) => await db.Articles
        .Where(x => x.Id == id)
        .SingleOrDefaultAsync()
).RequireRateLimiting("fixed");

//insert a new article
app.MapPost(
    $"/article/create",
    async (string apiKey, string title, string authors, string HTMLSnippet, BlogContext db) =>
    {
        if (string.Equals(apiKey, correctAPIKey, StringComparison.InvariantCulture)) //ensure the client's authenticated
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

//update an article
app.MapPut(
    $"/article/update",
    async (string apiKey, uint id, string title, string authors, string HTMLSnippet, BlogContext db) =>
    {
        if (string.Equals(apiKey, correctAPIKey, StringComparison.InvariantCulture)) //ensure the client's authenticated
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

//delete an article
app.MapDelete(
    $"/article/delete",
    async (string apiKey, uint id, BlogContext db) =>
    {
        if (string.Equals(apiKey, correctAPIKey, StringComparison.InvariantCulture)) //ensure the client's authenticated
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