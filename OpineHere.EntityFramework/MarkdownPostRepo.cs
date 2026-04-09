using Microsoft.EntityFrameworkCore;
using OpineHere.Data;
using OpineHere.Data.entity;
namespace OpineHere.EntityFramework;

public class MarkdownPostRepo:EfRepository<MarkdownPost>,IMarkdownPostRepo
{
    public MarkdownPostRepo(DbContext context) : base(context)
    {
    }
    private OpineContext context
    {
        get { return Context as OpineContext; }
    }
    public async Task PenNamePost(string PenName, string body)
    {
        var post = new MarkdownPost
        {
            PenName = PenName,
            Content = body,
            LastUpdate = DateTime.UtcNow,
            PostDate = DateTime.UtcNow,
        };
        context.Add(post);
        await context.SaveChangesAsync();
    }

    public async Task<IList<MarkdownPost>> GetPostsWithPenName(string penName)
    {
        return await context.MarkdownPost.Where(p => p.PenName == penName).ToListAsync();
    }
}