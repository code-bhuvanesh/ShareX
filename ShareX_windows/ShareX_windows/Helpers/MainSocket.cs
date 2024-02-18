using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShareX_windows.Helpers
{


    public enum SocketMode
    {
        SendingFile,
        ReceivingFile,
        SendPhotos,
        ReceivePhotos,
        SendMessage,
        ReciveMessage,
        GetImage,
        Any
    }
    public class MainSocket
    {
        private Action receiveFileTask;
        private Action receiveNotificationTask;

        private static MainSocket instance = null;
        public static MainSocket Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new MainSocket();
                }
                return instance;
            }
        }
        private int mainSocketPort = 6774;
        private SocketHelper _mainSocket;
        private MainSocket()
        {
            _mainSocket = new(port: mainSocketPort);
        }

        public async Task<bool> connect()
        {
            Debug.WriteLine($"socket is waiting");
            await _mainSocket.hostServer();
            Debug.WriteLine($"socket is connected");
            ReceiveThread();
            return true;

        }

       

        public async void ReceiveThread()
        {
           
            SocketMode currentMode = SocketMode.Any;

            while (true)
            {
                try
                {
                    var rcvMsg = await _mainSocket.receiveMsg();
                    int rcvInt = -1;

                    if (rcvMsg != "" && rcvMsg != null)
                    {
                        if (int.TryParse(rcvMsg, out rcvInt))
                        {
                            currentMode = (SocketMode)rcvInt;
                            Debug.WriteLine($"socket mode is {currentMode}");

                            if (!Enum.IsDefined(typeof(SocketMode), currentMode) && !currentMode.ToString().Contains(","))
                            {
                                //throw new InvalidOperationException(
                                //    $"{recMsg} is not an underlying value of the YourEnum enumeration."
                                //);
                                continue;
                            }
                            switch (currentMode)
                            {
                                case SocketMode.SendingFile:
                                    if(receiveFileTask != null)
                                        receiveFileTask();
                                    break;
                                case SocketMode.ReceivingFile:
                                    break;
                                case SocketMode.SendPhotos:
                                    break;
                                case SocketMode.ReciveMessage:
                                    break;
                                case SocketMode.SendMessage:
                                    break;
                                case SocketMode.Any:
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            //Debug.WriteLine($"rcv msg is {rcvMsg}");
                        }
                    }
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("mainSocket receive Therad :" + e.Message);
                    await _mainSocket.hostServer();
                }
            }
        }

        public async void sendMsg(SocketMode mode)
        {
            Debug.WriteLine($"socket name {mode.ToString()}");
            var sendStr = mode.ToString();
            await _mainSocket.send_msg(sendStr);
        }

        public void receiveFileCallback(Action task)
        {   
            receiveFileTask =  task;
        }
        public void receiveNotificationCallback(Action task)
        {   
            receiveNotificationTask =  task;
        }
    }
}
