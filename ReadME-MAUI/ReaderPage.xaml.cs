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

            string htmlContent = _currentBook.ReadingOrder[1].Content;

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
            overflow: hidden;
        }}
        #content {{
            padding: 30px 35px;
            font-family: Georgia, serif;
            font-size: 18px;
            line-height: 1.7;
            color: #111;
            text-align: justify;
            column-fill: auto;
            column-gap: 0px;
            overflow: hidden;
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
        let pageWidth = 0;
        let totalPages = 1;

        function init() {{
            let content = document.getElementById('content');
            let wrapper = document.getElementById('page-wrapper');

            pageWidth = wrapper.clientWidth;
            let pageHeight = wrapper.clientHeight;

            content.style.width = pageWidth + 'px';
            content.style.height = pageHeight + 'px';
            content.style.columnWidth = pageWidth + 'px';

            setTimeout(function() {{
                // scrollHeight / pageHeight = nb de colonnes de hauteur pageHeight
                let scrollH = content.scrollHeight;
                totalPages = Math.max(1, Math.round(scrollH / pageHeight));
                currentPage = 0;
                content.style.transform = 'translateX(0)';
            }}, 400);
        }}

        function nextPage() {{
            if (currentPage < totalPages - 1) {{
                currentPage++;
                let content = document.getElementById('content');
                content.style.transform = 'translateX(-' + (currentPage * pageWidth) + 'px)';
            }}
        }}

        function prevPage() {{
            if (currentPage > 0) {{
                currentPage--;
                let content = document.getElementById('content');
                content.style.transform = 'translateX(-' + (currentPage * pageWidth) + 'px)';
            }}
        }}

        // Bloquer tout scroll tactile
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
        await BookWebview.EvaluateJavaScriptAsync("nextPage()");
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        await BookWebview.EvaluateJavaScriptAsync("prevPage()");
    }
}