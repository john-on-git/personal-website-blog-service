using JGWPersonalWebsiteBlogAPI;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;

namespace JGWPersonalWebsiteBlogAPITests
{
    public class Tests
    {
        private CustomWebApplicationFactory<Program> appFactory;
        private string correctAPIKey;
        private HttpClient client;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            appFactory = new CustomWebApplicationFactory<Program>();
            correctAPIKey = Environment.GetEnvironmentVariable("JGW_PERSONAL_BLOG_API_KEY") ?? throw new FileNotFoundException("No API key provided: set environmental variable JGW_PERSONAL_BLOG_API_KEY to a secure password.");

        }

        [OneTimeTearDown]
        public void RunAfterAnyTests() => appFactory.Dispose();

        [SetUp]
        public void Setup() => client = appFactory.CreateClient();

        [TearDown]
        public void TearDown() => client.Dispose();

        [Test]
        public async Task TestCreate()
        {
            // arrange
            const string title = "Test Article";
            const string authors = "Nunit Tests";
            const string snippet = "<p>Hello, World</p>";
            var postBody = new StringContent(
                JsonConvert.SerializeObject(new Article(title, authors, snippet)),
                System.Text.Encoding.UTF8,
                MediaTypeNames.Application.Json
            );

            // act
            using var res = await client.PostAsync($"/article/create?apiKey={correctAPIKey}", postBody);

            // assert
            
            // the reponse code is correct
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            // the article was created with the correct values
            var responseBody = await res.Content.ReadFromJsonAsync<Article>();
            Assert.That(responseBody, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(responseBody.Title, Is.EqualTo(title));
                Assert.That(responseBody.Authors, Is.EqualTo(authors));
                Assert.That(responseBody.HTMLSnippet, Is.EqualTo(snippet));
            });
        }
    }
}