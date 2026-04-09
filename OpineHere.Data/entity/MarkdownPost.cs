using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace OpineHere.Data.entity;

[Table("Job")]
public class MarkdownPost
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The raw Markdown text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for a registered user. Null if using a Pen Name.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// A custom display name for the author. Null if using a UserId.
    /// </summary>
    public string? PenName { get; set; }

    public DateTimeOffset PostDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastUpdate { get; set; } = DateTimeOffset.UtcNow;
}