using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using QRCoder;
using Windows.Storage.Streams;
using System.Windows.Media.Imaging;

namespace ShareX_windows
{
    public static class Utils
    {
        private static byte[] bytes;
        private static IPAddress? ipAddress;
        private static IPEndPoint? remoteEP;
        private static Socket s;
        private static List<string> deviceList = new List<string>();
        private static List<Thread> ipThread = new List<Thread>();

        private static int buffer = 1024 * 10000;


        public static string getIp()
        {
            var strIP = "";

            IPHostEntry HostEntry = Dns.GetHostEntry((Dns.GetHostName()));
            if (HostEntry.AddressList.Length > 0)
            {
                foreach (IPAddress ip1 in HostEntry.AddressList)
                {
                    if (ip1.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //Debug.WriteLine($"device ip is {ip1.ToString()}      ......................");
                        var check = ip1.ToString().Substring(ip1.ToString().LastIndexOf("."));
                        if (check != "1")
                        {
                            strIP = ip1.ToString();
                        }
                    }
                }
            }
            return strIP;
        }


        public static void hostServer()
        {
            var strIP = getIp();


            //IPAddress ip = IPAddress.Parse("192.168.29.180");
            Debug.WriteLine($"strip {strIP}");
            IPAddress ip = IPAddress.Parse(strIP);
            IPEndPoint localEndPoint = new IPEndPoint(ip, 6578);

            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Debug.WriteLine($"connected 0.........");

                listener.Bind(localEndPoint);
                Debug.WriteLine($"connected 1.........");

                
                listener.Listen(10);
                Debug.WriteLine($"connected 2.........");

                s = listener.Accept();

                Debug.WriteLine($"connected 3.........");
            }
            catch(SocketException e)
            {
                Debug.WriteLine("failed to connect  " + e.Message);
            }
            //var mobileIp = s.RemoteEndPoint.ToString();
            //mobileIp = mobileIp.Substring(0, mobileIp.IndexOf(":"));
            //var name = GetHostName(mobileIp);
            //Debug.WriteLine($"ip is {mobileIp} name is {name}");
            //return mobileIp;
        }



        static string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }
            Console.WriteLine($"ip address is : {ip}");
            return ip;
        }



        public static void seachDevice(Action<string> toAdd)
        {
            //try
            //{
            //    string gate_ip = NetworkGateway();
            //    string[] array = gate_ip.Split('.');

            //    for (int i = 1; i < 255; i++)
            //    {
            //        string ip = array[0] + "." + array[1] + "." + array[2] + "." + i;
            //        Thread t = new Thread(delegate ()
            //        {
            //            Debug.WriteLine("ip is ....................... " + ip);
            //            try
            //            {
            //                bool connected = tryToConnect(ip);
            //                if (connected)
            //                    toAdd(ip);
            //            }
            //            catch
            //            {
            //            }
            //        });
            //        ipThread.Add(t);
            //        //createSearchThread(ping_var, toAdd);
            //    }

            //    ipThread.ForEach(x => { x.Start(); });
            //    System.Threading.Thread.Sleep(5);
            //    ipThread.ForEach(x => { x.Abort(); });
            //    Debug.WriteLine("thread completed");
            //}
            //catch
            //{

            //}
        }

        public static string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException ex)
            {
                
            }

            return null;
        }

        public static String? receive_msg()
        {
            try
            {
                if(s != null)
                {
                    byte[] rcvLenBytes = new byte[4];
                    s.Receive(rcvLenBytes);
                    Debug.WriteLine("s : " + (s != null));
                    int rcvLen = System.BitConverter.ToInt32(rcvLenBytes, 0);
                    byte[] rcvBytes = new byte[rcvLen];
                    s.Receive(rcvBytes);
                    String rcv = System.Text.Encoding.ASCII.GetString(rcvBytes);
                    return rcv;
                }
                return "0";


            }
            catch (Exception e)
            {
                Debug.WriteLine("error receiving message");
                Debug.WriteLine(e.Message);
                return null;
            }

        }
        public static void send_msg(string send_msg)
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
            }

        }

        public static OpenFileDialog openFile()
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Multiselect = true;
            if(file.ShowDialog() == true)
            {
                return file;
            }
            return null;

        }

        public static void sendFile(String filepath, Action<double, FileItem> updateProgress, FileItem fileItem)
        {
            if(filepath != null)
            {
                string Path = filepath.Substring(0, filepath.LastIndexOf("\\") + 1);
                string fileName = filepath.Substring(filepath.LastIndexOf("\\") + 1);

                /*Debug.WriteLine("path: " + Path + "................");
                Debug.WriteLine("fileName: " + fileName + "................");*/

                send_msg(fileName);

                var fileContent = File.OpenRead(filepath);
                long fileSize = fileContent.Length;
                Debug.WriteLine($"file size {fileContent.Length}");

                send_msg(fileSize.ToString());

                byte[] bytes = new byte[buffer];

                var fileRead = 0;

                while( fileRead < fileSize)
                {
                    Debug.WriteLine($"file asdassdkjhfklsdjgklsdklfhdsklfjdsklfkldsjfkdklfjdklfj");
                    var bytesRead = fileContent.Read(bytes);
                    Debug.WriteLine($"file asdassdkjhfklsdjgklsdklfhdsklfjdsklfkldsjfkdklfjdklfj");
                    s.Send(bytes, 0, bytesRead, SocketFlags.None);
                    fileRead += bytesRead;
                    var completed = Math.Round(((fileRead * 1.0) / fileSize) * 100, 2);
                    if (completed > 100.0)
                        completed = 100.00;
                    updateProgress(completed, fileItem);
                    
                }
                //send_msg(fileName);
                var status = receive_msg();
                Debug.WriteLine($"file {status}");
                Utils.send_msg(fileName);

            }
        }

        public static void receiveFile(Action<string,string, ListBox> addItem,Action<double, FileItem> updateProgress, ListBox filesList,int i)
        {

            var filePath = "D:\\ShareX\\";
            var fileName = receive_msg();
            var size = receive_msg();
            Debug.WriteLine($"file name : {fileName}, fileSize : {size}");
            if ((fileName != null && fileName != "" )&& (size != null && size != ""))
            {
                var fileSize = long.Parse(size);

                if(!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                addItem(fileName, getFileSize(fileSize),filesList);
                Thread.Sleep(20);
                var file = File.OpenWrite(filePath + fileName);

                var bytes = new byte[buffer];

                var fileRead = 0;

                while (fileRead < fileSize)
                {
                    var bytesRead = s.Receive(bytes);
                    file.Write(bytes, 0, bytesRead);
                    fileRead += bytesRead;

                    var completed = Math.Round(((fileRead * 1.0) / fileSize) * 100, 2);
                    if (completed > 100.0)
                        completed = 100.00;
                    //update UI
                    //dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    received_label.Content = completed.ToString();
                    //}), DispatcherPriority.Background);
                    updateProgress(completed, filesList.Items[i] as FileItem);
                    Debug.WriteLine($"completed {completed}");
                }
                Debug.WriteLine("received file");
                send_msg("completed");
                file.Close();
            }
            
        }

        public static string getFileName(string filePath)
        {
            string name = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            
            if(name.Length > 30)
            {
                return name.Substring(0, 30) + ".....";
            }

            return name;
        }

        public static String getFileSize(long size)
        {
           
            if (size > 1069547520)
            {
                return Math.Round((size * 1.0 / 1069547520),2).ToString() + " GB";
            }
            else if (size > 1048576)
            {
                return Math.Round((size * 1.0 / 1048576), 2).ToString()+ " MB";
            }
            else if (size > 1024)
            {
                return Math.Round((size * 1.0/ 1024),2).ToString()+ " KB";
            }
            else
            {
                return size.ToString() + " B";
            }

        }

        public static void stopReceving()
        {
            s.Blocking = false;
            s.Blocking = true;
        }
        public static void disconnect()
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }
    }
}
