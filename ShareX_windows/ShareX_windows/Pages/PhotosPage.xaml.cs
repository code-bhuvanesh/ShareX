using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ShareX_windows.Pages
{
    public partial class PhotosPage : Page
    {
        private int port = 2347;
        private string tempPath = System.IO.Path.GetTempPath();
        private List<string> images = new List<string>();
        private ImageFullScreen fullScreenImage;

        public PhotosPage()
        {
            InitializeComponent();
            var pUtils = new Utils(port);
            fullScreenImage = new ImageFullScreen(images, -1, this);
            Debug.WriteLine("starting server for photos");
            new Thread(delegate ()
            {
                pUtils.hostServer();
                int imgLength = 0;
                var gotCount = int.TryParse(pUtils.receive_msg(),out imgLength);
                if(!gotCount)
                {
                    imgLength = 0;
                }

                var imageHolders = new List<custom_controls.CustomImage>();
                Dispatcher.BeginInvoke(delegate()
                {
                    for (int i = 0; i < imgLength; i++)
                    {
                        imageHolders.Add(new custom_controls.CustomImage()
                        {
                            CornerRadius = new CornerRadius(5),
                            Background = new SolidColorBrush(Color.FromArgb(80, 217, 207, 207)),
                            Height = 200,
                            Width = 200,
                            Margin = new Thickness(10),

                        });
                        photosPannel.Children.Add(imageHolders[i]);
                    }

                },DispatcherPriority.Send);



                for (int i = 0; i < imgLength; i++)
                {
                    var imgPath = pUtils.receiveFile("D:\\ShareX\\test\\");
                    if(imgPath != null)
                    {
                        //images.Add(new FileStream(imgPath, FileMode.Open));
                        images.Add(imgPath);
                        fullScreenImage.images = images;
                        var pos = i;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var bitImg = new BitmapImage();

                            //managing images in memory
                            using (var stream = new FileStream(imgPath, FileMode.Open, FileAccess.Read))
                            {
                                bitImg.BeginInit();
                                bitImg.DecodePixelWidth = 200;
                                bitImg.CacheOption = BitmapCacheOption.OnLoad;
                                bitImg.StreamSource = stream;
                                bitImg.EndInit();
                            }

                            bitImg.Freeze();
                            var img = new ImageBrush(bitImg);
                            img.Stretch = Stretch.UniformToFill;
                            //photosPannel.Children.Insert(i,new Border()
                            //{
                            //    CornerRadius = new CornerRadius(5),
                            //    Background = img,
                            //    Height = 140,
                            //    Width = 140,
                            //    Margin = new Thickness(10),

                            //});
                            imageHolders[pos].Background = img;
                            imageHolders[pos].imgPath = imgPath;
                            imageHolders[pos].pos = pos;
                            imageHolders[pos].MouseLeftButtonUp += showImage;

                        }), DispatcherPriority.Send);
                    }
                    else
                    {
                        Debug.WriteLine("image path is nulll");
                    }
                    
                } 
            }).Start();
           

        }

        private void showImage(object sender, MouseButtonEventArgs e)
        {
            var pos = (sender as custom_controls.CustomImage).pos;
            fullScreenImage.setImage(pos);
            NavigationService.Navigate(fullScreenImage);
        }
    }
}
