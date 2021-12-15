﻿// <copyright file="SavedSettings.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>Settings saved for faster resume</summary>
    public class SavedSettings
    {
        /// <summary>Initializes a new instance of the <see cref="SavedSettings"/> class.</summary>
        public SavedSettings() { }

        /// <summary>Selected output device index</summary>
        public int OutputDeviceIndex { get; set; }

        /// <summary>Slected device for monitoring</summary>
        public int MonitorDeviceIndex { get; set; }

        /// <summary>Previous volume used</summary>
        public double Volume { get; set; }
    }
}
