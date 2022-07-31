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
    public partial class Window1 : Window
    {
        private Window2 qrWindow;
        private Utils utils;


        //nav pages
        private Object navPhotos;
        private Object navFileTransfer;
        private Object navMessages;
        private Object navNotifications;
        private Object navCallLogs;

        public Window1()
        {
            InitializeComponent();

            var mobileConnected = false;

            utils = new Utils();

            navPhotos = new Pages.PhotosPage();
            navFileTransfer = new Pages.FileTransferPage(utils);
            navMessages = new Pages.MessagesPage();
            navNotifications = new Pages.NotificationPage();
            navCallLogs = new Pages.CallLogsPage();


            connectionStatus.MouseLeftButtonDown += Window_Loaded;


            var navButton = navBar.Items.GetItemAt(0) as NavButton;
            navBar.SelectedItem = navButton;    
            //navFrame.Navigate(navButton.Navlink,utils);
            navFrame.Navigate(navPhotos);
            var serverThread = new Thread(delegate ()
            {
                //utils.seachDevice(addToList);
                utils.hostServer();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (qrWindow != null)
                        qrWindow.Close();
                    connectionStatus.Text = "connected";
                    mobileConnected = true;
                }), DispatcherPriority.Normal);
            });
            serverThread.IsBackground = true;
            serverThread.Start();
        }


        private void navBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = navBar.SelectedItem as NavButton;
            var navNo = selected.Navlink;

            switch (navNo)
            {
                case 0:
                    navFrame.Navigate(navPhotos);
                    break;
                case 1:
                    navFrame.Navigate(navFileTransfer);
                    break;
                case 2:
                    navFrame.Navigate(navMessages);
                    break;
                case 3:
                    navFrame.Navigate(navNotifications);
                    break;
                case 4:
                    navFrame.Navigate(navCallLogs);
                    break;
                default:
                    break;

            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            qrWindow = new Window2(utils.getIp());
            qrWindow.Owner = this;
            qrWindow.ShowDialog();

        }

       
    }
}
