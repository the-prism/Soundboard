// <copyright file="LeftMarginMultiplierConverter.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// File from https://github.com/TanyaPristupova/WpfOfficeTheme
    /// </summary>
    public class LeftMarginMultiplierConverter : IValueConverter
    {
        /// <summary>Margin length</summary>
        public double Length { get; set; }

        /// <summary>Convert margin</summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>New margin</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
            {
                return new Thickness(0);
            }

            return new Thickness(this.Length * item.GetDepth(), 0, 0, 0);
        }

        /// <summary>Convert the value back</summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Not implemented</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}