// <copyright file="Sound.xaml.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for Sound.xaml
    /// </summary>
    public partial class Sound : UserControl
    {
        private MainWindow handle;

        /// <summary>Initializes a new instance of the <see cref="Sound"/> class.</summary>
        public Sound()
        {
            this.InitializeComponent();
        }

        /// <summary>Initializes a new instance of the <see cref="Sound"/> class.</summary>
        /// <param name="filename"></param>
        /// <param name="handle"></param>
        public Sound(string filename, MainWindow handle)
        {
            this.InitializeComponent();
            this.handle = handle;
            this.Filename.Text = filename;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Filename.Text;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var window = new FavoriteWindow();

            window.Owner = this.handle;
            window.File.Text = this.Filename.Text;
            window.Show();
            this.handle.Favorites.Add(1, this.Filename.Text);
        }
    }
}
