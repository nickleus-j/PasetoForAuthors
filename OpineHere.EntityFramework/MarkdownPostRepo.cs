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
    public async Task PenNamePost(string PenName, string body,string title="A Post")
    {
        var post = new MarkdownPost
        {
            PenName = PenName,
            Title = title,
            Content = body,
            LastUpdate = DateTime.UtcNow,
            PostDate = DateTime.UtcNow,
        };
        context.Add(post);
        await context.SaveChangesAsync();
    }

    public async Task<IList<MarkdownPost>> GetPostsWithPenName(string penName)
    {
        return await context.MarkdownPost.Where(p => p.PenName == penName).OrderByDescending(p=>p.LastUpdate).ToListAsync();
    }

    public async Task<IList<MarkdownPost>> GetPostsWithPenName(string penName, int page, int pageSize = 10)
    {
        return await context.MarkdownPost.Where(p => p.PenName == penName)
            .OrderByDescending(p=>p.LastUpdate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize).ToListAsync();
    }

    public async Task<bool> HasMorePost(int page, int pageSize)
    {
        var posts = await GetFromPageAsync(page, pageSize, "LastUpdate","desc");
        return posts.Any();
    }
}