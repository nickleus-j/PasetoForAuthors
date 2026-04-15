using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using OpineHere.Data.entity;
using OpineHere.EntityFramework;
using Xunit;

namespace OpineHere.EntityFramework.Tests;

[TestSubject(typeof(MarkdownPostRepo))]
public class MarkdownPostRepoTest
{
// Helper method to conjure a fresh, isolated in-memory database for each test
        private OpineContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OpineContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
                
            return new OpineContext(options);
        }

        [Fact]
        public async Task PenNamePost_AddsNewPostToDatabase_Successfully()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString(); // Unique DB ensures test isolation
            using var context = GetInMemoryContext(dbName);
            var repo = new MarkdownPostRepo(context);

            string expectedPenName = "CodeWizard";
            string expectedBody = "This is the body of the post.";
            string expectedTitle = "My Magical Post";

            // Act
            await repo.PenNamePost(expectedPenName, expectedBody, expectedTitle);

            // Assert
            var savedPost = await context.MarkdownPost.SingleOrDefaultAsync();
            
            Assert.NotNull(savedPost);
            Assert.Equal(expectedPenName, savedPost.PenName);
            Assert.Equal(expectedBody, savedPost.Content);
            Assert.Equal(expectedTitle, savedPost.Title);
            
            // Verify timestamps were set properly (allowing a small window for execution time)
            Assert.True(savedPost.PostDate <= DateTime.UtcNow);
            Assert.True(savedPost.LastUpdate <= DateTime.UtcNow);
        }

        [Fact]
        public async Task PenNamePost_UsesDefaultTitle_WhenTitleOmitted()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            var repo = new MarkdownPostRepo(context);

            // Act
            await repo.PenNamePost("CodeWizard", "Body only");

            // Assert
            var savedPost = await context.MarkdownPost.SingleOrDefaultAsync();
            Assert.NotNull(savedPost);
            Assert.Equal("A Post", savedPost.Title); // Checking the default parameter
        }

        [Fact]
        public async Task GetPostsWithPenName_ReturnsOnlyPostsMatchingTheGivenPenName()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            
            // Seed the in-memory database
            context.MarkdownPost.AddRange(
                new MarkdownPost { PenName = "CodeWizard", Title = "Post 1", Content = "Body 1" },
                new MarkdownPost { PenName = "CodeWizard", Title = "Post 2", Content = "Body 2" },
                new MarkdownPost { PenName = "TechMage", Title = "Post 3", Content = "Body 3" }
            );
            await context.SaveChangesAsync();

            var repo = new MarkdownPostRepo(context);

            // Act
            var results = await repo.GetPostsWithPenName("CodeWizard");

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, post => Assert.Equal("CodeWizard", post.PenName));
        }

        [Fact]
        public async Task GetPostsWithPenName_ReturnsEmptyList_WhenNoMatchesExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetInMemoryContext(dbName);
            
            // Seed data with different pen names
            context.MarkdownPost.Add(new MarkdownPost { PenName = "TechMage", Title = "Post 1", Content = "Body 1" });
            await context.SaveChangesAsync();

            var repo = new MarkdownPostRepo(context);

            // Act
            var results = await repo.GetPostsWithPenName("PhantomWriter");

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }
}