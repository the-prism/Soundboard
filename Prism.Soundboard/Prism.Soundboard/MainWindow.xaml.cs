﻿using NAudio.Wave;
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
        private AudioFileReader audioFile;
        private Dictionary<string, int> outputDeviceIndexes;
        private Dictionary<string, string> filesAndPath;
        private int selectedOutputDeviceIndex;
        private string selectedFilePath;

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

            DirectoryInfo fileDirectory = new DirectoryInfo("Files");
            this.filesAndPath = new Dictionary<string, string>();

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
            if (audioFile == null)
            {
                audioFile = new AudioFileReader(this.selectedFilePath);
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

        private void AudioFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedFilePath = this.filesAndPath?[AudioFiles.SelectedItem.ToString()];
        }
    }
}
