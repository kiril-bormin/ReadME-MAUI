using System;
using System.Text;
using VersOne.Epub;

namespace ReadME_MAUI;

public partial class ReaderPage : ContentPage
{
    private EpubBook _currentBook;

    public ReaderPage(string filePath)
    {
        InitializeComponent();
        LoadBook(filePath);
    }

    private async void LoadBook(string path)
    {
        try
        {
            _currentBook = await EpubReader.ReadBookAsync(path);
            TopTitleLabel.Text = _currentBook.Title;

            StringBuilder fullContent = new StringBuilder();
            bool isFirst = true;
            for (int i = 1; i < _currentBook.ReadingOrder.Count; i++)
            {
                var chapter = _currentBook.ReadingOrder[i];
                string chapterHtml = chapter.Content;
                if (string.IsNullOrWhiteSpace(chapterHtml)) continue;

                chapterHtml = System.Text.RegularExpressions.Regex.Replace(chapterHtml, "<img[^>]*>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                string style = isFirst ? "" : " style='break-before: column;'";
                isFirst = false;

                int bodyStart = chapterHtml.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                if (bodyStart >= 0)
                {
                    int bodyEnd = chapterHtml.IndexOf("</body>", bodyStart, StringComparison.OrdinalIgnoreCase);
                    if (bodyEnd >= 0)
                    {
                        int contentStart = chapterHtml.IndexOf(">", bodyStart) + 1;
                        string bodyContent = chapterHtml.Substring(contentStart, bodyEnd - contentStart);
                        fullContent.Append($"<div class='chapter'{style}>{bodyContent}</div>");
                    }
                    else
                    {
                        fullContent.Append($"<div class='chapter'{style}>{chapterHtml}</div>");
                    }
                }
                else
                {
                    fullContent.Append($"<div class='chapter'{style}>{chapterHtml}</div>");
                }
            }
            string htmlContent = fullContent.ToString();

            string styledHtml = $@"
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
    <style>
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
        html, body {{
            width: 100vw;
            height: 100vh;
            overflow: hidden;
            touch-action: none;
            background-color: white;
        }}
        #page-wrapper {{
            width: 100vw;
            height: 100vh;
            padding: 30px 35px;
            overflow: hidden;
            box-sizing: border-box;
        }}
        #content {{
            height: 100%;
            font-family: Georgia, serif;
            font-size: 18px;
            line-height: 1.7;
            color: #111;
            text-align: justify;
            column-fill: auto;
            column-width: calc(100vw - 70px);
            column-gap: 70px;
            transition: transform 0.35s ease;
        }}
        img {{ max-width: 100%; height: auto; display: block; margin: auto; }}
    </style>
</head>
<body>
    <div id='page-wrapper'>
        <div id='content'>{htmlContent}</div>
    </div>
    <script>
        let currentPage = 0;
        let totalPages = 1;

        function init() {{
            let content = document.getElementById('content');
            let wrapper = document.getElementById('page-wrapper');

            setTimeout(function() {{
                let scrollW = content.scrollWidth;
                let exactShiftW = wrapper.getBoundingClientRect().width;
                
                totalPages = Math.max(1, Math.round((scrollW + 70) / exactShiftW));
                
                if (currentPage >= totalPages) {{
                    currentPage = Math.max(0, totalPages - 1);
                }}
                
                content.style.transform = 'translateX(calc(-100vw * ' + currentPage + '))';
            }}, 150);
        }}

        function getPercentage() {{
            if (totalPages <= 1) return 100;
            return Math.round((currentPage / (totalPages - 1)) * 100);
        }}

        function nextPage() {{
            if (currentPage < totalPages - 1) {{
                currentPage++;
                let content = document.getElementById('content');
                content.style.transform = 'translateX(calc(-100vw * ' + currentPage + '))';
            }}
            return getPercentage();
        }}

        function prevPage() {{
            if (currentPage > 0) {{
                currentPage--;
                let content = document.getElementById('content');
                content.style.transform = 'translateX(calc(-100vw * ' + currentPage + '))';
            }}
            return getPercentage();
        }}

        document.addEventListener('touchmove', function(e) {{ e.preventDefault(); }}, {{ passive: false }});

        window.addEventListener('load', function() {{
            setTimeout(init, 300);
        }});
        window.addEventListener('resize', init);
    </script>
</body>
</html>";

            MainThread.BeginInvokeOnMainThread(() =>
            {
                BookWebview.Source = new HtmlWebViewSource { Html = styledHtml };
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        string result = await BookWebview.EvaluateJavaScriptAsync("nextPage()");
        if (!string.IsNullOrEmpty(result) && result != "null")
        {
            PercentageLabel.Text = $"{result}%";
        }
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        string result = await BookWebview.EvaluateJavaScriptAsync("prevPage()");
        if (!string.IsNullOrEmpty(result) && result != "null")
        {
            PercentageLabel.Text = $"{result}%";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}