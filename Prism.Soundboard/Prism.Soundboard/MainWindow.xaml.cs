using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private WaveOutEvent monitorDevice;
        private AudioFileReader audioFile;
        private AudioFileReader monitorAudioFile;
        private Dictionary<string, int> outputDeviceIndexes;
        private Dictionary<string, string> filesAndPath;
        private int selectedOutputDeviceIndex;
        private int selectedMonitorDeviceIndex;
        private string selectedFilePath;
        private double desiredVolume;

        public MainWindow()
        {
            InitializeComponent();

            this.outputDeviceIndexes = new Dictionary<string, int>();

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                OutputDeviceSelector.Items.Add(caps.ProductName);
                MonitorDeviceSelector.Items.Add(caps.ProductName);
                this.outputDeviceIndexes.Add(caps.ProductName, n);
            }

            DirectoryInfo fileDirectory = new DirectoryInfo("Files");
            this.filesAndPath = new Dictionary<string, string>();

            if (!fileDirectory.Exists)
            {
                Directory.CreateDirectory("Files");
                Process.Start("explorer.exe", fileDirectory.FullName);
            }

            foreach (FileInfo file in fileDirectory.GetFiles())
            {
                this.filesAndPath.Add(file.Name, file.FullName);
                AudioFiles.Items.Add(file.Name);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent() { DeviceNumber = this.selectedOutputDeviceIndex };
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }

            if (monitorDevice == null && selectedOutputDeviceIndex != selectedMonitorDeviceIndex)
            {
                monitorDevice = new WaveOutEvent() { DeviceNumber = this.selectedMonitorDeviceIndex };
                monitorDevice.PlaybackStopped += OnPlaybackStopped;
            }

            if (audioFile == null)
            {
                audioFile = new AudioFileReader(this.selectedFilePath);
                float convertedVolume = (float)this.desiredVolume / 10f;
                audioFile.Volume = convertedVolume;
                monitorAudioFile = new AudioFileReader(this.selectedFilePath);
                outputDevice.Init(audioFile);
                monitorDevice?.Init(monitorAudioFile);
            }
            outputDevice.Play();
            monitorDevice?.Play();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            outputDevice?.Stop();
            monitorDevice?.Stop();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice?.Dispose();
            outputDevice = null;
            monitorDevice?.Dispose();
            monitorDevice = null;
            audioFile?.Dispose();
            audioFile = null;
            monitorAudioFile?.Dispose();
            monitorAudioFile = null;
        }

        private void OutputDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.selectedOutputDeviceIndex = this.outputDeviceIndexes?[OutputDeviceSelector.SelectedItem.ToString()] ?? -1;
            } 
            catch (Exception)
            {
                this.selectedOutputDeviceIndex = -1;
            }
        }

        private void AudioFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedFilePath = this.filesAndPath?[AudioFiles.SelectedItem.ToString()];
        }

        private void MonitorDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.selectedMonitorDeviceIndex = this.outputDeviceIndexes?[MonitorDeviceSelector.SelectedIndex.ToString()] ?? -1;
            }
            catch (Exception)
            {
                this.selectedMonitorDeviceIndex = -1;
            }
        }

        private void VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.desiredVolume = VolumeControl.Value;
        }
    }
}
