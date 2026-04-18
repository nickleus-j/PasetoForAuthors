using JetBrains.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OpineHere.Data.entity;
namespace OpineHere.EntityFramework.Tests;

[TestSubject(typeof(AuthorProfileRepo))]
public class AuthorProfileRepoTests
    {
        // Helper to create a unique database for every test run
        private OpineContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OpineContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new OpineContext(options);
        }

        [Fact]
        public async Task RegisterNewUserAsAuthorAsync_PersistsProfile_WithCorrectData()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using(var context = GetInMemoryContext(dbName)){
                var repo = new AuthorProfileRepo(context);

                var userId = Guid.NewGuid().ToString();
                var givenName = "Jane";
                var surname = "Doe";

                // Act
                await repo.RegisterNewUserAsAuthorAsync(userId, givenName, surname);

                // Assert
                var savedProfile = await context.AuthorProfile.FirstOrDefaultAsync(p => p.UserId == new Guid(userId));

                Assert.NotNull(savedProfile);
                Assert.Equal(givenName, savedProfile.GivenName);
                Assert.Equal(surname, savedProfile.Surname);
                Assert.Equal(new Guid(userId), savedProfile.UserId);
            }
        }

        [Fact]
        public async Task GetProfileAsync_ReturnsCorrectProfile_WhenUserExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetInMemoryContext(dbName))
            {

                var targetGuid = Guid.NewGuid();
                var profile = new AuthorProfile
                {
                    UserId = targetGuid,
                    GivenName = "John",
                    Surname = "Smith"
                };

                context.AuthorProfile.Add(profile);
                await context.SaveChangesAsync();

                var repo = new AuthorProfileRepo(context);

                // Act
                var result = await repo.GetProfileAsync(targetGuid.ToString());

                // Assert
                Assert.NotNull(result);
                Assert.Equal(targetGuid, result.UserId);
                Assert.Equal("John", result.GivenName);
            }
        }

        [Fact]
        public async Task GetProfileAsync_ThrowsInvalidOperationException_WhenUserDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetInMemoryContext(dbName))
            {
                var repo = new AuthorProfileRepo(context);

                // Act & Assert
                // Your implementation uses .SingleAsync(), which throws an exception if no match is found.
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await repo.GetProfileAsync(Guid.NewGuid().ToString());
                });
            }
        }
    }