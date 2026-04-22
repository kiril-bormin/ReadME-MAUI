using VersOne.Epub;

namespace ReadME_MAUI;

public partial class ReaderPage : ContentPage
{
    private int _currentChapterIndex = 0;
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
            // Lire le fichier epub
            EpubBook book = await EpubReader.ReadBookAsync(path);

            // changement de titre en haut de la page
            TopTitleLabel.Text = book.Title;

            // Extraction du premier chapitre
            string htmlContent = book.ReadingOrder[1].Content;

            string styledHtml = $@"
            <html>
            <head>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body {{ 
                        font-family: sans-serif; 
                        font-size: 16px; 
                        line-height: 1.5; 
                        padding: 15px; 
                        color: black;
                        background-color: white;
                        text-align: justify;
                    }}
                    img {{ max-width: 100%; height: auto; }}
                </style>
            </head>
            <body>{htmlContent}</body>
            </html>";

            // Affichage dans la WebView
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
}