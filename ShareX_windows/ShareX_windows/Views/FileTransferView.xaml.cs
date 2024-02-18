using Microsoft.UI.Xaml.Controls;
using ShareX_windows.customWigets;
using ShareX_windows.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShareX_windows.Views
{
    public sealed partial class FileTransferView : UserControl
    {
        public FileTrasferViewModel ftViewModel;
        public FileTransferView()
        {
            this.InitializeComponent();
            ftViewModel = new FileTrasferViewModel();
        }
      


    }
}
