// <copyright file="IAudioService.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>Interface for the audio service</summary>
    public interface IAudioService
    {
        /// <summary>The list of output devices</summary>
        public Dictionary<string, int> OutputDevices { get; set; }

        /// <summary>The list of input devices</summary>
        public Dictionary<string, int> InputDevices { get; set; }

        /// <summary>List of files and their paths</summary>
        public Dictionary<string, string> FilesAndPaths { get; set; }

        /// <summary>Device selected for output</summary>
        public int SelectedOutputDeviceIndex { get; set; }

        /// <summary>Device selected for monitoring</summary>
        public int SelectedMonitorDeviceIndex { get; set; }

        /// <summary>Device selected for input</summary>
        public int SelectedInputDeviceIndex { get; set; }

        /// <summary>Set the volume for playback</summary>
        public double DesiredVolume { get; set; }

        /// <summary>List of favorites and their slots</summary>
        public Dictionary<int, string> Favorites { get; set; }

        /// <summary>Selected file to be played</summary>
        public string SelectedFilePath { get; set; }

        /// <summary>Selected favorite to be played</summary>
        public string SelectedFavoritePath { get; set; }
    }
}
