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
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using NAudio.Wave;
    using UnManaged;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private WaveOutEvent outputDevice;
        private WaveOutEvent monitorDevice;
        private WaveInEvent inputMic;
        private WaveOutEvent inputDevice;
        private BufferedWaveProvider waveProvider;
        private AudioFileReader audioFile;
        private AudioFileReader monitorAudioFile;
        private Dictionary<string, int> outputDeviceIndexes;
        private Dictionary<string, int> inputDeviceIndexes;
        private Dictionary<string, string> filesAndPath;
        private int selectedOutputDeviceIndex;
        private int selectedMonitorDeviceIndex;
        private int selectedInputDeviceIndex;
        private string selectedFilePath;
        private double desiredVolume;
        private bool simpleMode;
        private List<Tuple<string, string>> lastFilesPlayed = new List<Tuple<string, string>>(10);
        private HotKey play = null;
        private HotKey stop = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.outputDeviceIndexes = new Dictionary<string, int>();
            this.inputDeviceIndexes = new Dictionary<string, int>();

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                this.OutputDeviceSelector.Items.Add(caps.ProductName);
                this.MonitorDeviceSelector.Items.Add(caps.ProductName);
                this.outputDeviceIndexes.Add(caps.ProductName, n);
            }

            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var caps = WaveIn.GetCapabilities(n);
                this.InputDeviceSelector.Items.Add(caps.ProductName);
                this.inputDeviceIndexes.Add(caps.ProductName, n);
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

            this.play = new HotKey(Key.F1, KeyModifier.None, this.OnHotKeyHandler);
            this.stop = new HotKey(Key.F1, KeyModifier.Ctrl, this.OnHotKeyHandler);
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

        /// <inheritdoc/>
        public void Dispose()
        {
            this.play?.Dispose();
            this.stop?.Dispose();
            this.outputDevice?.Dispose();
            this.monitorDevice?.Dispose();
            this.audioFile?.Dispose();
            this.inputMic?.Dispose();
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

        private void OnRecordStopped(object sender, StoppedEventArgs args)
        {
            this.inputMic?.Dispose();
            this.inputMic = null;

            this.inputDevice?.Dispose();
            this.inputDevice = null;

            this.waveProvider = null;
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

        private void InputDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.selectedInputDeviceIndex = this.inputDeviceIndexes?[this.InputDeviceSelector.SelectedItem.ToString()] ?? -1;
            }
            catch (Exception)
            {
                this.selectedInputDeviceIndex = -1;
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
                InputMicIndex = this.selectedInputDeviceIndex,
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

                if (restoredSettings.InputMicIndex != -1)
                {
                    this.InputDeviceSelector.SelectedItem = this.inputDeviceIndexes.Where(v => v.Value == restoredSettings.InputMicIndex).FirstOrDefault().Key;
                }

                this.VolumeControl.Value = restoredSettings.Volume;

                this.selectedOutputDeviceIndex = restoredSettings.OutputDeviceIndex;
                this.selectedMonitorDeviceIndex = restoredSettings.MonitorDeviceIndex;
                this.selectedInputDeviceIndex = restoredSettings.InputMicIndex;
                this.desiredVolume = restoredSettings.Volume;
                this.lastFilesPlayed = restoredSettings.SimpleOptions ?? new List<Tuple<string, string>>(10);
                this.SimpleMode = restoredSettings.SimpleMode;
            }
            catch (FileNotFoundException)
            {
            }
        }

        private void Passthrough_Click(object sender, RoutedEventArgs e)
        {
            if (this.inputMic is null)
            {
                this.Passthrough_Text.Text = "Stop";

                this.inputDevice = new WaveOutEvent() { DeviceNumber = this.selectedOutputDeviceIndex };
                this.inputDevice.PlaybackStopped += this.OnRecordStopped;

                this.inputMic = new WaveInEvent() { DeviceNumber = this.selectedInputDeviceIndex, WaveFormat = new WaveFormat(48000, 16, 2) };
                this.inputMic.RecordingStopped += this.OnRecordStopped;

                this.waveProvider = new BufferedWaveProvider(this.inputMic.WaveFormat);

                this.inputMic.DataAvailable += (s, e) =>
                {
                    this.waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
                };

                this.inputDevice.Init(this.waveProvider);

                this.inputMic.StartRecording();
                this.inputDevice.Play();
            }
            else
            {
                this.Passthrough_Text.Text = "Record";

                this.inputMic.StopRecording();
                this.inputDevice.Stop();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                this.Play_Click(null, null); // Call your method here
                e.Handled = true; // Mark the event as handled
            }
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {
            if (hotKey == this.play)
            {
                this.Play_Click(null, null);
            }
            else if (hotKey == this.stop)
            {
                this.Stop_Click(null, null);
            }
        }
    }
}
