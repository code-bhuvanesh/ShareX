using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using ShareX_windows.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareX_windows.Views
{
    public sealed partial class ImageFullscreenView : UserControl
    {
        public ImageFullScreenViewModel viewModel;
        public ObservableCollection<Image> imagesList;
        public List<string> ImagePaths { get; }
        public NavigationView Nav { get; }
        public PhotosView Pv { get; }
        
        private int curIndex = 0;

        private string tempPath = System.IO.Path.GetTempPath() + "sharex\\";


        public ImageFullscreenView(NavigationView nav,PhotosView pv)
        {
            this.InitializeComponent();
            //viewModel = new ImageFullScreenViewModel(imagePaths, curPos);
            imagesList = new ObservableCollection<Image>();
            ImagePaths = new List<string>();
            Nav = nav;
            Pv = pv;
        }

        public void updateImages(string imgPath)
        {
            ImagePaths.Add(tempPath + imgPath);
        }
        
        
        private void back_btn(object sender, RoutedEventArgs e)
        {
            Nav.Content = Pv;
        }

    
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            curIndex--;
            if (curIndex >=0)
            {
                setImage(curIndex);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            curIndex++;
            if(curIndex < ImagePaths.Count)
            {
                setImage(curIndex);
            }
        }

        public async void setImage(int pos)
        {
            curIndex = pos;
            IRandomAccessStream inputstream = null;
            using (FileStream ImageFile = await Task.Run(() => new FileStream(ImagePaths[pos], FileMode.Open)))
            {
                await Task.Run(() =>
                {
                    inputstream = ImageFile.AsRandomAccessStream();
                });

                BitmapImage sourceImage = new BitmapImage();
                sourceImage.SetSource(inputstream);
                img.Source = sourceImage;

            }
        }
    }
}