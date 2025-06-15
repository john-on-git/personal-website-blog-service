using JGWPersonalWebsiteBlogAPI;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using NUnit.Framework.Internal;
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
            correctAPIKey = Environment.GetEnvironmentVariable("JGW_PERSONAL_BLOG_API_KEY") ?? throw new FileNotFoundException();
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests() => appFactory.Dispose();

        [SetUp]
        public void Setup() => client = appFactory.CreateClient();

        [TearDown]
        public void TearDown() => client.Dispose();

        [Test]
        public async Task TestCreateUnauthorized()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task TestCreateSuccess()
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

            // the response code is correct
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

        [Test]
        public async Task TestIndex()
        {
            // arrange (nothing to do)

            // act
            using var res = await client.GetAsync($"/article");

            // assert

            // the response code is correct
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // the response contains twelve articles
            var responseBody = await res.Content.ReadFromJsonAsync<Article[]>();
            Assert.That(responseBody, Is.Not.Null);
            Assert.Multiple(() =>
            {
                // correct number of values
                Assert.That(responseBody, Has.Length.EqualTo(12));
                
                // all articles are not null and all properties are not null
                Assert.That(responseBody, Has.All.Matches((Article x) => 
                x!=null && 
                x.Id!=null && 
                x.Title!=null && 
                x.Authors!=null && 
                x.HTMLSnippet!=null
            )); 
            });
        }

        [Test]
        public async Task TestUpdateUnauthorized()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task TestUpdateSuccess()
        {
            // arrange
            const string title = "Updated";
            const string authors = "Nunit Tests";
            const string snippet = "Updated";
            var postBody = new StringContent(
                JsonConvert.SerializeObject(new Article(0, title, authors, snippet)),
                System.Text.Encoding.UTF8,
                MediaTypeNames.Application.Json
            );

            // act
            using var res = await client.PutAsync($"/article/update?apiKey={correctAPIKey}", postBody);

            // assert

            // the response code is correct
            Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // the article was updated with the correct values
            var responseBody = await res.Content.ReadFromJsonAsync<Article>();
            Assert.That(responseBody, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(responseBody.Title, Is.EqualTo(title));
                Assert.That(responseBody.Authors, Is.EqualTo(authors));
                Assert.That(responseBody.HTMLSnippet, Is.EqualTo(snippet));
            });
        }

        [Test]
        public async Task TestDeleteUnauthorized()
        {
            throw new NotImplementedException();
        }

        [Test]
        public async Task TestDeleteSuccess()
        {
            // arrange
            var articleToDelete = new Article(
                1,
                "Test Seed Article to be Deleted",
                "Nunit Tests Seeding",
                "Delete Me!",
                DateTime.UnixEpoch
            );

            // act
            using var deleteRes = await client.DeleteAsync($"/article/delete?apiKey={correctAPIKey}&id={articleToDelete.Id}");

            // assert

            // the response code is correct
            Assert.That(deleteRes.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // the article was updated with the correct values
            var deleteResBody = await deleteRes.Content.ReadFromJsonAsync<Article>();
            Assert.That(deleteResBody, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(deleteResBody, Is.EqualTo(articleToDelete));
            });

            // GET returns nothing
            using var getRes = await client.GetAsync($"/article?id={articleToDelete.Id}");
            var getResBody = await getRes.Content.ReadAsStringAsync();
            Assert.Multiple(() =>
            {
                Assert.That(getRes.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(getResBody, Is.EqualTo(string.Empty));
            });
        }
    }
}