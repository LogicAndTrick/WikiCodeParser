using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }

        private async void Render()
        {
            if (!_webViewInitialised) return;

            var text = TextBox.Text;
            try
            {
                var parsed = _parser.ParseResult(text);
                var content = parsed.ToHtml() + HtmlMetadata(parsed.GetMetadata());
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
<link href=""https://fonts.googleapis.com/css?family=Roboto:400,300"" rel=""stylesheet"" type=""text/css"">
<script type=""text/javascript"" src=""https://twhl.info/js/all.js""></script>
<script>
function display_result({ content }) {
    const el = document.getElementById('el');
    el.innerHTML = content;
}
function display_exception({ message, stacktrace }) {
    const el = document.getElementById('el');
    el.innerHTML = `<h3>${message}</h3><p>${stacktrace}</p>`;
}
</script>
</head>
<body style=""background: #fffcf9;"">
<div class=""bbcode"" id=""el"">
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

            var sb = new StringBuilder("<h6>Metadata</h6>");
            sb.Append("<ul>");

            foreach (var kv in meta)
            {
                sb.Append("<li>");
                sb.Append("<strong>");
                sb.Append(kv.Key);
                sb.Append(":</strong> ");
                sb.Append(kv.Value);
                sb.Append("</li>");
            }

            sb.Append("</ul>");
            return sb.ToString();
        }
    }
}
