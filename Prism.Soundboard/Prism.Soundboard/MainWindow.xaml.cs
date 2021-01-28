using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prism.Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private Dictionary<string, int> outputDeviceIndexes;
        private int selectedOutputDeviceIndex;

        public MainWindow()
        {
            InitializeComponent();

            this.outputDeviceIndexes = new Dictionary<string, int>();

            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                OutputDeviceSelector.Items.Add(caps.ProductName);
                this.outputDeviceIndexes.Add(caps.ProductName, n);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent() { DeviceNumber = this.selectedOutputDeviceIndex };
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }
            if (audioFile == null)
            {
                audioFile = new AudioFileReader(@"M:\Session 6.mp3");
                outputDevice.Init(audioFile);
            }
            outputDevice.Play();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            outputDevice?.Stop();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;
        }

        private void OutputDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedOutputDeviceIndex = this.outputDeviceIndexes?[OutputDeviceSelector.SelectedItem.ToString()] ?? -1;
        }
    }
}
