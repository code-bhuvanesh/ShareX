using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using ShareX_windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareX_windows.customWigets
{
    public sealed class FileItem : ListViewItem
    {
        public FileTrasferViewModel fViewModel;
        public FileItem()
        {
            this.DefaultStyleKey = typeof(FileItem);
        }

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(FileItem), new PropertyMetadata(""));

        public string FileSize
        {
            get { return (string)GetValue(FileSizeProperty); }
            set { SetValue(FileSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FileSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileSizeProperty =
            DependencyProperty.Register("FileSize", typeof(string), typeof(FileItem), new PropertyMetadata("0 KB"));

        public double progress
        {
            get { return (double)GetValue(progressProperty); }
            set
            {
                if(value <=100 && value >=0)
                {
                    SetValue(progressProperty, value);
                    SetValue(PercentageProperty, Math.Floor(value).ToString() + "%");
                }
                else if(value >100)
                {
                    SetValue(progressProperty, 100);
                }
                else
                {
                    SetValue(progressProperty, 0);
                }
              
            }
        }

        // Using a DependencyProperty as the backing store for value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty progressProperty =
            DependencyProperty.Register("progress", typeof(double), typeof(FileItem), new PropertyMetadata(0.0));


        public String Percentage
        {
            get { return ((string)GetValue(PercentageProperty) +"%"); }
            set { SetValue(PercentageProperty, value + "%" ); }
        }

        // Using a DependencyProperty as the backing store for per.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(String), typeof(FileItem), new PropertyMetadata("0%"));

    }


}
