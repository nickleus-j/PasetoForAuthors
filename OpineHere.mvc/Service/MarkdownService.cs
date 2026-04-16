using Markdig;

namespace OpineHere.mvc.Service;

public class MarkdownService
{
    public static string MarkdownToHtml(string markdownText)
    {
        var pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
        string htmlContent = Markdown.ToHtml(markdownText, pipeline); // Convert markdown to HTML using Markdig
        return $"<html><head><style></style></head><body>{htmlContent}</body></html>";
    }
}