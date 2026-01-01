// <copyright file="App.xaml.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _apiHost;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseUrls("http://localhost:5000");

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

            _apiHost = app;

            _apiHost.StartAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _apiHost?.StopAsync().Wait(); base.OnExit(e);
        }
    }
}
