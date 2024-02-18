using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ShareX_windows.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareX_windows.Views
{
    public sealed partial class PhotosView : UserControl
    {
        public PhotosViewModel pViewModel;
        public PhotosView(NavigationView nav)
        {
            this.InitializeComponent();
            Nav = nav;
            pViewModel = new PhotosViewModel(Nav, this);
        }

        public NavigationView Nav { get; }
    }
}
