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
            padding: 30px 35px;
            overflow: hidden;
        }}
        #content {{
            height: 100%;
            font-family: Georgia, serif;
            font-size: 18px;
            line-height: 1.7;
            color: #111;
            text-align: justify;
            column-fill: auto;
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
        let shiftWidth = 0;
        let totalPages = 1;

        function init() {{
            let content = document.getElementById('content');
            let wrapper = document.getElementById('page-wrapper');

            content.style.transform = 'none';
            content.style.width = 'auto';
            content.style.height = '100%';
            content.style.columnWidth = 'auto';
            content.style.columnGap = 'normal';

            let wrapperWidth = wrapper.clientWidth; 
            let style = window.getComputedStyle(wrapper);
            let padLeft = parseFloat(style.paddingLeft) || 0;
            let padRight = parseFloat(style.paddingRight) || 0;
            
            let contentInnerWidth = wrapperWidth - padLeft - padRight;
            
            content.style.columnWidth = contentInnerWidth + 'px';
            content.style.columnGap = (padLeft + padRight) + 'px';
            
            shiftWidth = wrapperWidth;

            setTimeout(function() {{
                let scrollW = content.scrollWidth;
                totalPages = Math.max(1, Math.ceil(scrollW / shiftWidth));
                
                if (currentPage >= totalPages) {{
                    currentPage = Math.max(0, totalPages - 1);
                }}
                
                content.style.transform = 'translateX(-' + (currentPage * shiftWidth) + 'px)';
            }}, 100);
        }}

        function nextPage() {{
            if (currentPage < totalPages - 1) {{
                currentPage++;
                let content = document.getElementById('content');
                content.style.transform = 'translateX(-' + (currentPage * shiftWidth) + 'px)';
            }}
        }}

        function prevPage() {{
            if (currentPage > 0) {{
                currentPage--;
                let content = document.getElementById('content');
                content.style.transform = 'translateX(-' + (currentPage * shiftWidth) + 'px)';
            }}
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
        await BookWebview.EvaluateJavaScriptAsync("nextPage()");
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        await BookWebview.EvaluateJavaScriptAsync("prevPage()");
    }
}