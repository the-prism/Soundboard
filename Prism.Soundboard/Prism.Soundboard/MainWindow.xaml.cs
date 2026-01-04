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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using NAudio.Wave;
    using Prism.Soundboard.Services;
    using UnManaged;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private IAudioService audioService;

        private WaveOutEvent outputDevice;
        private WaveOutEvent monitorDevice;
        private WaveInEvent inputMic;
        private WaveOutEvent inputDevice;
        private BufferedWaveProvider waveProvider;
        private AudioFileReader audioFile;
        private AudioFileReader monitorAudioFile;
        private double desiredVolume;
        private bool simpleMode;
        private List<Tuple<string, string>> lastFilesPlayed = new List<Tuple<string, string>>(10);
        private Dictionary<int, string> favorites = new Dictionary<int, string>();
        private HotKey play = null;
        private HotKey stop = null;
        private HotKey favorite1 = null;
        private HotKey favorite2 = null;
        private HotKey favorite3 = null;
        private HotKey favorite4 = null;
        private HotKey favorite5 = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="audioService">Dependency injected service</param>
        public MainWindow(IAudioService audioService)
        {
            this.InitializeComponent();

            this.audioService = audioService;

            foreach (var entry in this.audioService.OutputDevices)
            {
                this.OutputDeviceSelector.Items.Add(entry.Key);
                this.MonitorDeviceSelector.Items.Add(entry.Key);
            }

            foreach (var entry in this.audioService.InputDevices)
            {
                this.InputDeviceSelector.Items.Add(entry.Key);
            }

            this.LoadSettings();

            foreach (var file in this.audioService.FilesAndPaths)
            {
                var content = new Sound(file.Key, this);
                content.Filename.Text = file.Key;
                if (this.Favorites.ContainsValue(file.Key))
                {
                    var favorite = this.Favorites.First(m => m.Value == file.Key);
                    content.Slot.Text = favorite.Key.ToString();
                }

                this.AudioFiles.Items.Add(content);
            }

            this.play = new HotKey(Key.F1, KeyModifier.None, this.OnHotKeyHandler);
            this.stop = new HotKey(Key.F1, KeyModifier.Ctrl, this.OnHotKeyHandler);
            this.favorite1 = new HotKey(Key.F2, KeyModifier.None, this.OnHotKeyHandler);
            this.favorite2 = new HotKey(Key.F3, KeyModifier.None, this.OnHotKeyHandler);
            this.favorite3 = new HotKey(Key.F4, KeyModifier.None, this.OnHotKeyHandler);
            this.favorite4 = new HotKey(Key.F5, KeyModifier.None, this.OnHotKeyHandler);
            this.favorite5 = new HotKey(Key.F6, KeyModifier.None, this.OnHotKeyHandler);
        }

        /// <summary>List of favorites for hotkeys</summary>
        public Dictionary<int, string> Favorites
        {
            get => this.favorites;
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
        public void Dispose()
        {
            this.play?.Dispose();
            this.stop?.Dispose();
            this.outputDevice?.Dispose();
            this.monitorDevice?.Dispose();
            this.audioFile?.Dispose();
            this.inputMic?.Dispose();
            this.favorite1?.Dispose();
            this.favorite2?.Dispose();
            this.favorite3?.Dispose();
            this.favorite4?.Dispose();
            this.favorite5?.Dispose();
        }

        /// <summary>Refresh list after favorites change</summary>
        public void Refresh()
        {
            this.AudioFiles.Items.Clear();
            foreach (var file in this.audioService.FilesAndPaths)
            {
                var content = new Sound(file.Key, this);
                content.Filename.Text = file.Key;
                if (this.Favorites.ContainsValue(file.Key))
                {
                    var favorite = this.Favorites.First(m => m.Value == file.Key);
                    content.Slot.Text = favorite.Key.ToString();
                }

                this.AudioFiles.Items.Add(content);
            }
        }

        /// <summary>Play the audio file at the selected file path</summary>
        /// <param name="filePath"></param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PlayAudio(string filePath)
        {
            if (this.audioFile is not null)
            {
                var waitMainTask = this.WaitForMainPlaybackStopped();
                var waitMonitorTask = this.WaitForMonitorPlaybackStopped();

                this.outputDevice?.Stop();
                this.monitorDevice?.Stop();

                await waitMainTask;
                await waitMonitorTask;
            }

            if (this.outputDevice == null)
            {
                this.outputDevice = new WaveOutEvent() { DeviceNumber = this.audioService.SelectedOutputDeviceIndex };
                this.outputDevice.PlaybackStopped += this.OnPlaybackStopped;
            }

            if (this.monitorDevice == null && this.audioService.SelectedOutputDeviceIndex != this.audioService.SelectedMonitorDeviceIndex)
            {
                this.monitorDevice = new WaveOutEvent() { DeviceNumber = this.audioService.SelectedMonitorDeviceIndex };
                this.monitorDevice.PlaybackStopped += this.OnPlaybackStopped;
            }

            if (this.audioFile == null)
            {
                float convertedVolume = (float)this.desiredVolume / 10f;

                this.audioFile = new AudioFileReader(filePath);
                this.audioFile.Volume = convertedVolume;
                this.monitorAudioFile = new AudioFileReader(filePath);
                this.monitorAudioFile.Volume = convertedVolume;

                this.outputDevice.Init(this.audioFile);
                this.monitorDevice?.Init(this.monitorAudioFile);
            }

            this.outputDevice.Play();
            this.monitorDevice?.Play();
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            await this.PlayAudio(this.audioService.SelectedFilePath);
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

        private Task WaitForMainPlaybackStopped()
        {
            var taskCompletion = new TaskCompletionSource<object>();

            EventHandler<StoppedEventArgs> handler = null;
            handler = (s, e) =>
            {
                this.outputDevice?.PlaybackStopped -= handler;
                taskCompletion.SetResult(null);
            };

            this.outputDevice?.PlaybackStopped += handler;

            return taskCompletion.Task;
        }

        private Task WaitForMonitorPlaybackStopped()
        {
            var taskCompletion = new TaskCompletionSource<object>();

            EventHandler<StoppedEventArgs> handler = null;
            handler = (s, e) =>
            {
                this.monitorDevice?.PlaybackStopped -= handler;
                taskCompletion.SetResult(null);
            };

            this.monitorDevice?.PlaybackStopped += handler;

            return taskCompletion.Task;
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
                this.audioService?.SelectedOutputDeviceIndex = this.audioService?.OutputDevices?[this.OutputDeviceSelector.SelectedItem.ToString()] ?? -1;
            }
            catch (Exception)
            {
                this.audioService?.SelectedOutputDeviceIndex = -1;
            }
        }

        private void AudioFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.AudioFiles.SelectedItem is not null)
            {
                this.audioService.SelectedFilePath = this.audioService.FilesAndPaths?[this.AudioFiles.SelectedItem.ToString()];
                this.LastSelected(this.audioService.SelectedFilePath, this.AudioFiles.SelectedItem.ToString());
            }
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
                this.audioService?.SelectedMonitorDeviceIndex = this.audioService?.OutputDevices?[this.MonitorDeviceSelector.SelectedItem.ToString()] ?? -1;
            }
            catch (Exception)
            {
                this.audioService?.SelectedMonitorDeviceIndex = -1;
            }
        }

        private void InputDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.audioService?.SelectedInputDeviceIndex = this.audioService?.InputDevices?[this.InputDeviceSelector.SelectedItem.ToString()] ?? -1;
            }
            catch (Exception)
            {
                this.audioService?.SelectedInputDeviceIndex = -1;
            }
        }

        private void VolumeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.desiredVolume = this.VolumeControl.Value;
        }

        private void AudioFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.audioService.SelectedFilePath = this.audioService.FilesAndPaths?[this.AudioFiles.SelectedItem.ToString()];

            this.Play_Click(sender, e);
        }

        private void SaveSettings()
        {
            SavedSettings settingsToSave = new SavedSettings()
            {
                OutputDeviceIndex = this.audioService.SelectedOutputDeviceIndex,
                MonitorDeviceIndex = this.audioService.SelectedMonitorDeviceIndex,
                InputMicIndex = this.audioService.SelectedInputDeviceIndex,
                Volume = this.desiredVolume,
                SimpleMode = this.simpleMode,
                SimpleOptions = this.lastFilesPlayed,
                Favorites = this.favorites,
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
                    this.OutputDeviceSelector.SelectedItem = this.audioService.OutputDevices.Where(v => v.Value == restoredSettings.OutputDeviceIndex).FirstOrDefault().Key;
                }

                if (restoredSettings.MonitorDeviceIndex != -1)
                {
                    this.MonitorDeviceSelector.SelectedItem = this.audioService.OutputDevices.Where(v => v.Value == restoredSettings.MonitorDeviceIndex).FirstOrDefault().Key;
                }

                if (restoredSettings.InputMicIndex != -1)
                {
                    this.InputDeviceSelector.SelectedItem = this.audioService.InputDevices.Where(v => v.Value == restoredSettings.InputMicIndex).FirstOrDefault().Key;
                }

                this.VolumeControl.Value = restoredSettings.Volume;

                this.audioService.SelectedOutputDeviceIndex = restoredSettings.OutputDeviceIndex;
                this.audioService.SelectedMonitorDeviceIndex = restoredSettings.MonitorDeviceIndex;
                this.audioService.SelectedInputDeviceIndex = restoredSettings.InputMicIndex;
                this.desiredVolume = restoredSettings.Volume;
                this.lastFilesPlayed = restoredSettings.SimpleOptions ?? new List<Tuple<string, string>>(10);
                this.favorites = restoredSettings.Favorites ?? new Dictionary<int, string>();
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

                this.inputDevice = new WaveOutEvent() { DeviceNumber = this.audioService.SelectedOutputDeviceIndex };
                this.inputDevice.PlaybackStopped += this.OnRecordStopped;

                this.inputMic = new WaveInEvent() { DeviceNumber = this.audioService.SelectedInputDeviceIndex, WaveFormat = new WaveFormat(48000, 16, 2) };
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

        private async void OnHotKeyHandler(HotKey hotKey)
        {
            if (hotKey == this.play)
            {
                await this.PlayAudio(this.audioService.SelectedFilePath);
            }
            else if (hotKey == this.stop)
            {
                this.Stop_Click(null, null);
            }
            else if (hotKey == this.favorite1)
            {
                await this.PlayFavorite(1);
            }
            else if (hotKey == this.favorite2)
            {
                await this.PlayFavorite(2);
            }
            else if (hotKey == this.favorite3)
            {
                await this.PlayFavorite(3);
            }
            else if (hotKey == this.favorite4)
            {
                await this.PlayFavorite(4);
            }
            else if (hotKey == this.favorite5)
            {
                await this.PlayFavorite(5);
            }
        }

        private async Task PlayFavorite(int id)
        {
            this.audioService.SelectedFavoritePath = this.audioService.FilesAndPaths?[this.Favorites[id]];
            await this.PlayAudio(this.audioService.SelectedFavoritePath);
        }
    }
}
