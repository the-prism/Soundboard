// <copyright file="IAudioService.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard.Web.Services
{
    /// <summary>Interface for the audio services</summary>
    public interface IAudioService
    {
        /// <summary>List of all the files in the soundboard</summary>
        public Dictionary<string, string> ListOfFiles { get; }

        /// <summary>Play a certain file</summary>
        /// <param name="file">Filename of the file to play</param>
        public Task PlayFile(string file);
    }
}
