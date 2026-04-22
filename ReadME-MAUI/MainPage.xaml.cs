using VersOne.Epub;
using System.IO;
using ReadME_MAUI.Models;
using System.Collections.ObjectModel;
namespace ReadME_MAUI
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Book> MyBooks { get; set; } = new(); //crée la db en ram

        public MainPage()
        {
            InitializeComponent();

            BindingContext = this;
        }
        private async void ImportBook(object sender, EventArgs e)
        {

            try
            {
                var result = await FilePicker.Default.PickAsync(); // demander à l'os d'ouvrir l'explorateur
                if (result == null) return;

                // Créer un chemin permanent dans le stockage local de l'appli
                string targetPath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);

                // Copier le fichier du cache temporaire au dossier permanent 
                using (var sourceStream = await result.OpenReadAsync())
                using (var targetStream = File.Create(targetPath))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }

                EpubBook book = await EpubReader.ReadBookAsync(targetPath); //VersOne extrait les metadonnées

                MyBooks.Add(new Book
                {
                    Title = book.Title,
                    Author = book.Author,
                    File = targetPath,
                    // conversion de l'image binaire de l'Epub en ImageSource MAUI
                    Cover = book.CoverImage != null ? ImageSource.FromStream(() => new MemoryStream(book.CoverImage)) : null
                });

            }
            catch (Exception error)
            {
                await DisplayAlert("Erreur d'importation de livre", error.Message, "Ok");
            }
        }
        private async void OnSelectedBook(object sender, SelectionChangedEventArgs e)
        {
            // on récupère le livre séléctioné 
            var selectedBook = e.CurrentSelection.FirstOrDefault() as Models.Book;

            // on montre la page ReaderPage avec le livre 
            if (selectedBook != null)
            {
                await Navigation.PushAsync(new ReaderPage(selectedBook.File));

                ((CollectionView)sender).SelectedItem = null;
            }
        }

    }
}