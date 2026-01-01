// <copyright file="App.xaml.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Prism.Soundboard.Services;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _apiHost;

        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseUrls("http://localhost:5000");
            builder.Services.AddLogging();

            // Register Services
            builder.Services.AddSingleton<IAudioService, AudioService>();

            // Register Views
            builder.Services.AddSingleton<MainWindow>();

            var app = builder.Build();

            app.MapGet("/ping", () => "pong");
            app.MapGet("/time", () => DateTime.Now);
            app.MapGet("/play", () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // This runs on the UI thread
                    (Application.Current.MainWindow as MainWindow).PlayAudio(false);
                });

                return "OK";
            });

            app.MapGet("/test", ([FromServices] IAudioService audioService) =>
            {
                return Results.Ok(audioService.Test += "1");
            });

            _apiHost = app;

            _apiHost.StartAsync();

            var mainWindow = app.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_apiHost != null)
            {
                Task.Run(() => _apiHost.StopAsync()).Wait();
            }

            base.OnExit(e);
        }
    }
}
