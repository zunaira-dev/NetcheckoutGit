using System;
using System.Runtime.InteropServices;

using UnityEngine;

namespace NetCheckout
{
    /// <summary>
    /// Base class for checkout clients.
    /// </summary>
    public abstract class BaseClient
    {
        // windows os only
        [DllImport("user32.dll")] protected static extern uint GetActiveWindow();
        [DllImport("user32.dll")] protected static extern bool SetForegroundWindow(IntPtr hWnd);

        protected Settings settings;

        protected IntPtr windowHandle;

        public BaseClient()
        {
            settings = Resources.Load<Settings>("Config/Settings");

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            windowHandle = (IntPtr)GetActiveWindow();
#endif
        }

        public abstract void SetClientSecret(string secret);
    }
}