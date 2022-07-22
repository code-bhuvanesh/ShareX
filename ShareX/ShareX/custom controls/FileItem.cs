using System;
using System.Collections.Generic;
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

namespace ShareX_windows
{
    public class FileItem : ListBoxItem
    {
        static FileItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileItem), new FrameworkPropertyMetadata(typeof(FileItem)));
        }



        public String fileName
        {
            get { return (String)GetValue(fileNameProperty); }
            set { SetValue(fileNameProperty, value); }
        }

        public static readonly DependencyProperty fileNameProperty =
            DependencyProperty.Register("fileName", typeof(String), typeof(FileItem), new PropertyMetadata(null));



        public String fileSize
        {
            get { return (String)GetValue(fileSizeProperty); }
            set { SetValue(fileSizeProperty, value); }
        }

        public static readonly DependencyProperty fileSizeProperty =
            DependencyProperty.Register("fileSize", typeof(String), typeof(FileItem), new PropertyMetadata(null));

    }
}
