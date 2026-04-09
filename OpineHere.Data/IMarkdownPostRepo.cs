using OpineHere.Data.entity;
namespace OpineHere.Data;

public interface IMarkdownPostRepo:IRepository<MarkdownPost>
{
    Task PenNamePost(string PenName, string body,string title="A Post");
    Task<IList<MarkdownPost>> GetPostsWithPenName(string penName);
}