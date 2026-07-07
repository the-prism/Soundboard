// <copyright file="App.xaml.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

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
        protected override void OnExit(ExitEventArgs e)
        {
            if (this.apiHost != null)
            {
                Task.Run(() => this.apiHost.StopAsync()).Wait();
            }

            base.OnExit(e);
        }
    }
}
