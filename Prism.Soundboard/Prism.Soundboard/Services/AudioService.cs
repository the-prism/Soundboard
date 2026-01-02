// <copyright file="AudioService.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    using NAudio.Wave;

    /// <summary>Implementation of the audio service</summary>
    public class AudioService : IAudioService
    {
        /// <summary>Initializes a new instance of the <see cref="AudioService"/> class.</summary>
        public AudioService()
        {
            this.OutputDevices = new Dictionary<string, int>();
            this.InputDevices = new Dictionary<string, int>();
            this.FilesAndPaths = new Dictionary<string, string>();
            this.Favorites = new Dictionary<int, string>();

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                this.OutputDevices.Add(caps.ProductName, n);
            }

            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var caps = WaveIn.GetCapabilities(n);
                this.InputDevices.Add(caps.ProductName, n);
            }

            DirectoryInfo fileDirectory = new DirectoryInfo("Files");

            if (!fileDirectory.Exists)
            {
                Directory.CreateDirectory("Files");
                Process.Start("explorer.exe", fileDirectory.FullName);
            }

            foreach (FileInfo file in fileDirectory.GetFiles())
            {
                this.FilesAndPaths.Add(file.Name, file.FullName);
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, int> OutputDevices
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public Dictionary<string, int> InputDevices
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> FilesAndPaths
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public int SelectedOutputDeviceIndex
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public int SelectedMonitorDeviceIndex
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public int SelectedInputDeviceIndex
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public double DesiredVolume
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public Dictionary<int, string> Favorites
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public string SelectedFilePath
        {
            get;
            set => field = value;
        }

        /// <inheritdoc/>
        public string SelectedFavoritePath
        {
            get;
            set => field = value;
        }
    }
}
