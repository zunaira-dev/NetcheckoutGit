using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetCheckout
{
    /// <summary>
    /// Interacts with the Android plugin
    /// </summary>
    public static class AndroidHelper
    {
        /// <summary>
        /// Opens android browser window and navigates to the specified url.
        /// </summary>
        /// <param name="url"></param>
        public static void OpenNativeUrl(string url)
        {
            AndroidJavaObject nativeUrl = GetNativeUrlInstance();
            
            nativeUrl.Call("openUrl", url);
        }

        /// <summary>
        /// Configures the Android plugin to automatically close the browser window upon checkout completion.
        /// </summary>
        /// <param name="url">Client URL</param>
        /// <param name="key">Client secret key</param>
        /// <param name="timeInterval">Seconds between API calls to the client</param>
        /// <param name="validator">Key/value pair to check for successful checkout</param>
        /// <param name="appUri">Custom uri to link back to app</param>
        public static void SetupNativeUrlCloseEvent(string url, string key, float timeInterval, (string, string) validator, string appUri)
        {
            AndroidJavaObject nativeUrl = GetNativeUrlInstance();

            string[] validatorArray = new string[] { validator.Item1, validator.Item2 };
            nativeUrl.Call("closeOnEvent", url, key, timeInterval, validatorArray, appUri);
        }

        private static AndroidJavaObject GetNativeUrlInstance()
        {
            AndroidJavaClass @class = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = @class.GetStatic<AndroidJavaObject>("currentActivity");

            string package = Application.identifier;
            return new AndroidJavaObject("com.netcheckout.nativeurl.NativeUrl", activity, package);
        }
    }
}