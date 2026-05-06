using VersOne.Epub;
using System.IO;
using ReadME_MAUI.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ReadME_MAUI
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Book> MyBooks { get; set; } = new(); //crée la db en ram
        private string libraryPath => Path.Combine(FileSystem.AppDataDirectory, "library.json");

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadLibrary();
        }

        private async void LoadLibrary()
        {
            if (File.Exists(libraryPath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(libraryPath);
                    var books = JsonSerializer.Deserialize<List<Book>>(json);
                    if (books != null)
                    {
                        foreach (var book in books)
                        {
                            MyBooks.Add(book);
                            _ = LoadCoverAsync(book);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task LoadCoverAsync(Book book)
        {
            try
            {
                if (File.Exists(book.File))
                {
                    var epub = await EpubReader.ReadBookAsync(book.File);
                    if (epub.CoverImage != null)
                    {
                        book.Cover = ImageSource.FromStream(() => new MemoryStream(epub.CoverImage));
                    }
                }
            }
            catch { }
        }

        private async void SaveLibrary()
        {
            try
            {
                string json = JsonSerializer.Serialize(MyBooks);
                await File.WriteAllTextAsync(libraryPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

                SaveLibrary();
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

        private async void OnBookOptionsTapped(object sender, TappedEventArgs e)
        {
            var label = sender as Label;
            if (label?.BindingContext is Book book)
            {
                string action = await DisplayActionSheet("Options", "Annuler", "Supprimer");
                if (action == "Supprimer")
                {
                    bool confirm = await DisplayAlert("Supprimer le livre", $"Voulez-vous vraiment supprimer '{book.Title}' ?", "Oui", "Non");
                    if (confirm)
                    {
                        MyBooks.Remove(book);
                        SaveLibrary();
                        try
                        {
                            if (File.Exists(book.File))
                            {
                                File.Delete(book.File);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

    }
}