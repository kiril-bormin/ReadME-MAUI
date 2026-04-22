using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadME_MAUI.Models
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string File { get; set; }
        public ImageSource? Cover { get; set; }

    }
}
