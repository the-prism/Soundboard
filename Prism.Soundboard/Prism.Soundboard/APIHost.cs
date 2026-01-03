// <copyright file="APIHost.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Prism.Soundboard.Services;

    /// <summary>Host of the API services</summary>
    public static class APIHost
    {
        /// <summary>Create the API host</summary>
        /// <returns>Configured host ready to be started</returns>
        public static IHost BuildAPI()
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseUrls("http://localhost:5000");
            builder.Services.AddLogging();

            // Register Services
            builder.Services.AddSingleton<IAudioService, AudioService>();

            // Register Views
            builder.Services.AddSingleton<MainWindow>();

            var app = builder.Build();

            BuildRoutes(ref app);

            return app;
        }

        private static void BuildRoutes(ref WebApplication app)
        {
            app.MapGet("/ping", () => "pong");
            app.MapGet("/isalive", () => DateTime.Now);
            app.MapGet("/play", ([FromServices] IAudioService audioService) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // This runs on the UI thread
                    (Application.Current.MainWindow as MainWindow).PlayAudio(audioService.SelectedFilePath);
                });

                return "OK";
            });

            app.MapPost("/play", ([FromServices] IAudioService audioService, [FromBody] string filename) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // This runs on the UI thread
                    (Application.Current.MainWindow as MainWindow).PlayAudio(audioService.FilesAndPaths[filename]);
                });
            });

            app.MapGet("/files", ([FromServices] IAudioService audioService) =>
            {
                return Results.Ok(audioService.FilesAndPaths);
            });
        }
    }
}
