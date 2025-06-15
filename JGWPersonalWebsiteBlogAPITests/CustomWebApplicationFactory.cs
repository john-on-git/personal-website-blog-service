using JGWPersonalWebsiteBlogAPI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Threading;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(IDbContextOptionsConfiguration<BlogContext>));

            services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbConnection));

            services.Remove(dbConnectionDescriptor);

            // Create open SqliteConnection so EF won't automatically close it.
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                return connection;
            });

            services.AddDbContext<BlogContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection).UseSeeding((context, _) =>
                {
                    context.Set<Article>().Add(new Article(
                        0,
                        $"Test Seed Article to be Updated",
                        "Not Updated",
                        "Not Updated")
                    );
                    for (int i = 0; i < 99; i++)
                    {
                        context.Set<Article>().Add(new Article(
                            $"Testing Seed Article {i}",
                            "Nunit Tests Seeding",
                            "<p>Hello World!</p>")
                        );
                    }
                    context.SaveChanges();

                })
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    context.Set<Article>().Add(new Article(
                        0,
                        $"Test Seed Article to be Updated",
                        "Not Updated",
                        "Not Updated")
                    );
                    for (int i=0;i<99;i++)
                    {
                        context.Set<Article>().Add(new Article(
                            $"Testing Seed Article {i}", 
                            "Nunit Tests Seeding", 
                            "<p>Hello World!</p>")
                        );
                    }
                    await context.SaveChangesAsync(cancellationToken);
                });
            });
        });

        builder.UseEnvironment("Development");
    }
}
