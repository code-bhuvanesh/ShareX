using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using ShareX_windows.customWigets;
using ShareX_windows.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Input;

namespace ShareX_windows.ViewModels
{
    public class FileTrasferViewModel : ViewModelBase
    {
        public ObservableCollection<FileItem> filesToSend;
        public ObservableCollection<FileItem> filesToReceive;

        private FileItem cSendingFile;
        private FileItem cReceivingFile;

        private List<string> filesSelected;

        private SocketHelper sSocket;
        private SocketHelper rSocket;

        private int sPort = 3233;
        private int rPort = 3243;

        private int noOfReceivingFiles = 0;
        private int noOfSendingFiles = 0;

        private bool sending = false;
        private bool receiving = false;

        private string sendBtnName = "send";
        public string sendBtnNameProperty
        {
            get
            {
                return sendBtnName;
            }
            set
            {
                sendBtnName = value;
                OnPropertyChanged(nameof(sendBtnNameProperty));
            }
        }

        private string receiveBtnName = "receive";
        public string receiveBtnNameProperty
        {
            get
            {
                return receiveBtnName;
            }
            set
            {
                receiveBtnName = value;
                OnPropertyChanged(nameof(receiveBtnNameProperty));
            }
        }

        public FileTrasferViewModel()
        {
            mainMethod();
        }

        private async void mainMethod()
        {
            filesSelected = new List<string>();
            filesToSend = new ObservableCollection<FileItem>();
            filesToReceive = new ObservableCollection<FileItem>();


            rSocket = new SocketHelper(rPort);
            sSocket = new SocketHelper(sPort);

            await rSocket.hostServer();

            await sSocket.hostServer();

            MainSocket.Instance.receiveFileCallback(_ReceiveFiles);
        }


        public ICommand SelectFiles => new RelayCommand(_SelectFiles);
        private void _SelectFiles()
        {
            OpenFileDialog file = FileHandler.openFile();
            if (file != null)
            {
                String[] filePath = file.FileNames;
                filesSelected.Clear();
                for (int i = 0; i < filePath.Length; i++)
                {
                    filesSelected.Add(filePath[i]);
                    string fileName = FileHandler.getFileName(filePath[i]);
                    string fileSize = FileHandler.getFileSize(file.OpenFiles()[i].Length);

                    filesToSend.Add(new FileItem
                    {
                        FileName = fileName,
                        FileSize = fileSize
                    });
                }
            }


        }

        public ICommand SendFiles => new RelayCommand(_SendFiles);
        private async void _SendFiles()
        {
            var transfering = SettingsHelper.read_Setting(Properties.Settings1.Default, "transfering");
            if(transfering == "true")
            {
                Debug.WriteLine("cannot send file Files are receiving");
                return;
            }
           
            try
            {
                if (sending)
                    return;
                MainSocket.Instance.sendMsg(SocketMode.SendingFile);
                sending = true;
                sendBtnNameProperty = "sending";
                var filesCount = filesSelected.Count;
                await sSocket.send_msg(filesCount.ToString());
                for (int i = 0; i < filesCount; i++)
                {
                    cSendingFile = filesToSend[noOfSendingFiles];
                    await sSocket.sendFile(filesSelected[i], sUpdateFileProgress);
                    noOfSendingFiles++;
                }
                cSendingFile = null;
                sending = false;
                sendBtnNameProperty = "send";
            }
            catch(SocketException e)
            {
                Debug.WriteLine($"send socket error {e.Message}");
                filesToReceive.RemoveAt(noOfSendingFiles);
                await sSocket.hostServer();
                return;
            }
           
        }

        public ICommand ReceiveFiles => new RelayCommand(_ReceiveFiles);
        private async void _ReceiveFiles()
        {
          
            try
            {
                if (receiving)
                    return;
                SettingsHelper.save_Setting(Properties.Settings1.Default, "transfering", "true");
                receiving = true;
                receiveBtnNameProperty = "receiving";
                Debug.WriteLine($"file receiving.......................");
                string fileCountStr = null;
                fileCountStr = await rSocket.receiveMsg();
                Debug.WriteLine($"file count {fileCountStr}");
                var fileCount = 0;
                if (int.TryParse(fileCountStr, out fileCount))
                {
                    var filesList = await rSocket.filesToReceive(fileCount);
                    filesList.ForEach(f =>
                    {
                        filesToReceive.Add(new FileItem()
                        {
                            FileName = f[0],
                            FileSize = f[1],
                        });
                        Debug.WriteLine($"1 files to receive size {filesToReceive.Count}");
                    });
                    await rSocket.send_msg("success");
                    Debug.WriteLine($"2 files to receive size {filesToReceive.Count}");
                    for (int i = 0; i < fileCount; i++)
                    {
                        cReceivingFile = filesToReceive[noOfReceivingFiles];
                        await rSocket.receiveFile(updateProgress: rUpdateFileProgress);
                        noOfReceivingFiles++;
                    }
                }
                else
                {
                    await rSocket.send_msg("error");
                    _ReceiveFiles();

                }
                receiving = false;
                receiveBtnNameProperty = "receive";
            }
            catch(System.Net.Sockets.SocketException e)
            {
                Debug.WriteLine(e.ToString());
                filesToReceive.RemoveAt(noOfReceivingFiles);
                await rSocket.hostServer();
                return;

            }
            SettingsHelper.save_Setting(Properties.Settings1.Default, "transfering", "false");


        }

        private void sUpdateFileProgress(double completed)
        {
            if (cSendingFile != null)
            {
                cSendingFile.progress = completed;
            }


        }
        private void rUpdateFileProgress(double completed)
        {
            if (cReceivingFile != null)
            {
                cReceivingFile.progress = completed;
            }
        }

        public async void disconect()
        {
            try
            {
                await rSocket.disconnect();
                await sSocket.disconnect();
            }
            catch
            {

            }
            
        }

    }
}
