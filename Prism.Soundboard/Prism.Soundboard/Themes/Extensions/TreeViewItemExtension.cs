// <copyright file="TreeViewItemExtension.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Prism.Soundboard
{
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// File from https://github.com/TanyaPristupova/WpfOfficeTheme
    /// </summary>
    public static class TreeViewItemExtension
    {
        /// <summary>Get the depth of a tree item</summary>
        /// <param name="item"></param>
        /// <returns>The depth value</returns>
        public static int GetDepth(this TreeViewItem item)
        {
            var parent = GetParent(item);
            while (GetParent(item) != null)
            {
                return GetDepth(parent) + 1;
            }

            return 0;
        }

        /// <summary>Get the parent of a tree item</summary>
        /// <param name="item"></param>
        /// <returns>The parent</returns>
        public static TreeViewItem GetParent(this TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                if (parent == null)
                {
                    return null;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TreeViewItem;
        }
    }
}