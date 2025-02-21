// <copyright file="FavoriteWindow.xaml.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Collections.Generic;
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
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for FavoriteWindow.xaml
    /// </summary>
    public partial class FavoriteWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FavoriteWindow"/> class.
        /// </summary>
        public FavoriteWindow()
        {
            this.InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.AddToDictionary(1);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.AddToDictionary(2);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.AddToDictionary(3);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.AddToDictionary(4);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            this.AddToDictionary(5);
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            var dictionary = (this.Owner as MainWindow)?.Favorites;
            if (dictionary.ContainsValue(this.File.Text))
            {
                int key = dictionary.First(m => m.Value == this.File.Text).Key;
                dictionary.Remove(key);

                (this.Owner as MainWindow)?.Refresh();
            }

            this.Close();
        }

        private void AddToDictionary(int key)
        {
            var dictionary = (this.Owner as MainWindow)?.Favorites;
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = this.File.Text;
            }
            else
            {
                dictionary.Add(key, this.File.Text);
            }

            (this.Owner as MainWindow)?.Refresh();
            this.Close();
        }

    }
}
