using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareX_windows.Models
{
    public class ImageFullScreenViewModel
    {
        public List<string> ImagePath { get; }
        public int CurPos { get;}

        public ObservableCollection<Image> imagesList;

        public ImageFullScreenViewModel(List<string> imagePath, int curPos)
        {
            ImagePath = imagePath;
            CurPos = curPos;
        }


        
    }
}
