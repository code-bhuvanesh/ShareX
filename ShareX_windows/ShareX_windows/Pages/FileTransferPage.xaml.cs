using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    public partial class FileTransferPage : Page
    {

        private List<string> files = new List<string>();
        private bool isReceiving = false;
        private bool isSending = false;

        private Utils utils;

        public FileTransferPage(Utils u)
        {
            InitializeComponent();
            utils = u;
        }

        public void selectFiles(Object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = utils.openFile();
            if(file != null)
            {
                String[] filePath = file.FileNames;
                for(int i = 0; i < filePath.Length; i++)
                {
                    files.Add(filePath[i]);
                    string fileName = utils.getFileName(filePath[i]);
                    string fileSize = utils.getFileSize(file.OpenFiles()[i].Length);

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
                utils.send_msg(filesCount.ToString());
                for (int i = 0; i < filesCount; i++)
                {
                    utils.sendFile(files[0], updateProgress, sendFilesList.Items[0] as FileItem);
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

        private Thread recevieThread;
        private void ReceiveFiles(object sender, RoutedEventArgs e)
        {
            if(!isReceiving)
            {
                isReceiving = true;
                receiveFilesBtn.ButtonText = "Receiving...";
                recevieThread = new Thread(delegate ()
                {
                    var msg = utils.receive_msg();
                    Debug.WriteLine($"file count {msg}");
                    var fileCount = 0;
                    if (int.TryParse(msg,out fileCount))
                    {
                        utils.send_msg("success");
                        for (int i = 0; i < fileCount; i++)
                        {
                            utils.receiveFile(addItem, updateProgress, receivedFilesList, i);

                        }
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            receiveFilesBtn.ButtonText = "Receive";
                        }), DispatcherPriority.Send);
                        isReceiving = true;
                    }
                    else
                    {
                        utils.send_msg("error");
                        ReceiveFiles(sender, e);

                    }
                    

                });
                recevieThread.Start();
            }
            else
            {
                recevieThread.Interrupt();
                receiveFilesBtn.ButtonText = "Receive";
                isReceiving = false;
            }


        }

        void updateProgress(double i, FileItem fileItem)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if(fileItem != null)
                {
                    fileItem.value = i;

                }

            }), DispatcherPriority.Send);
        }

        void addItem(string fileName, string fileSize, ListBox fileList)
        {
            if(fileName.Length >=30)
            {
                fileName = fileName.Substring(0,27) + "...";
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                fileList.Items.Add(new FileItem
                {
                    fileName = fileName,
                    fileSize = fileSize
                });
                var item = fileList.Items.GetItemAt(fileList.Items.Count - 1);
                receivedFilesList.ScrollIntoView(item);
            }), DispatcherPriority.Send);
        }

    }
}
