using Avalonia.Media.Imaging;

namespace AvaloniaApplication.Models
{
    /// <summary>Представляет элемент, отображаемый в списке UI.</summary>
    public class ImageItem
    {
        public Bitmap Image { get; set; }

        public string Name { get; set; }

        public ImageItem(Bitmap image, string name)
        {
            Image = image;
            Name = name;
        }
    }
}
