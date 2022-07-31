using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShareX_windows.Pages
{
    /// <summary>
    /// Interaction logic for ImageFullScreen.xaml
    /// </summary>
    public partial class ImageFullScreen : Page
    {
        private object photoPage;
        public int pos = 0;
        public List<string> images { get; set; }
        public ImageFullScreen(List<string> images, int pos, Object photoPage)
        {
            InitializeComponent();
            this.images = images;
            this.pos = pos;

            this.photoPage = photoPage;

            //setImage(pos);
           
        }

        public void setPos(int pos)
        {
            this.pos = pos;
        }

        public void setImage(int pos)
        {
            this.pos = pos;
            Debug.WriteLine($"current position is {pos}");
            if(pos >= 0)
            {
                var bitImg = new BitmapImage();

                using (var stream = new FileStream(images[pos], FileMode.Open, FileAccess.Read))
                {
                    bitImg.BeginInit();
                    bitImg.CacheOption = BitmapCacheOption.OnLoad;
                    bitImg.StreamSource = stream;
                    bitImg.EndInit();
                }

                bitImg.Freeze();

                fullImage.Source = bitImg;
            }
            
        }

        private void rightArrow(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"right arrow is {pos}");

            if (pos < images.Count-1)
            {
                pos++;
                setImage(pos);
            }

        }

        private void leftArrow(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"left arrow is {pos}");

            if (pos > 0)
            {
                pos--;
                setImage(pos);
            }
        }

        private void goBack(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(photoPage);
        }
    }
}
