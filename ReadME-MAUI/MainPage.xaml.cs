using VersOne.Epub;
namespace ReadME_MAUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage() => InitializeComponent();
        private async void ImportBook(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(); // demander à l'os d'ouvrir l'explorateur
                if (result == null) return; 
                EpubBook book = await EpubReader.ReadBookAsync(result.FullPath); //versone extrait les metadonnées

                await DisplayAlert("Livre ", $"Titre : {book.Title}\nAuteur : {book.Author} \n{book.CoverImage}", "Cool");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur d'importation de livre", ex.Message, "OK");
            }
        }
    }
}