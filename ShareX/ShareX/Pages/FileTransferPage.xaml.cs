using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ShareX_windows.Pages
{
    /// <summary>
    /// Interaction logic for FileTransferPage.xaml
    /// </summary>
    public partial class FileTransferPage : Page
    {

        private List<string> files = new List<string>();
        private bool isReceiving = false;
        private bool isSending = false;


        public FileTransferPage()
        {
            InitializeComponent();
        }

        public void selectFiles(Object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = Utils.openFile();
            if(file != null)
            {
                String[] filePath = file.FileNames;
                for(int i = 0; i < filePath.Length; i++)
                {
                    files.Add(filePath[i]);
                    string fileName = Utils.getFileName(filePath[i]);
                    string fileSize = Utils.getFileSize(file.OpenFiles()[i].Length);

                    sendFilesList.Items.Add(new FileItem
                    {
                        fileName = fileName,
                        fileSize = fileSize
                    });
                }
            }
        

        }

        private void SendFiles(object sender, RoutedEventArgs e)
        {
            if (isSending)
                return;
            isSending = true;
            new Thread(delegate ()
            {
                var filesCount = files.Count;
                Utils.send_msg(filesCount.ToString());
                for (int i = 0; i < filesCount; i++)
                {
                    Utils.sendFile(files[0]);
                    files.RemoveAt(0);
                    Debug.WriteLine($"files sent {i}");
                    Dispatcher.BeginInvoke(new Action(() => 
                    {
                        sendFilesList.Items.RemoveAt(0);
                    }), DispatcherPriority.Send);
                }
            }).Start();
            isSending = false;

        }

        private void ReceiveFiles(object sender, RoutedEventArgs e)
        {
            if(!isReceiving)
            {
                isReceiving = true;
                receiveFilesBtn.ButtonText = "receiving...";
                new Thread(delegate ()
                {
                    var msg = Utils.receive_msg();
                    Debug.WriteLine($"file count {msg}");
                    var fileCount = int.Parse(msg);

                    for (int i = 0; i < fileCount; i++)
                    {
                        var file = Utils.receiveFile();
                        if (file != null)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                receivedFilesList.Items.Add(new FileItem
                                {
                                    fileName = Utils.getFileName(file.Name),
                                    fileSize = Utils.getFileSize(file.Length)
                                });
                                var item = receivedFilesList.Items.GetItemAt(receivedFilesList.Items.Count - 1);
                                receivedFilesList.ScrollIntoView(item);
                                file.Close();
                            }), DispatcherPriority.Background);


                        }

                    }
                }).Start();
            }
            receiveFilesBtn.ButtonText = "receive";


        }

    }
}
