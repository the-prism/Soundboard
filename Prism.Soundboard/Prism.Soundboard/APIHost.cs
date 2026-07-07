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

    /// <summary>Host of the API and the embedded Blazor web UI</summary>
    public static class APIHost
    {
        /// <summary>Create the API host</summary>
        /// <returns>Configured host ready to be started</returns>
        public static IHost BuildAPI()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                // Keep the content root at the exe folder regardless of the launching
                // process's working directory.
                ContentRootPath = AppContext.BaseDirectory,
            });

            builder.WebHost.UseUrls("http://0.0.0.0:5010");

            // MapStaticAssets needs the static web assets file provider to resolve asset bytes.
            // CreateBuilder only wires it up automatically in the Development environment, so call
            // it explicitly: it loads the build manifest when running from bin (dev), and is a
            // no-op once published, where the assets are copied into wwwroot and served directly.
            builder.WebHost.UseStaticWebAssets();
            builder.Services.AddLogging();

            // Register Services
            builder.Services.AddSingleton<IAudioService, AudioService>();

            // Bridge the web UI to the in-process WPF audio playback
            builder.Services.AddSingleton<Web.Services.IAudioService, WebAudioService>();

            // Register the Blazor web UI
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Register Views
            builder.Services.AddSingleton<MainWindow>();

            var app = builder.Build();

            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<Web.Components.App>()
                .AddInteractiveServerRenderMode();

            BuildRoutes(ref app);

            return app;
        }

        private static void BuildRoutes(ref WebApplication app)
        {
            app.MapGet("/ping", () => "pong");
            app.MapGet("/isalive", () => DateTime.Now);
            app.MapGet("/play", ([FromServices] IAudioService audioService) =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    // This runs on the UI thread
                    await (Application.Current.MainWindow as MainWindow).PlayAudio(audioService.SelectedFilePath);
                });

                return "OK";
            });

            app.MapPost("/play", ([FromServices] IAudioService audioService, [FromBody] string filename) =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    // This runs on the UI thread
                    await (Application.Current.MainWindow as MainWindow).PlayAudio(audioService.FilesAndPaths[filename]);
                });
            });

            app.MapGet("/files", ([FromServices] IAudioService audioService) =>
            {
                return Results.Ok(audioService.FilesAndPaths);
            });
        }
    }
}
