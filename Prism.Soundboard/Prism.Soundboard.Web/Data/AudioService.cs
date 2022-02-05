// <copyright file="AudioService.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard.Web.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;

    using NAudio.Wave;

    /// <summary>Service responsible for playing audio files</summary>
    public class AudioService : INotifyPropertyChanged
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
        private List<string> monitorDevices = new List<string>();
        private List<string> outputDevices = new List<string>();
        private List<string> files = new List<string>();
        private string selectedFile = null;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Audio service</summary>
        public AudioService()
        {
            this.outputDeviceIndexes = new Dictionary<string, int>();

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                this.OutputDevices.Add(caps.ProductName);
                this.MonitorDevices.Add(caps.ProductName);
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
                this.AudioFiles.Add(file.Name);
            }

            this.LoadSettings();
        }

        /// <summary>List of output devices</summary>
        public List<string> OutputDevices
        {
            get
            {
                return this.outputDevices;
            }
        }

        /// <summary>List of monitor devices</summary>
        public List<string> MonitorDevices
        {
            get
            {
                return this.monitorDevices;
            }
        }

        /// <summary>List of audio files</summary>
        public List<string> AudioFiles
        {
            get
            {
                return this.files;
            }
        }

        /// <summary>File selected for playback</summary>
        public string SelectedFile
        {
            get
            {
                return this.selectedFile;
            }

            set
            {
                if (this.selectedFile != value)
                {
                    this.selectedFile = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool SimpleMode
        {
            get
            {
                return this.simpleMode;
            }

            set
            {
                this.simpleMode = value;
            }
        }

        public void Play()
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

        public void Stop()
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

        public void OutputDeviceSelector(string device)
        {
            try
            {
                this.selectedOutputDeviceIndex = this.outputDeviceIndexes?[device] ?? -1;
            }
            catch (Exception)
            {
                this.selectedOutputDeviceIndex = -1;
            }
        }

        public void MonitorDeviceSelector(string device)
        {
            try
            {
                this.selectedMonitorDeviceIndex = this.outputDeviceIndexes?[device] ?? -1;
            }
            catch (Exception)
            {
                this.selectedMonitorDeviceIndex = -1;
            }
        }

        public void SelectAudioFile(string file)
        {
            this.selectedFilePath = this.filesAndPath?[file];
            this.LastSelected(this.selectedFilePath, file);
        }

        private void VolumeControl(double volume)
        {
            this.desiredVolume = volume;
        }

        private void PlayImmediate(string file)
        {
            this.selectedFilePath = this.filesAndPath?[file];

            this.Play();
        }

        /// <summary>Create the OnPropertyChanged method to raise the event
        /// The calling member's name will be used as the parameter.</summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                    this.selectedOutputDeviceIndex = restoredSettings.OutputDeviceIndex;
                }

                if (restoredSettings.MonitorDeviceIndex != -1)
                {
                    this.selectedMonitorDeviceIndex = restoredSettings.MonitorDeviceIndex;
                }

                this.desiredVolume = restoredSettings.Volume;

                this.selectedOutputDeviceIndex = restoredSettings.OutputDeviceIndex;
                this.selectedMonitorDeviceIndex = restoredSettings.MonitorDeviceIndex;
                this.desiredVolume = restoredSettings.Volume;
                this.lastFilesPlayed = restoredSettings.SimpleOptions;
                this.SimpleMode = restoredSettings.SimpleMode;
            }
            catch (FileNotFoundException)
            {
            }
        }
    }
}
