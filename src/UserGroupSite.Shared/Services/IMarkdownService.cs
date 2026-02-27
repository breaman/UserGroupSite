namespace UserGroupSite.Shared.Services;

/// <summary>Converts Markdown text to sanitized HTML.</summary>
public interface IMarkdownService
{
    /// <summary>Renders the given Markdown string as HTML.</summary>
    /// <param name="markdown">The raw Markdown content.</param>
    /// <returns>The rendered HTML string.</returns>
    string ToHtml(string markdown);
}
