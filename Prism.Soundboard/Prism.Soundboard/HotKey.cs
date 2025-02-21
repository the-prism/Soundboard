// <copyright file="HotKey.cs" company="the-prism">
// Copyright (c) the-prism. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1121 // Use built-in type alias
namespace UnManaged
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Input;
    using System.Windows.Interop;

    /// <summary>Keymodifiers flags</summary>
    [Flags]
    public enum KeyModifier
    {
        /// <summary>No modifiers</summary>
        None = 0x0000,

        /// <summary>Alt key</summary>
        Alt = 0x0001,

        /// <summary>Ctrl key</summary>
        Ctrl = 0x0002,

        /// <summary>No repeat key</summary>
        NoRepeat = 0x4000,

        /// <summary>Shift key</summary>
        Shift = 0x0004,

        /// <summary>Windows key</summary>
        Win = 0x0008,
    }

    /// <summary>Class to handle win32 hotkeys</summary>
    public class HotKey : IDisposable
    {
        /// <summary>Hotkey event id</summary>
        public const int WmHotKey = 0x0312;

        private static Dictionary<int, HotKey> dictHotKeyToCalBackProc;
        private bool disposed = false;

        /// <summary>Initializes a new instance of the <see cref="HotKey"/> class.</summary>
        /// <param name="k">Key to register</param>
        /// <param name="keyModifiers">Modifiers to register</param>
        /// <param name="action">Callback to register</param>
        /// <param name="register">Should the keybind be registered with the system</param>
        public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, bool register = true)
        {
            this.Key = k;
            this.KeyModifiers = keyModifiers;
            this.Action = action;
            if (register)
            {
                this.Register();
            }
        }

        /// <summary>Gets key pressed</summary>
        public Key Key { get; private set; }

        /// <summary>Gets modfiers pressed</summary>
        public KeyModifier KeyModifiers { get; private set; }

        /// <summary>Gets callback for the keypress</summary>
        public Action<HotKey> Action { get; private set; }

        /// <summary>Registered id for the keybind</summary>
        public int Id { get; set; }

        /// <summary>Register the keybind</summary>
        /// <returns>If the registration was successfull</returns>
        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(this.Key);
            this.Id = virtualKeyCode + ((int)this.KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, this.Id, (UInt32)this.KeyModifiers, (UInt32)virtualKeyCode);

            if (dictHotKeyToCalBackProc == null)
            {
                dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            dictHotKeyToCalBackProc.Add(this.Id, this);

            Debug.Print(result.ToString() + ", " + this.Id + ", " + virtualKeyCode);
            return result;
        }

        /// <summary>Unregister the keybind</summary>
        public void Unregister()
        {
            HotKey hotKey;
            if (dictHotKeyToCalBackProc.TryGetValue(this.Id, out hotKey))
            {
                UnregisterHotKey(IntPtr.Zero, this.Id);
            }
        }

        /// <summary>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be _disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be _disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    this.Unregister();
                }

                // Note disposing has been done.
                this.disposed = true;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // ******************************************************************
        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WmHotKey)
                {
                    HotKey hotKey;

                    if (dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey))
                    {
                        if (hotKey.Action != null)
                        {
                            hotKey.Action.Invoke(hotKey);
                        }

                        handled = true;
                    }
                }
            }
        }
    }
}

#pragma warning restore SA1121 // Use built-in type alias
