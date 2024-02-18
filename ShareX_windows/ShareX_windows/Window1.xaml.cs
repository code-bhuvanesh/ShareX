using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ShareX_windows.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using System.Runtime.InteropServices; // For DllImport
using WinRT; // required to support Window.As<ICompositionSupportsSystemBackdrop>()
using ShareX_windows.Helpers;
using QRCoder;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareX_windows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Window1 : Window
    {
        private PhotosView navPhotos;
        private FileTransferView navFileTransfer;
        private MessagesView navMessages;
        private NotificationsView navNotifications;
        private CallsView navCalls;
        private QrConnectView showQrCode;
        public Window1()
        {
            this.InitializeComponent();

            SetTitleBar(AppTitleBar);

            //initialiseViews
            navPhotos = new PhotosView(navBar);
            navFileTransfer = new FileTransferView();
            navMessages = new MessagesView();
            navNotifications = new NotificationsView();
            navCalls = new CallsView();
            Debug.WriteLine($"socket name is {(int)SocketMode.SendingFile}");

            connectMainSocket();


        }


        private async void connectMainSocket()
        {
            Debug.WriteLine(await SocketHelper.getIp());
            showQrCode = new QrConnectView(await SocketHelper.getIp());
            navBar.Content = showQrCode;
            navBar.IsPaneOpen = false;
            navBar.IsEnabled = false;
            await MainSocket.Instance.connect();
            HeartSocket.Instance.initializeDisconnection(disconnectAllSockets);
            await HeartSocket.Instance.connect();
            navBar.IsEnabled = true;
            navBar.IsPaneOpen = true;
            //navBar.Content = navNotifications;
            navBar.Content = navPhotos;

        }

        public async void disconnectAllSockets()
        {
            navPhotos.pViewModel.disconect();
        }


        private void navBar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selected = navBar.SelectedItem as NavigationViewItem;
            var navNo = selected.Tag;

            switch (navNo)
            {
                case "0":
                    //navFrame.Navigate(typeof(PhotosView),navPhotos);
                    sender.Content = navPhotos; 
                    break;
                case "1":
                    //navFrame.Navigate(typeof(FileTransferView),navFileTransfer);
                    sender.Content = navFileTransfer;
                    break;
                case "2":
                    //navFrame.Navigate(typeof(MessagesView), navMessages);
                    sender.Content = navMessages;
                    break;
                case "3":
                    //navFrame.Navigate(typeof(NotificationsView), navNotifications);
                    sender.Content = navNotifications;
                    break;
                case "4":
                    //navFrame.Navigate(typeof(CallsView), navCalls);
                    sender.Content = navCalls;
                    break;
                default:
                    break;

            }
        }
    }
}
