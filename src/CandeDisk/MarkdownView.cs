using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CandeDisk;

/// <summary>
/// Mostra un sottoinsieme minimo di Markdown (titoli, elenchi, **grassetto**, `codice`) come FlowDocument.
/// Serve a usare un solo file (docs/GUIDA.md) sia su GitHub sia nella guida dell'app.
/// </summary>
public static class MarkdownView
{
    private static readonly Regex Inline =
        new(@"(\*\*[^*]+\*\*|`[^`]+`|\[[^\]]+\]\([^)]+\))", RegexOptions.Compiled);
    private static readonly Regex Link = new(@"^\[([^\]]+)\]\(([^)]+)\)$", RegexOptions.Compiled);

    public static FlowDocument Build(string markdown)
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(24, 16, 24, 16),
        };

        List? currentList = null;
        foreach (string raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            string line = raw.TrimEnd();

            if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                currentList ??= new List { MarkerStyle = TextMarkerStyle.Disc, Margin = new Thickness(0, 0, 0, 8) };
                currentList.ListItems.Add(new ListItem(Body(line[2..], margin: 0)));
                continue;
            }
            if (currentList != null)
            {
                doc.Blocks.Add(currentList);
                currentList = null;
            }

            if (line.Length == 0) continue;
            if (line.StartsWith("```") || line == "---") continue;

            if (line.StartsWith("### ")) doc.Blocks.Add(Heading(line[4..], 15, "#333333"));
            else if (line.StartsWith("## ")) doc.Blocks.Add(Heading(line[3..], 18, "#1F6FD1"));
            else if (line.StartsWith("# ")) doc.Blocks.Add(Heading(line[2..], 24, "#14233C"));
            else if (line.StartsWith("> "))
            {
                var quote = Body(line[2..], margin: 8);
                quote.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF4, 0xCE));
                quote.Padding = new Thickness(10, 6, 10, 6);
                doc.Blocks.Add(quote);
            }
            else doc.Blocks.Add(Body(line, margin: 8));
        }
        if (currentList != null) doc.Blocks.Add(currentList);
        return doc;
    }

    private static Paragraph Heading(string text, double size, string color) => new(new Run(StripMarkup(text)))
    {
        FontSize = size,
        FontWeight = FontWeights.SemiBold,
        Foreground = (Brush)new BrushConverter().ConvertFromString(color)!,
        Margin = new Thickness(0, size >= 18 ? 16 : 10, 0, 6),
    };

    private static Paragraph Body(string text, double margin)
    {
        var p = new Paragraph { Margin = new Thickness(0, 0, 0, margin) };
        int last = 0;
        foreach (Match m in Inline.Matches(text))
        {
            if (m.Index > last) p.Inlines.Add(new Run(text[last..m.Index]));
            string token = m.Value;
            var link = Link.Match(token);
            if (token.StartsWith("**"))
                p.Inlines.Add(new Bold(new Run(token[2..^2])));
            else if (link.Success && Uri.TryCreate(link.Groups[2].Value, UriKind.Absolute, out var uri))
            {
                var hyperlink = new Hyperlink(new Run(link.Groups[1].Value)) { NavigateUri = uri };
                hyperlink.RequestNavigate += (_, e) =>
                {
                    AppInfo.OpenUrl(e.Uri.AbsoluteUri);
                    e.Handled = true;
                };
                p.Inlines.Add(hyperlink);
            }
            else
                p.Inlines.Add(new Run(token[1..^1])
                {
                    FontFamily = new FontFamily("Consolas"),
                    Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE)),
                });
            last = m.Index + m.Length;
        }
        if (last < text.Length) p.Inlines.Add(new Run(text[last..]));
        return p;
    }

    private static string StripMarkup(string text) => text.Replace("**", "").Replace("`", "");
}
