using OpineHere.Data.entity;
namespace OpineHere.Data;

public interface IMarkdownPostRepo:IRepository<MarkdownPost>
{
    Task PenNamePost(string PenName, string body);
    Task<IList<MarkdownPost>> GetPostsWithPenName(string penName);
}