using OpineHere.Data.entity;

namespace OpineHere.Data;

public interface IAuthorProfileRepo:IRepository<AuthorProfile>
{
    Task RegisterNewUserAsAuthorAsync(string userId,string givenName,string surname);
    Task<AuthorProfile> GetProfileAsync(string userId);
}