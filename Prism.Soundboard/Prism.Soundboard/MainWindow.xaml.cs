// <copyright file="MainWindow.xaml.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
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

    using NAudio.Wave;

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
        private bool simpleMode;
        private List<Tuple<string, string>> lastFilesPlayed = new List<Tuple<string, string>>(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.outputDeviceIndexes = new Dictionary<string, int>();

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                this.OutputDeviceSelector.Items.Add(caps.ProductName);
                this.MonitorDeviceSelector.Items.Add(caps.ProductName);
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
                this.AudioFiles.Items.Add(file.Name);
            }

            this.LoadSettings();
        }

        private bool SimpleMode
        {
            get
            {
                return this.simpleMode;
            }

            set
            {
                if (value is true)
                {
                    this.AudioFiles.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.AudioFiles.Visibility = Visibility.Visible;
                }

                this.simpleMode = value;
            }
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var accentBrush = this.TryFindResource("AccentColorBrush") as SolidColorBrush;
            if (accentBrush != null)
            {
                accentBrush.Color.CreateAccentColors();
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (this.outputDevice == null)
            {
                this.outputDevice = new WaveOutEvent() { DeviceNumber = this.selectedOutputDeviceIndex };
                this.outputDevice.PlaybackStopped += this.OnPlaybackStopped;
            }

            if (this.monitorDevice == null && this.selectedOutputDeviceIndex != this.selectedMonitorDeviceIndex)
            {
                this.monitorDevice = new WaveOutEvent() { DeviceNumber = this.selectedMonitorDeviceIndex };
                this.monitorDevice.PlaybackStopped += this.OnPlaybackStopped;
            }

            if (this.audioFile == null)
            {
                float convertedVolume = (float)this.desiredVolume / 10f;

                this.audioFile = new AudioFileReader(this.selectedFilePath);
                this.audioFile.Volume = convertedVolume;
                this.monitorAudioFile = new AudioFileReader(this.selectedFilePath);
                this.monitorAudioFile.Volume = convertedVolume;

                this.outputDevice.Init(this.audioFile);
                this.monitorDevice?.Init(this.monitorAudioFile);
            }

            this.outputDevice.Play();
            this.monitorDevice?.Play();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            this.outputDevice?.Stop();
            this.monitorDevice?.Stop();
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            this.outputDevice?.Dispose();
            this.outputDevice = null;

            this.monitorDevice?.Dispose();
            this.monitorDevice = null;

            this.audioFile?.Dispose();
            this.audioFile = null;

            this.monitorAudioFile?.Dispose();
            this.monitorAudioFile = null;
        }

        private void OutputDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.selectedOutputDeviceIndex = this.outputDeviceIndexes?[this.OutputDeviceSelector.SelectedItem.ToString()] ?? -1;
            }
            catch (Exception)
            {
                this.selectedOutputDeviceIndex = -1;
            }
        }

        private void AudioFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedFilePath = this.filesAndPath?[this.AudioFiles.SelectedItem.ToString()];
            this.LastSelected(this.selectedFilePath, this.AudioFiles.SelectedItem.ToString());
        }

        private void LastSelected(string selectedFilePath, string display)
        {
            var lastSelection = new Tuple<string, string>(display, selectedFilePath);
            if (this.lastFilesPlayed.Contains(lastSelection))
            {
                return;
            }

            if (this.lastFilesPlayed.Count < 10)
            {
                this.lastFilesPlayed.Add(lastSelection);
            }
            else
            {
                this.lastFilesPlayed.RemoveAt(0);
                this.lastFilesPlayed.Add(lastSelection);
            }
        }

        private void MonitorDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.selectedMonitorDeviceIndex = this.outputDeviceIndexes?[this.MonitorDeviceSelector.SelectedItem.ToString()] ?? -1;
            }
            catch (Exception)
            {
                this.selectedMonitorDeviceIndex = -1;
            }
        }

        private void VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.desiredVolume = this.VolumeControl.Value;
        }

        private void AudioFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.selectedFilePath = this.filesAndPath?[this.AudioFiles.SelectedItem.ToString()];

            this.Play_Click(sender, e);
        }

        private void SaveSettings()
        {
            SavedSettings settingsToSave = new SavedSettings()
            {
                OutputDeviceIndex = this.selectedOutputDeviceIndex,
                MonitorDeviceIndex = this.selectedMonitorDeviceIndex,
                Volume = this.desiredVolume,
                SimpleMode = this.simpleMode,
                SimpleOptions = this.lastFilesPlayed,
            };

            string content = JsonSerializer.Serialize(settingsToSave);

            File.WriteAllText("settings.json", content);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.SaveSettings();
        }

        private void LoadSettings()
        {
            try
            {
                string content = File.ReadAllText("settings.json");
                SavedSettings restoredSettings = JsonSerializer.Deserialize<SavedSettings>(content);

                if (restoredSettings.OutputDeviceIndex != -1)
                {
                    this.OutputDeviceSelector.SelectedItem = this.outputDeviceIndexes.Where(v => v.Value == restoredSettings.OutputDeviceIndex).FirstOrDefault().Key;
                }

                if (restoredSettings.MonitorDeviceIndex != -1)
                {
                    this.MonitorDeviceSelector.SelectedItem = this.outputDeviceIndexes.Where(v => v.Value == restoredSettings.MonitorDeviceIndex).FirstOrDefault().Key;
                }

                this.VolumeControl.Value = restoredSettings.Volume;

                this.selectedOutputDeviceIndex = restoredSettings.OutputDeviceIndex;
                this.selectedMonitorDeviceIndex = restoredSettings.MonitorDeviceIndex;
                this.desiredVolume = restoredSettings.Volume;
                this.lastFilesPlayed = restoredSettings.SimpleOptions;
                this.SimpleMode = restoredSettings.SimpleMode;
            }
            catch (FileNotFoundException) { }
        }
    }
}
