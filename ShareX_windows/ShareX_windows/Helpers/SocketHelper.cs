using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ShareX_windows.Helpers
{
    public class SocketHelper
    {
        public Socket s;


        private int buffer = 1024 * 10000;
        private int port = 6578;

        public SocketHelper(int port = 6578)
        {
            this.port = port;
        }


        public static async Task<string> getIp()
        {
            return await Task.Run(() =>
            {
                var strIP = "";
                IPHostEntry HostEntry = Dns.GetHostEntry((Dns.GetHostName()));
                if (HostEntry.AddressList.Length > 0)
                {
                    foreach (IPAddress ip1 in HostEntry.AddressList)
                    {
                        if (ip1.AddressFamily == AddressFamily.InterNetwork)
                        {
                            Debug.WriteLine($"device ip is {ip1.ToString()}      ......................");
                            var check = ip1.ToString().Substring(ip1.ToString().LastIndexOf("."));
                            if (check != "1")
                            {
                                strIP = ip1.ToString();
                            }
                            break;
                        }
                    }
                }
                return strIP;
            });
        }

        Socket listener;
        IPAddress ip;
        IPEndPoint localEndPoint;
        public async Task hostServer()
        {


            await Task.Run(async () =>
            {
          
                //IPAddress ip = IPAddress.Parse("192.168.29.180");
                //string mobileSavedIp = SettingsHelper.read_Setting(Properties.Settings1.Default, "MobileIp");
                //strIP = mobileSavedIp == "" ? strIP : mobileSavedIp;
                //Debug.WriteLine($"mobileIP read is  {SettingsHelper.read_Setting(Properties.Settings1.Default, "MobileIp")}, {strIP}");
                if(listener == null)
                {
                    var strIP = await getIp();
                    Debug.WriteLine($"got ip is {strIP}, port {port}");
                    ip = IPAddress.Parse(strIP);
                    localEndPoint = new IPEndPoint(ip, port);
                    listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    listener.Bind(localEndPoint);
                }

                try
                {
                    listener.Listen(10);
                    s = listener.Accept();
                    var connectedIp = s.RemoteEndPoint.ToString();
                    connectedIp = connectedIp.Substring(0, connectedIp.IndexOf(":"));
                    Debug.WriteLine($"connection port is {port}, connected ip is {connectedIp}, {s != null}");
                    /*//if (mobileSavedIp != "" && mobileSavedIp != connectedIp)
                    //{
                    //    s.Close();
                    //    listener.Close();
                    //    await hostServer();
                    //    Debug.WriteLine("host server started again");
                    //    return;
                    //}
                    //SettingsHelper.save_Setting(Properties.Settings1.Default, "MobileIp", connectedIp);
                    //Debug.WriteLine($"mobileIP 2 read is  {read_Setting("MobileIp")}....");

                    //    await Task.Run(() =>
                    //    {
                    //        while (true)
                    //        {
                    //            try
                    //            {
                    //                var isConnected = !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
                    //                return isConnected;
                    //            }
                    //            catch (SocketException e)
                    //            {
                    //                Debug.WriteLine("connecteion error : " + e.Message);
                    //            }
                    //        }
                    //    });*/
                }
                catch (SocketException e)
                {
                    Debug.WriteLine($"failed to connect port {port} \n" + e.Message);
                }


                //var name = GetHostName(mobileIp);
                //Debug.WriteLine($"ip is {mobileIp} name is {name}");
            });

        }

        private async Task<string> GetHostName(string ipAddress)
        {
            return await Task.Run(() =>
             {
                 try
                 {
                     IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                     if (entry != null)
                     {
                         Debug.WriteLine("getHotsName : " + entry.HostName);
                         return entry.HostName;
                     }
                 }
                 catch (SocketException ex)
                 {
                     Debug.WriteLine("getHotsName : " + ex.Message);
                 }

                 return null;
             });

        }

        public async Task<string> receiveMsg()
        {
            return await Task.Run(async () =>
            {
                try
                {
                    String rcv = null;
                    if (s != null)
                    {
                        if (port == 3267)
                        {
                            Debug.WriteLine($"s : {s != null} for port {port}");
                        }
                        byte[] rcvLenBytes = new byte[4];
                        s.Receive(rcvLenBytes);
                        //Debug.WriteLine("s : " + (s != null));
                        int rcvLen = System.BitConverter.ToInt32(rcvLenBytes, 0);
                        byte[] rcvBytes = new byte[rcvLen];
                        s.Receive(rcvBytes);
                        rcv = System.Text.Encoding.ASCII.GetString(rcvBytes);
                    }
                   
                    return rcv;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("error receiving message");
                    Debug.WriteLine(e.Message);
                    await hostServer();
                    return await receiveMsg();
                }
            });



        }
        public async Task send_msg(string send_msg)
        {

            await Task.Run(async () =>
            {
                try
                {
                    string toSend = send_msg;

                    //sending
                    int toSendLen = System.Text.Encoding.ASCII.GetByteCount(toSend);
                    byte[] toSendBytes = System.Text.Encoding.ASCII.GetBytes(toSend);
                    //Debug.WriteLine($"send len bytes {toSendBytes.Length}");
                    //Debug.WriteLine("ascii value is " + toSendBytes.ToString());
                    byte[] toSendLenBytes = System.BitConverter.GetBytes(toSendLen);
                    s.Send(toSendLenBytes);
                    s.Send(toSendBytes);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("error sending message");
                    Debug.WriteLine(e.Message);
                    await hostServer();
                }
            });


        }

        public async Task sendFile(String filepath, Action<double> updateProgress = null)
        {
            try
            {
                if (filepath != null)
                {
                    string Path = filepath.Substring(0, filepath.LastIndexOf("\\") + 1);
                    string fileName = filepath.Substring(filepath.LastIndexOf("\\") + 1);

                    /*Debug.WriteLine("path: " + Path + "................");*/
                    //Debug.WriteLine("fileName: " + fileName + "................");

                    await send_msg(fileName);

                    var fileContent = File.OpenRead(filepath);
                    long fileSize = fileContent.Length;
                    //Debug.WriteLine($"file size {fileContent.Length}");

                    await send_msg(fileSize.ToString());

                    byte[] bytes = new byte[buffer];

                    var fileRead = 0;

                    while (fileRead < fileSize)
                    {
                        await Task.Run(() =>
                        {
                            var bytesRead = fileContent.Read(bytes);
                            s.Send(bytes, 0, bytesRead, SocketFlags.None);
                            fileRead += bytesRead;
                        });

                        var completed = Math.Round(((fileRead * 1.0) / fileSize) * 100, 0);
                        if (completed > 100.0)
                            completed = 100.00;
                        if (updateProgress != null)
                        {
                            updateProgress(completed);
                        }

                    }
                    //send_msg(fileName);
                    var status = await receiveMsg();
                    //Debug.WriteLine($"file {status}");
                    await send_msg(fileName);
                }
            }
            catch (SocketException e)
            {
                Debug.WriteLine($"error sending file {e.Message}");
                await hostServer();
            }

        }

        public async Task<List<List<string>>> filesToReceive(int fileCount)
        {

            return await Task.Run(async () =>
            {
                var filesList = new List<List<string>>();
                for (int i = 0; i < fileCount; i++)
                {
                    var file = new List<string>();
                    file.Add(await receiveMsg());
                    file.Add(await receiveMsg());
                    filesList.Add(file);
                }
                return filesList;
            });

        }

        public byte[] reciveBitmap()
        {
            var bitmapSize = int.Parse(receiveMsg().Result);
            Debug.WriteLine("byte size " + bitmapSize);
            byte[] bitmapBytes = new byte[bitmapSize];
            s.Receive(bitmapBytes, 0 ,bitmapSize ,SocketFlags.None);
            return bitmapBytes;
    
        }


        public async Task<String> receiveFile(string filePath1 = "D:\\ShareX\\", Action<double> updateProgress = null)
        {

            var filePath = filePath1;
            var fileName = await receiveMsg();
            fileName = FileHandler.ReplaceInvalidChars(fileName);
            var size = await receiveMsg();
            //Debug.WriteLine($"file name : {fileName}, fileSize : {size}");
            if ((fileName != null && fileName != "") && (size != null && size != ""))
            {
                var fileSize = long.Parse(size);

                await Task.Run(() =>
                {
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                });


                var file = File.OpenWrite(filePath + fileName);

                var bytes = new byte[buffer];

                var fileRead = 0;

                while (fileRead < fileSize)
                {
                    await Task.Run(() =>
                    {
                        var bytesRead = s.Receive(bytes);
                        file.Write(bytes, 0, bytesRead);
                        fileRead += bytesRead;

                    });

                    var completed = Math.Round(((fileRead * 1.0) / fileSize) * 100, 2);
                    if (completed > 100.0)
                        completed = 100.00;
                    if (updateProgress != null)
                    {
                        updateProgress(completed);
                    }
                }
                //Debug.WriteLine("received file");
                await send_msg("completed");
                file.Close();
                return filePath + fileName;
            }
            return null;


        }



        public async Task stopReceving()
        {
            await Task.Run(() =>
            {
                s.Blocking = false;
                s.Blocking = true;
            });


        }
        public async Task disconnect()
        {
            await Task.Run(() =>
            {
                try
                {
                    //if(listener != null)
                    //{
                    //    listener.Dispose();
                    //    listener = null;
                    //}
                    //s.Dispose();
                    //s.Close();
                    s.Shutdown(SocketShutdown.Receive);
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e);
                }
                
            });
        }


    }
}
