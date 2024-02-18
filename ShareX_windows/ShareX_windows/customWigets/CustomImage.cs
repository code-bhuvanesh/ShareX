using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareX_windows.customWigets
{
    public sealed class CustomImage : Button
    {
        public CustomImage()
        {
            this.DefaultStyleKey = typeof(CustomImage);
        }
        public String imgPath
        {
            get { return (String)GetValue(imgPathProperty); }
            set { SetValue(imgPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for imgPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty imgPathProperty =
            DependencyProperty.Register("imgPath", typeof(String), typeof(CustomImage), new PropertyMetadata(""));



        public int pos
        {
            get { return (int)GetValue(posProperty); }
            set { SetValue(posProperty, value); }
        }

        // Using a DependencyProperty as the backing store for pos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty posProperty =
            DependencyProperty.Register("pos", typeof(int), typeof(CustomImage), new PropertyMetadata(null));

    }
}
