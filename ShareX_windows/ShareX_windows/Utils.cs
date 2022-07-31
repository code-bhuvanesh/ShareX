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
using System.Configuration;

namespace ShareX_windows
{
    public class Utils
    {
        private byte[] bytes;
        private IPAddress? ipAddress;
        private IPEndPoint? remoteEP;
        private Socket s;
        private List<string> deviceList = new List<string>();
        private List<Thread> ipThread = new List<Thread>();

        private int buffer = 1024 * 10000;
        private int port = 6578;

        public Utils(int port = 6578)
        {
            this.port = port;
        }


        public string getIp()
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


        public void hostServer()
        {
            var strIP = getIp();


            //IPAddress ip = IPAddress.Parse("192.168.29.180");
            string mobileSavedIp = read_Setting("MobileIp");
            strIP = mobileSavedIp == "" ? strIP : mobileSavedIp;
            Debug.WriteLine($"mobileIP read is  {read_Setting("MobileIp")}, {strIP}");
            IPAddress ip = IPAddress.Parse(strIP);
            IPEndPoint localEndPoint = new IPEndPoint(ip, port);
            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                s = listener.Accept();
                var connectedIp = s.RemoteEndPoint.ToString();
                connectedIp = connectedIp.Substring(0, connectedIp.IndexOf(":"));
                Debug.WriteLine($"connection port is {port}, connected ip is {connectedIp}");
                if (mobileSavedIp != "" && mobileSavedIp != connectedIp)
                {
                    s.Close();
                    listener.Close();
                    hostServer();
                    Debug.WriteLine("host server started again");
                    return;
                }
                save_Setting("MobileIp", connectedIp);
                //Debug.WriteLine($"mobileIP 2 read is  {read_Setting("MobileIp")}....");
                
                new Thread(delegate()
                    {
                        while(true)
                        {
                            try
                            {
                                var isConnected = !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
                            }
                            catch (SocketException e) 
                            { 
                                Debug.WriteLine("connecteion error : " + e.Message);
                                break;
                            }
                        }
                        
                    }).Start();



            }
            catch(SocketException e)
            {
                Debug.WriteLine("failed to connect  " + e.Message);
            }
           

            //var name = GetHostName(mobileIp);
            //Debug.WriteLine($"ip is {mobileIp} name is {name}");
        }



        string NetworkGateway()
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



        public void seachDevice(Action<string> toAdd)
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

        public string GetHostName(string ipAddress)
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

        public String? receive_msg()
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
        public void send_msg(string send_msg)
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

        public OpenFileDialog openFile()
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Multiselect = true;
            if(file.ShowDialog() == true)
            {
                return file;
            }
            return null;

        }

        public void sendFile(String filepath, Action<double, FileItem> updateProgress, FileItem fileItem)
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
                send_msg(fileName);

            }
        }

        public void receiveFile(Action<string,string, ListBox> addItem,Action<double, FileItem> updateProgress, ListBox filesList,int i,string filePath1 = "D:\\ShareX\\")
        {

            var filePath = filePath1;
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

        public String? receiveFile(string filePath1 = "D:\\ShareX\\")
        {

            var filePath = filePath1;
            var fileName = receive_msg();
            var size = receive_msg();
            Debug.WriteLine($"file name : {fileName}, fileSize : {size}");
            if ((fileName != null && fileName != "") && (size != null && size != ""))
            {
                var fileSize = long.Parse(size);

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

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
                }
                Debug.WriteLine("received file");
                send_msg("completed");
                file.Close();
                return filePath + fileName;
            }
            return null;

        }

        public string getFileName(string filePath)
        {
            string name = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            
            if(name.Length > 30)
            {
                return name.Substring(0, 30) + ".....";
            }

            return name;
        }

        public String getFileSize(long size)
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

        public void stopReceving()
        {
            s.Blocking = false;
            s.Blocking = true;
        }
        public void disconnect()
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }

        public void save_Setting(string setting_Name, string setting_Value)

        {
            string property_name = setting_Name;

            SettingsProperty prop = null;

            if (Properties.Settings1.Default.Properties[property_name] != null)
            {
                prop = Properties.Settings1.Default.Properties[property_name];
            }

            else
            {
                prop = new System.Configuration.SettingsProperty(property_name);
                prop.PropertyType = typeof(string);
                Properties.Settings1.Default.Properties.Add(prop);
                Properties.Settings1.Default.Save();
            }
            Properties.Settings1.Default.Properties[property_name].DefaultValue = setting_Value;

            Properties.Settings1.Default.Save();

        }



        public string read_Setting(string setting_Name)
        {
            string sResult = "";

            if (Properties.Settings1.Default.Properties[setting_Name] != null)
            {
                sResult = Properties.Settings1.Default.Properties[setting_Name].DefaultValue.ToString();
            }

            if (sResult == "NaN") sResult = "0";

            return sResult;
        }

    }

}
