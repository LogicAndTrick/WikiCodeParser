using LogicAndTrick.WikiCodeParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace LogicAndTrick.WikiCodeParser.WinUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Parser _parser;
        private bool _webViewInitialised;

        public MainWindow()
        {
            _parser = new Parser(ParserConfiguration.Default());

            InitializeComponent();
            WebView.CoreWebView2InitializationCompleted += (_, _) =>
            {
                InitWebView();
                WebView.CoreWebView2.DOMContentLoaded += (_, _) =>
                {
                    _webViewInitialised = true;
                    Render();
                };
            };
            WebView.EnsureCoreWebView2Async().ConfigureAwait(false);
            TextBox.TextChanged += (_, _) => Render();

            TextBox.Text = @"= Title
Hello

== Heading

This is a test

~~~
information
~~~
";
        }

        private async void Render()
        {
            if (!_webViewInitialised) return;

            var text = TextBox.Text;
            try
            {
                var parsed = _parser.ParseResult(text);
                var content = "<div class=\"bbcode\">" + parsed.ToHtml() + "</div>" + HtmlMetadata(parsed.GetMetadata());
                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    content
                });
                await WebView.CoreWebView2.ExecuteScriptAsync($"display_result({json});");
            }
            catch (Exception ex)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = ex.Message,
                    stacktrace = ex.StackTrace
                });
                await WebView.CoreWebView2.ExecuteScriptAsync($"display_exception({json});");
            }
        }

        private void InitWebView()
        {
            var sb = new StringBuilder("<!DOCTYPE html>");
            sb.AppendLine(@"
<head>
<meta charset=""utf-8"">
<link href=""https://twhl.info/css/app.css"" rel=""stylesheet"">
<style>
@font-face {
 font-family:FontAwesome;
 font-style:normal;
 font-weight:400;
 src:url(https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/fonts/fontawesome-webfont.eot?v=4.7.0);
 src:url(https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/fonts/fontawesome-webfont.eot?#iefix&v=4.7.0) format(""embedded-opentype""),
 url(https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/fonts/fontawesome-webfont.woff2?v=4.7.0) format(""woff2""),
 url(https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/fonts/fontawesome-webfont.woff?v=4.7.0) format(""woff""),
 url(https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/fonts/fontawesome-webfont.ttf?v=4.7.0) format(""truetype""),
 url(https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/fonts/fontawesome-webfont.svg?v=4.7.0#fontawesomeregular) format(""svg"")
}
</style>
<link href=""https://fonts.googleapis.com/css?family=Roboto:400,300"" rel=""stylesheet"" type=""text/css"">
<script type=""text/javascript"" src=""https://twhl.info/js/all.js""></script>
<script>
function display_result({ content }) {
    const el = document.getElementById('el');
    el.innerHTML = content;
    el.querySelectorAll('pre code').forEach(code => {
        hljs.highlightBlock(code);
    });
    document.querySelectorAll('a').forEach(link => link.target = '_blank');
}
function display_exception({ message, stacktrace }) {
    const el = document.getElementById('el');
    el.innerHTML = `<h3>${message}</h3><p>${stacktrace}</p>`;
}
</script>
</head>
<body style=""background: #fffcf9;"">
<div id=""el"" style=""overflow: hidden; padding: 1rem;"">
</div>
</body>
</html>
");
            var html = sb.ToString();
            WebView.NavigateToString(html);
        }

        private string HtmlMetadata(List<KeyValuePair<string, object>> meta)
        {
            if (!meta.Any()) return "";

            var cats = new List<string>();
            var credits = new List<WikiRevisionCredit>();
            var others = new List<KeyValuePair<string, object>>();

            var sb = new StringBuilder();

            foreach (var kv in meta)
            {
                switch (kv.Key)
                {
                    case "WikiCategory":
                        cats.Add(kv.Value.ToString() ?? "");
                        break;
                    case "WikiCredit":
                        credits.Add((WikiRevisionCredit) kv.Value);
                        break;
                    case "WikiLink":
                    case "WikiUpload":
                        break;
                    default:
                        others.Add(kv);
                        break;
                }
            }

            if (cats.Any())
            {
                sb.Append(@"
<ul class=""wiki-categories inline-bullet"">
    <li class=""header"">Categories</li>");
                foreach (var cat in cats)
                {
                    sb.Append("<li><a href=\"https://twhl.info/wiki/page/category:").Append(Uri.EscapeDataString(cat.Replace(' ', '_'))).Append("\">").Append(cat.Replace('_', ' ')).Append("</a></li>");

                }
    sb.Append("</ul>");
            }

            if (credits.Any(x => x.Type != WikiRevisionCredit.TypeFull))
            {
                sb.Append(@"
<ul class=""wiki-credits"">
    <li class=""header"">Article Credits</li>");
                foreach (var cred in credits.Where(x => x.Type != WikiRevisionCredit.TypeFull))
                {
                    sb.Append("<li>");
                    if (cred.Type == WikiRevisionCredit.TypeCredit)
                    {
                        sb.Append("<span class=\"fa fa-pencil\"></span> ");
                        if (cred.UserID.HasValue) sb.Append("<a href=\"https://twhl.info/user/view/").Append(cred.UserID).Append("\">[Avatar for user #").Append(cred.UserID).Append("]</a>");
                        else if (!string.IsNullOrWhiteSpace(cred.Url)) sb.Append("<a href=\"").Append(cred.Url).Append("\">").Append(cred.Name).Append("</a>");
                        else sb.Append("<strong>").Append(cred.Name).Append("</strong>");
                        sb.Append(" &ndash; ").Append(cred.Description);
                    }
                    else if (cred.Type == WikiRevisionCredit.TypeArchive)
                    {
                        sb.Append("<span class=\"fa fa-globe\"></span> This article contains archived content from ");
                        sb.Append("<strong>").Append(cred.Name).Append("</strong>");
                        if (!string.IsNullOrWhiteSpace(cred.Description)) sb.Append(" &ndash; <em>").Append(cred.Description).Append("</em>");
                        sb.Append(".");
                        if (!string.IsNullOrWhiteSpace(cred.Url)) sb.Append("Original link <a href=\"").Append(cred.Url).Append("\">here</a>.");
                        if (!string.IsNullOrWhiteSpace(cred.WaybackUrl)) sb.Append("Archive link <a href=\"").Append(cred.WaybackUrl).Append("\">here</a>.");
                    }
                }

                if (credits.Any(x => x.Type == WikiRevisionCredit.TypeArchive))
                {
                    sb.Append(@"
                    <li>
                        <span class=""fa fa-info-circle""></span>
                        TWHL only publishes archived articles from defunct websites, or with permission.
                        For more information on TWHL's archiving efforts, please visit the
                        <a href=""https://twhl.info/wiki/page/TWHL_Archiving_Project"" title=""TWHL Archiving Project"">TWHL Archiving Project</a> page.
                    </li>");
                }
                sb.Append("</ul>");
            }

            if (credits.Any(x => x.Type == WikiRevisionCredit.TypeFull))
            {
                sb.Append(@"<div class=""wiki-archive-credits bbcode"">");
                foreach (var cred in credits.Where(x => x.Type == WikiRevisionCredit.TypeFull))
                {
                    sb.Append(@$"
                        <div class=""card card-info"">
                            <div class=""card-body"">
                                <div>
                                    <span class=""fa fa-globe""></span>
                                    This article was originally published on <strong>{cred.Name}</strong>{(String.IsNullOrWhiteSpace(cred.Description) ? "" : $"as <em>{cred.Description}</em>")}.
                                </div>
                    ");
                    if (!string.IsNullOrWhiteSpace(cred.Url))
                    {
                        sb.Append(@$"
                                <div class=""ml-3"">
                                    <span class=""fa fa-link""></span> The original URL of the article was <a href=""{cred.Url}"">{cred.Url}</a>.
                                </div>
                        ");
                    }

                    if (!string.IsNullOrWhiteSpace(cred.WaybackUrl))
                        sb.Append(@$"
                                <div class=""ml-3"">
                                    <span class=""fa fa-archive""></span> The archived page is available <a href=""{cred.WaybackUrl}"">here</a>.
                                </div>
                        ");
                    sb.Append(@"
                                <div>
                                    <span class=""fa fa-info-circle""></span>
                                    TWHL only publishes archived articles from defunct websites, or with permission.
                                    For more information on TWHL's archiving efforts, please visit the
                                    <a href=""https://twhl.info/wiki/page/TWHL_Archiving_Project"" title=""TWHL Archiving Project"">TWHL Archiving Project</a> page.
                                </div>
                            </div>
                        </div>
                    ");
                }
                sb.Append(@"</div>");
            }

            if (others.Any())
            {
                sb.AppendLine("<h6>Metadata</h6>");
                sb.Append("<dl class=\"dl-horizontal\">");

                foreach (var kv in others)
                {
                    sb.Append("<dt>");
                    sb.Append(kv.Key);
                    sb.Append("</dt><dd>");
                    sb.Append(kv.Value as string ?? "<pre>" + JsonSerializer.Serialize(kv.Value, new JsonSerializerOptions { WriteIndented = true }) + "</pre>");
                    sb.Append("</dd>");
                }

                sb.Append("</dl>");
            }

            return sb.ToString();
        }
    }
}
