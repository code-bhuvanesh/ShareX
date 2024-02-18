using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using ShareX_windows.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ShareX_windows.ViewModels
{
    public class NotificationViewModel : ViewModelBase
    {
        private int port = 5467;
        private SocketHelper nSockethelper;
        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        private string applicationName;
        public string applicationNameProperty
        {
            get
            {
                return applicationName;
            }
            set
            {
                applicationName = value;
                OnPropertyChanged(nameof(applicationNameProperty));
            }
        }


        private string tittle;
        public string titleProperty
        {
            get
            {
                return tittle;
            }
            set
            {
                tittle = value;
                OnPropertyChanged(nameof(titleProperty));
            }
        }

        private string content;
        public string contentProperty
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                OnPropertyChanged(nameof(contentProperty));
            }
        }


        private BitmapSource iconBitmap;
        public BitmapSource iconBitmapProperty
        {
            get
            {
                return iconBitmap;
            }
            set
            {
                iconBitmap = value;
                OnPropertyChanged(nameof(iconBitmapProperty));
            }
        }


        public NotificationViewModel()
        {
            nSockethelper = new SocketHelper(port);
            mainTask();
        }

        public async void mainTask()
        {
            await nSockethelper.hostServer();
            new Thread(() =>
            {
                while (true)
                {
                    receiveNotification();
                }
            }).Start();
            MainSocket.Instance.receiveNotificationCallback(receiveNotification);
        }


        bool recivingNotification = false;
        public async void receiveNotification()
        {
            BitmapImage appIconBitmap = null;

            //add receiving feature for get bitmap icons
            if (nSockethelper.receiveMsg().Result == "true")
            {
                var bitmapBytes = nSockethelper.reciveBitmap();
                Debug.WriteLine("bytes size = " + bitmapBytes.Length.ToString());
                _dispatcherQueue.TryEnqueue(async () =>
                {
                     BitmapImage iconImage = new BitmapImage();

                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(bitmapBytes.AsBuffer());
                        stream.Seek(0);
                        await iconImage.SetSourceAsync(stream);
                    }
                    iconBitmapProperty = iconImage;

                });

            }

            var pack = await nSockethelper.receiveMsg();
            //Debug.WriteLine(pack);

            var ticker = await nSockethelper.receiveMsg();
            //Debug.WriteLine(ticker);

            var recTitle = await nSockethelper.receiveMsg();
            //Debug.WriteLine(recTitle);

            var recContent = await nSockethelper.receiveMsg();
            //Debug.WriteLine(recContent);

            var appName = await nSockethelper.receiveMsg();
            //Debug.WriteLine(appName);


            if (pack == "com.whatsapp")
            {
                //_dispatcherQueue.TryEnqueue(async () =>
                //{
                //    if (await nSockethelper.receiveMsg() == "true")
                //    {

                //        appIconBitmap = await nSockethelper.reciveBitmap();
                //    }
                //    iconBitmapProperty = appIconBitmap;
                //});
                var pack1 = nSockethelper.receiveMsg().Result;
                var ticker1 =nSockethelper.receiveMsg().Result;
                var tittle1 = nSockethelper.receiveMsg().Result;
                recContent += "\n" + nSockethelper.receiveMsg().Result;
      
            }
            _dispatcherQueue.TryEnqueue(() =>
            {
                titleProperty = recTitle;
                contentProperty = recContent;
                applicationName = appName;
            });
           
            //Debug.WriteLine("\n******************************************");
            //Debug.WriteLine($"titile : {titleProperty} \ncontent : {content} \napp : {pack} \nticker : {ticker}");
            //Debug.WriteLine("******************************************\n");
        }
    }
}
