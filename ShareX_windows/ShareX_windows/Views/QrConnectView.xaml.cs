using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using System;
using System.IO;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareX_windows.Views
{
    public sealed partial class QrConnectView : UserControl
    {


        public string Ip { get; }

        public QrConnectView(string ip)
        {
            this.InitializeComponent();
            GenrateQr(ip);
        }
        public async void GenrateQr(string msg)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(msg, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(30, new byte[] { 255, 255, 255, 255 }, new byte[] { 0, 0, 0, 0 });


            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(qrCodeAsBitmapByteArr);
                    await writer.StoreAsync();
                }
                var image = new BitmapImage();
                await image.SetSourceAsync(stream);

                QRImage.Source = image;
            }
        }



        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.CreateOptions = BitmapCreateOptions.None;
                image.SetSource(mem.AsRandomAccessStream());
            }
            return image;
        }
    }
}
