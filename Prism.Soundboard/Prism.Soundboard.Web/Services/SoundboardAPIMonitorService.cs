// <copyright file="SoundboardAPIMonitorService.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard.Web.Services
{
    /// <summary>Service to monitor the avaibility of the sounboard api</summary>
    public class SoundboardAPIMonitorService : BackgroundService
    {
        private readonly HttpClient httpClient;
        private readonly IHostApplicationLifetime lifetime;
        private readonly ILogger<SoundboardAPIMonitorService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundboardAPIMonitorService"/> class.
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="lifetime"></param>
        /// <param name="logger"></param>
        public SoundboardAPIMonitorService(IHttpClientFactory httpClientFactory, IHostApplicationLifetime lifetime, ILogger<SoundboardAPIMonitorService> logger)
        {
            this.httpClient = httpClientFactory.CreateClient();
            this.lifetime = lifetime;
            this.logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int failures = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await this.httpClient.GetAsync("http://localhost:5000/isalive", stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        failures = 0; // reset
                    }
                    else
                    {
                        failures++;
                    }
                }
                catch
                {
                    failures++;
                }

                if (failures >= 3)
                {
                    this.logger.LogCritical("Upstream API is unavailable. Shutting down server.");
                    this.lifetime.StopApplication();
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
