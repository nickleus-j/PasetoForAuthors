using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpineHere.mvc.Models;

public class MarkdownPostDto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RawContent { get; set; }
    [MinLength(3)]
    public string Title { get; set; }

    public Guid? UserId { get; set; }

    [DisplayName("Author")]
    public string? AuthorName { get; set; }

    public DateTimeOffset PostDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastUpdate { get; set; } = DateTimeOffset.UtcNow;
}