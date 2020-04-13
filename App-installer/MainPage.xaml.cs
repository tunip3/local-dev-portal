using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Web;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using Microsoft.Tools.WindowsDevicePortal;
using static Microsoft.Tools.WindowsDevicePortal.DevicePortal;
using Windows.Security.Cryptography.Certificates;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App_installer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
           
        }

        private Windows.Storage.StorageFile file;
        private List<Windows.Storage.StorageFile> dependancies = new List<Windows.Storage.StorageFile>();

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".appx");
            picker.FileTypeFilter.Add(".msix");
            picker.FileTypeFilter.Add(".appxbundle");
            picker.FileTypeFilter.Add(".msixbundle");
            file = await picker.PickSingleFileAsync();
        }

        async private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (file != null)
            {
                DevicePortal portal = new DevicePortal(
                    new DefaultDevicePortalConnection(
                    "https://127.0.0.1:11443", // a full URI (e.g. 'https://localhost:50443')
                    username.Text,
                    password.Text));
                Certificate certificate = await portal.GetRootDeviceCertificateAsync(true);
                await portal.ConnectAsync(manualCertificate: certificate);
                await portal.InstallApplicationAsync(file.Name, file, dependancies);
                await new MessageDialog("app installed").ShowAsync();
            } else {
                //file not selected
                await new MessageDialog("please select a file first").ShowAsync();
            }
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        async private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".appx");
            picker.FileTypeFilter.Add(".msix");
            picker.FileTypeFilter.Add(".appxbundle");
            picker.FileTypeFilter.Add(".msixbundle");
            dependancies.Add(await picker.PickSingleFileAsync());
        }
    }
}
