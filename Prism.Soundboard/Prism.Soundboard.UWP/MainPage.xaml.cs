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
using Windows.Media.Devices;
using Windows.Devices.Enumeration;
using Windows.Storage;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Prism.Soundboard.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Dictionary<string, DeviceInformation> outputDevices;
        private Dictionary<string, IStorageItem> filesAndPath;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void AppLoaded(object sender, RoutedEventArgs e)
        {
            this.outputDevices = new Dictionary<string, DeviceInformation>();

            string audioSelector = MediaDevice.GetAudioRenderSelector();
            var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
            foreach (var device in outputDevices)
            {
                this.outputDevices.Add(device.Name, device);
                this.OutputDeviceSelector.Items.Add(device.Name);
                this.MonitorDeviceSelector.Items.Add(device.Name);
            }

            this.filesAndPath = new Dictionary<string, IStorageItem>();

            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder files = await folderPicker.PickSingleFolderAsync();

            IReadOnlyList<IStorageItem> itemsList = await files.GetItemsAsync();

            foreach (var item in itemsList)
            {
                this.filesAndPath.Add(item.Name, item);
                this.AudioFiles.Items.Add(item.Name);
            }
        }
    }
}
