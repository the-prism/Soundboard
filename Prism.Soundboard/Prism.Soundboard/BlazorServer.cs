// <copyright file="BlazorServer.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>Handle the creation of the blazor server process</summary>
    public static class BlazorServer
    {
        /// <summary>Start the webui part</summary>
        /// <returns>The started process</returns>
        public static Process StartBlazor()
        {
            var exePath = Path.Combine(AppContext.BaseDirectory, "blazor", "Prism.Soundboard.Web.exe");

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "blazor"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
            psi.Environment["ASPNETCORE_URLS"] = "http://0.0.0.0:5010";

            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    HandleWebOutput(e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    HandleWebError(e.Data);
                }
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        private static void HandleWebOutput(string line)
        {
            Debug.WriteLine("[WEB] " + line);
        }

        private static void HandleWebError(string line)
        {
            Debug.WriteLine("[WEB ERROR] " + line);
        }
    }
}
