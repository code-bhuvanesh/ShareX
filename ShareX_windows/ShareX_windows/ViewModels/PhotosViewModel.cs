using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ShareX_windows.customWigets;
using ShareX_windows.Helpers;
using ShareX_windows.Views;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace ShareX_windows.ViewModels
{
  
    public class PhotosViewModel
    {
        private DispatcherQueue _dispatcherQueue;


        private int port = 7586;
        private string tempPath = System.IO.Path.GetTempPath() + "sharex\\";
        private List<string> images = new List<string>();
        //private ImageFullscreenView fullScreenImage;
        Task<IRandomAccessStream> getImage;

        public ObservableCollection<CustomImage> photosList;

        public NavigationView Nav { get; }
        public PhotosView Pv { get; }
        public ImageFullscreenView IFV;

        private SocketHelper pSocket;

        private StorageFolder imageFolder;
        public PhotosViewModel(NavigationView nav, PhotosView pv)
        {
            photosList = new ObservableCollection<CustomImage>();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            pSocket = new SocketHelper(port);
            mainTask("first task");
            Nav = nav;
            Pv = pv;
            IFV = new ImageFullscreenView(Nav, Pv);
        }

        private void img_click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            //IFV.showImages(((CustomImage)sender).pos);
            IFV.setImage(((CustomImage)sender).pos);
            Nav.Content = IFV;
        }


        public async void mainTask(String taskName)
        {

            //fullScreenImage = new ImageFullscreenView(images, -1, this);
            Debug.WriteLine("starting server for photos");
            await pSocket.hostServer();
            int imgLength = 0;
            
            Debug.WriteLine($"temp path {tempPath}");
            if(!System.IO.Directory.Exists(tempPath)) {
                System.IO.Directory.CreateDirectory(tempPath);
            }

            imageFolder = await StorageFolder.GetFolderFromPathAsync(tempPath);
            var query = imageFolder.CreateFileQuery();
            var files = (await query.GetFilesAsync());
            var dirFiles = new List<string>();
            foreach(var file in files)
            {
                dirFiles.Add(file.Path);
            }
            Debug.WriteLine($"no of photos is {dirFiles.Count}");
            //dirFiles = new string[0];
            var recInt = await pSocket.receiveMsg();
            Debug.WriteLine($"no of photos receiving is {recInt}");

            var gotCount = int.TryParse(recInt, out imgLength);
            if (!gotCount)
            {
                imgLength = 0;
            }
            var noImagesToSend = imgLength;
            var ImageToSendAtTime = 20;
           
            var pos = 0;
            while (noImagesToSend -1 > 0)
            {
                var toSendNow = ImageToSendAtTime;
                if(toSendNow - ImageToSendAtTime < 0)
                {
                    toSendNow = noImagesToSend;
                }
                for (int i = 0; i < toSendNow; i++)
                {
                    await Task.Run(() =>
                    {
                        Thread.Sleep(10);
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            photosList.Add(new CustomImage()
                            {
                                Background = new SolidColorBrush(Color.FromArgb((byte)6, (byte)255, (byte)255, (byte)255)),

                            });
                            //Debug.WriteLine("image added ");
                        });

                    });


                    //photosList.Add(imageHolders[i]);
                }
                
             

                for (int i = 0; i < toSendNow; i++)
                {
                    string imgName = null;
                    try
                    {
                        imgName = await pSocket.receiveMsg();
                        imgName = FileHandler.ReplaceInvalidChars(imgName);
                        Debug.WriteLine($"img name : {imgName}, task name : {taskName}");
                        
                        await Task.Run(() =>
                        {
                            Thread.Sleep(10);
                        });
                    }
                    catch(SocketException e)
                    {
                        Debug.WriteLine($"error receiving photos {e.Message}");
                        await pSocket.hostServer();
                    }
                    if (imgName != null)
                    {
                        //images.Add(new FileStream(imgPath, FileMode.Open));

                        images.Add(imgName);
                        //fullScreenImage.images = images;
                        try
                        {
                            getImage = Task.Run(() => openImage(imgName, taskName));
                            using (IRandomAccessStream inputstream = await getImage)
                            {
                                if (inputstream == null)
                                    continue;
                                BitmapImage sourceImage = new BitmapImage();
                                await sourceImage.SetSourceAsync(inputstream);
                                var origHeight = sourceImage.PixelHeight;
                                var origWidth = sourceImage.PixelWidth;
                                var ratioX = 200 / (float)origWidth;
                                var ratioY = 200 / (float)origHeight;
                                var ratio = Math.Min(ratioX, ratioY);
                                var newHeight = (int)(origHeight * ratio);
                                var newWidth = (int)(origWidth * ratio);

                                sourceImage.DecodePixelHeight = newHeight;
                                sourceImage.DecodePixelWidth = newWidth;
                                var img = new ImageBrush()
                                {
                                    ImageSource = sourceImage,
                                };
                                img.Stretch = Stretch.UniformToFill;
                               
                                photosList[pos].Background = img;
                                photosList[pos].imgPath = tempPath + imgName;
                                photosList[pos].pos = pos;
                                photosList[pos].Click += img_click;
                                //imageHolders[pos].MouseLeftButtonUp += showImage;
                                
                                pos++;
                            }
                            IFV.updateImages(imgName);
                        }
                        catch(Exception e)
                        {
                            Debug.WriteLine("file error: ", e.Message);
                        }
                      
                    }
                    else
                    {
                        Debug.WriteLine("image path is nulll");
                    }
                }
            }
        }

        public async Task<IRandomAccessStream> openImage(string imgName, string taskName)
        {
            StorageFile file = null;
            try
            {
                //if(File.Exists(tempPath + imgName ))
                //{
                //    file = await imageFolder.GetFileAsync(imgName);
                //    await pSocket.send_msg("success");
                //}
                //else
                //{
                //    Debug.WriteLine($"imgName {imgName}");
                //    await pSocket.send_msg("failed");
                //    var imagePath = await pSocket.receiveFile(tempPath);
                //    file = await imageFolder.GetFileAsync(imgName);
                //    return await file.OpenAsync(FileAccessMode.Read);
                //}

                try
                {
                    file = await imageFolder.GetFileAsync(imgName);
                    await pSocket.send_msg("success");
                }
                catch(Exception e)
                {
                    Debug.WriteLine( "file not found " + e.Message);
                    Debug.WriteLine($"imgName {imgName}, task = {taskName}");
                    await pSocket.send_msg("failed");
                    var imagePath = await pSocket.receiveFile(tempPath); 
                    file = await imageFolder.GetFileAsync(imgName);
                }


            }
            catch(Exception e)
            {
                 Debug.WriteLine("another error"  + e.Message);
            }
            return (file != null) ? await file.OpenAsync(FileAccessMode.Read) : null;

        }

        public async void disconect()
        {
            try
            {
                //await pSocket.disconnect();
                if(getImage != null)
                {
                    getImage.Dispose();
                }
                await pSocket.disconnect();
                mainTask("second task");
            }
            catch(Exception e) {
                Debug.WriteLine("photos view disconnect \n" + e.Message);
            }
        }

       
    }
}
