using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReadME_MAUI.Models
{
    public class Book : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string File { get; set; }

        private ImageSource? _cover;
        [JsonIgnore]
        public ImageSource? Cover
        {
            get => _cover;
            set
            {
                if (_cover != value)
                {
                    _cover = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cover)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
