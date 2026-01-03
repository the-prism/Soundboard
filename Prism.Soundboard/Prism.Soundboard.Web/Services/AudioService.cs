// <copyright file="AudioService.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard.Web.Services
{
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>Implementation of the audio service</summary>
    public class AudioService : IAudioService
    {
        private IHttpClientFactory clientFactory;

        /// <summary>Initializes a new instance of the <see cref="AudioService"/> class.</summary>
        /// <param name="clientFactory"></param>
        public AudioService(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;

            this.ListOfFiles = this.GetListOfFiles();
        }

        /// <inheritdoc/>
        public Dictionary<string, string> ListOfFiles
        {
            get;
            private set => field = value;
        }

        /// <inheritdoc/>
        public async Task PlayFile(string file)
        {
            var client = this.clientFactory.CreateClient();

            var response = await client.PostAsJsonAsync<string>("http://localhost:5000/play", file);
            response.EnsureSuccessStatusCode();
        }

        private Dictionary<string, string> GetListOfFiles()
        {
            try
            {
                var client = this.clientFactory.CreateClient();

                var response = client.GetAsync("http://localhost:5000/files").Result;
                response.EnsureSuccessStatusCode();

                return JsonSerializer.Deserialize<Dictionary<string, string>>(response.Content.ReadAsStringAsync().Result) ?? new Dictionary<string, string>();
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
