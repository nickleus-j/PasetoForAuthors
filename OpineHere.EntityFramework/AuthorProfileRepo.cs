using Microsoft.EntityFrameworkCore;
using OpineHere.Data;
using OpineHere.Data.entity;

namespace OpineHere.EntityFramework;

public class AuthorProfileRepo:EfRepository<AuthorProfile>,IAuthorProfileRepo
{
    private OpineContext context
    {
        get { return Context as OpineContext; }
    }
    public AuthorProfileRepo(DbContext context) : base(context)
    {
    }

    public async Task RegisterNewUserAsAuthorAsync(string userId, string givenName, string surname)
    {
        AuthorProfile profile = new AuthorProfile
        {
            GivenName = givenName,
            Surname = surname,
            UserId = new Guid(userId)
        };
        await context.AddAsync(profile);
        await context.SaveChangesAsync();
    }

    public async Task<AuthorProfile> GetProfileAsync(string userId)
    {
        Guid userGuid = Guid.Parse(userId);
        var profile=await context.AuthorProfile.SingleAsync(a=>a.UserId==userGuid);
        return profile;
    }
}