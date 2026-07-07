// <copyright file="WebAudioService.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;

    using Prism.Soundboard.Services;

    /// <summary>Bridges the embedded web UI to the in-process WPF audio playback.</summary>
    public class WebAudioService : Web.Services.IAudioService
    {
        private readonly IAudioService audioService;

        /// <summary>Initializes a new instance of the <see cref="WebAudioService"/> class.</summary>
        /// <param name="audioService">The WPF audio service holding the available files.</param>
        public WebAudioService(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> ListOfFiles => this.audioService.FilesAndPaths;

        /// <inheritdoc/>
        public async Task PlayFile(string file)
        {
            // Playback must run on the WPF UI thread.
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await (Application.Current.MainWindow as MainWindow).PlayAudio(this.audioService.FilesAndPaths[file]);
            }).Task.Unwrap();
        }
    }
}
