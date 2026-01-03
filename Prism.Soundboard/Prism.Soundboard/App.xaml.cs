// <copyright file="App.xaml.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Prism.Soundboard.Services;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost apiHost;

        /// <inheritdoc/>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.apiHost = APIHost.BuildAPI();

            this.apiHost.StartAsync();

            var mainWindow = this.apiHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        /// <inheritdoc/>
        protected override async void OnExit(ExitEventArgs e)
        {
            if (this.apiHost != null)
            {
                Task.Run(() => this.apiHost.StopAsync()).Wait();
            }

            base.OnExit(e);
        }
    }
}
