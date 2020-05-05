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
using System.Net;

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
        public class DefaultDevicePortalConnection : IDevicePortalConnection
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DefaultDevicePortalConnection" /> class.
            /// </summary>
            /// <param name="address">The fully qualified (ex: "https:/1.2.3.4:4321") address of the device.</param>
            /// <param name="userName">The user name used in the connection credentials.</param>
            /// <param name="password">The password used in the connection credentials.</param>
            public DefaultDevicePortalConnection(
                string address,
                string userName,
                string password)
            {
                this.Connection = new Uri(address);
                if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(password))
                {
                    // append auto- to the credentials to bypass CSRF token requirement on non-Get requests.
                    this.Credentials = new NetworkCredential(string.Format("auto-{0}", userName), password);
                }
            }

            /// <summary>
            /// Gets the URI used to connect to the device.
            /// </summary>
            public Uri Connection
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets Web Socket Connection property
            /// </summary>
            public Uri WebSocketConnection
            {
                get
                {
                    if (this.Connection == null)
                    {
                        return null;
                    }

                    // Convert the scheme from http[s] to ws[s].
                    string scheme = this.Connection.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";

                    return new Uri(string.Format("{0}://{1}", scheme, this.Connection.Authority));
                }
            }

            /// <summary>
            /// Gets the credentials used to connect to the device.
            /// </summary>
            public NetworkCredential Credentials
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets the device's operating system family.
            /// </summary>
            public string Family
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the operating system information.
            /// </summary>
            public OperatingSystemInformation OsInfo
            {
                get;
                set;
            }

            /// <summary>
            /// Updates the device's connection Uri.
            /// </summary>
            /// <param name="requiresHttps">Indicates whether or not to always require a secure connection.</param>
            public void UpdateConnection(bool requiresHttps)
            {
                this.Connection = new Uri(
                    string.Format(
                        "{0}://{1}",
                        requiresHttps ? "https" : "http",
                        this.Connection.Authority));
            }

            /// <summary>
            /// Updates the device's connection Uri.
            /// </summary>
            /// <param name="ipConfig">Object that describes the current network configuration.</param>
            /// <param name="requiresHttps">True if an https connection is required, false otherwise.</param>
            /// <param name="preservePort">True if the previous connection's port is to continue to be used, false otherwise.</param>
            public void UpdateConnection(
                IpConfiguration ipConfig,
                bool requiresHttps,
                bool preservePort)
            {
                Uri newConnection = null;

                foreach (NetworkAdapterInfo adapter in ipConfig.Adapters)
                {
                    foreach (IpAddressInfo addressInfo in adapter.IpAddresses)
                    {
                        // We take the first, non-169.x.x.x address we find that is not 0.0.0.0.
                        if ((addressInfo.Address != "0.0.0.0") && !addressInfo.Address.StartsWith("169."))
                        {
                            string address = addressInfo.Address;
                            if (preservePort)
                            {
                                address = string.Format(
                                    "{0}:{1}",
                                    address,
                                    this.Connection.Port);
                            }

                            newConnection = new Uri(
                                string.Format(
                                    "{0}://{1}",
                                    requiresHttps ? "https" : "http",
                                    address));
                            break;
                        }
                    }

                    if (newConnection != null)
                    {
                        this.Connection = newConnection;
                        break;
                    }
                }
            }
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

        private void auth_Click(object sender, RoutedEventArgs e)
        {
            if (username.IsEnabled != true)
            {
                username.IsEnabled = true;
                username.Text="gg";
            } else {
                username.IsEnabled = false;
                username.Text = null;
            }
            if (password.IsEnabled != true)
            {
                password.IsEnabled = true;
                password.Text = "bois";
            } else {
                password.IsEnabled = false;
                username.Text = null;
            }
        }
    }
}
