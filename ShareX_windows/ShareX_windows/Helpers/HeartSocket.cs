using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ShareX_windows.Helpers
{
    public class HeartSocket
    {
        private SocketHelper _mainSocket;

        private static HeartSocket instance = null;
        public static HeartSocket Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HeartSocket();
                }
                return instance;
            }
        }
        private static SocketHelper heartSocket;

        private HeartSocket()
        {
            heartSocket = new(1233);
        }
        System.Action disconnectAllSockets;
        public void initializeDisconnection(System.Action das)
        {
            disconnectAllSockets = das;
        }

        public async Task connect()
        {
            await heartSocket.hostServer();
            new Thread(async () =>
             {
                 var n = 0;
                 while (true)
                 {
                     try
                     {
                         Thread.Sleep(1000);
                         //var b = new byte[1] { 2 };
                         //heartSocket.s.Send(b, 1, System.Net.Sockets.SocketFlags.None);
                         //Debug.WriteLine($"heart socket read msg is {IsConnected(heartSocket.s)}");
                         //Debug.WriteLine($"heart socket read msg is {heartSocket.s.Receive(b, 1, System.Net.Sockets.SocketFlags.None)}");
                         //await heartSocket.send_msg(n.ToString());
                         //n++;
                         //Debug.WriteLine("heartSocket msg sent");
                         //await heartSocket.receiveMsg();
                         //Debug.WriteLine("heartSocket msg sent");
                         //Debug.WriteLine($"heart socket read msg is {await heartSocket.receiveMsg()}");
                         var heartSocketConnected = IsConnected(heartSocket.s);
                         Debug.WriteLine($"is connected {heartSocketConnected}");
                         if (!heartSocketConnected)
                         {
                             Debug.WriteLine($"heart socket disconnected");
                             await MainSocket.Instance.connect();
                             await connect();

                             disconnectAllSockets();
                         }


                     }
                     catch (SocketException e)
                     {
                         Debug.WriteLine($"heart Socket error is {e.Message}");
                         await MainSocket.Instance.connect();
                         disconnectAllSockets();

                     }

                 }
             }).Start();
        }

        public bool IsConnected(Socket socket)
        {
            try
            {
                bool part1 = socket.Poll(1000, SelectMode.SelectRead);
                bool part2 = (socket.Available == 0);
                if (part1 && part2)
                    return false;
                else
                    return true;
            }
            catch (SocketException) { return false; }
        }
    }
}
