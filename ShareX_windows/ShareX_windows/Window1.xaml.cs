using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ShareX_windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Window2 qrWindow;
        public Window1()
        {
            InitializeComponent();

            var mobileConnected = false;

            connectionStatus.MouseLeftButtonDown += Window_Loaded;


            var navButton = navBar.Items.GetItemAt(1) as NavButton;
            navFrame.Navigate(navButton.Navlink);
            new Thread(delegate ()
            {
                //Utils.seachDevice(addToList);
                Utils.hostServer();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if(qrWindow != null)
                        qrWindow.Close();
                    connectionStatus.Text = "connected";
                    mobileConnected = true;
                }), DispatcherPriority.Normal);
            }).Start();

            



        }


        private void navBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = navBar.SelectedItem as NavButton;

            navFrame.Navigate(selected.Navlink);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            qrWindow = new Window2(Utils.getIp());
            qrWindow.Owner = this;
            qrWindow.ShowDialog();

        }
    }
}
