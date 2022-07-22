using System;using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ShareX_windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            new Thread(delegate ()
            {
                //Utils.seachDevice(addToList);
                Utils.hostServer();
                string? rcv1 = Utils.receive_msg();
                if (rcv1 != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        received_label.Content = rcv1;
                    }), DispatcherPriority.Background);
                }

                while (true)
                {
                    var fileCount = int.Parse(Utils.receive_msg());

                    for (int i = 0; i < fileCount; i++)
                    {
                        Utils.receiveFile();

                    }

                    //string? rcv = Utils.receive_msg();


                }
            }).Start();
   

        }

        

        private void addToList(string toAdd)
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {
               /* listView.Items.Add(new ListViewItem()
                {
                    Content = toAdd
                });*/
            }), DispatcherPriority.Background);
           
        }

        private void Send_btn(object sender, RoutedEventArgs e)
        {
            /*Utils.send_msg(input_text.Text);
            input_text.Clear();*/
            Utils.receiveFile();
        }

        private void select_btn_Click(object sender, RoutedEventArgs e)
        {
            //Utils.sendFile();
        }
    }

}
