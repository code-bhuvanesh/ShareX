using MahApps.Metro.IconPacks;
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

    public class TranslucentButton : Button
    {
        static TranslucentButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TranslucentButton), new FrameworkPropertyMetadata(typeof(TranslucentButton)));
        }

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(TranslucentButton), new PropertyMetadata(null));

        public PackIconMaterialKind ButtonIcon
        {
            get { return (PackIconMaterialKind)GetValue(ButtonIconProperty); }
            set { SetValue(ButtonIconProperty, value); }
        }

        public static readonly DependencyProperty ButtonIconProperty =
            DependencyProperty.Register("ButtonIcon", typeof(PackIconMaterialKind), typeof(TranslucentButton), new PropertyMetadata(null));

    }
}
