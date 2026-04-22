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
                EpubBook book = await EpubReader.ReadBookAsync(result.FullPath); //VersOne extrait les metadonnées

                MyBooks.Add(new Book
                {
                    Title = book.Title,
                    Author = book.Author,
                    // Conversion de l'image binaire de l'Epub en ImageSource MAUI
                    Cover = book.CoverImage != null ? ImageSource.FromStream(() => new MemoryStream(book.CoverImage)) : null
                });

            }
            catch (Exception error)
            {
                await DisplayAlert("Erreur d'importation de livre", error.Message, "D'accord");
            }
        }


    }
}