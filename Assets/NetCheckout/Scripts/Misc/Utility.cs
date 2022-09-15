using System;
using System.Text;
using System.Runtime.InteropServices;

using UnityEngine;

namespace NetCheckout
{
    public static class Utility
    {
#if UNITY_EDITOR
#elif UNITY_IOS
        [DllImport("__Internal")]
        public static extern void OpenNativeUrl(string url);
        [DllImport("__Internal")]
        public static extern void CloseNativeUrl();
#elif UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OpenLink(string url, bool newTab);
#endif

        /// <summary>
        /// Opens the url in a browser. For desktop, url will always open in a new tab.
        /// </summary>
        /// <param name="url">i.e. https://www.mywebsite.com</param>
        /// <param name="newTab">If true, opens the url in a new browser tab. Has no effect in desktop builds.</param>
        public static void OpenURL(string url, bool newTab = true)
        {
#if UNITY_EDITOR
            System.Diagnostics.Process.Start(url);
#elif UNITY_IOS
            OpenNativeUrl(url);
#elif UNITY_ANDROID
            AndroidHelper.OpenNativeUrl(url);
#elif UNITY_WEBGL
            OpenLink(url, newTab);
#else
            System.Diagnostics.Process.Start(url);
#endif
        }

        /// <summary>
        /// Returns input string in base-64 encoded format.
        /// </summary>
        public static string GetBase64(string input)
        {
            return Convert.ToBase64String(GetBytes(input));
        }

        /// <summary>
        /// Converts input string to bytes.
        /// </summary>
        public static byte[] GetBytes(string input)
        {
            return Encoding.ASCII.GetBytes(input);
        }

        /// <summary>
        /// Converts bytes to UTF8 string.
        /// </summary>
        public static string GetUTF8String(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Returns the current UTC time in ISO8601 format.
        /// </summary>
        /// <returns></returns>
        public static string GetISO8601Time()
        {
            return DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}