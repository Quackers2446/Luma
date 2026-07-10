using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;

namespace CozyAR.Utils
{
    public static class NativeGalleryHelper
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SaveImageToGalleryNative(string path);

        [DllImport("__Internal")]
        private static extern void SaveVideoToGalleryNative(string path);
#endif

        public static void SaveImage(string filePath, string albumName = "CozyAR")
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Image file not found: {filePath}");
                return;
            }

            Debug.Log($"Saving image to native gallery: {filePath}");

#if UNITY_EDITOR
            Debug.Log("[Editor] Simulating image save to native gallery.");
#elif UNITY_ANDROID
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", filePath))
                using (AndroidJavaObject uriObj = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObj))
                using (AndroidJavaObject intentObj = new AndroidJavaObject("android.content.Intent", "android.intent.action.MEDIA_SCANNER_SCAN_FILE", uriObj))
                {
                    currentActivity.Call("sendBroadcast", intentObj);
                }
                Debug.Log("Android MediaScanner broadcast triggered successfully for image.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to trigger MediaScanner on Android: {ex.Message}");
            }
#elif UNITY_IOS
            SaveImageToGalleryNative(filePath);
            Debug.Log("iOS SaveImageToGalleryNative called.");
#else
            Debug.LogWarning("SaveImage is not supported on this platform.");
#endif
        }

        public static void SaveVideo(string filePath, string albumName = "CozyAR")
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Video file not found: {filePath}");
                return;
            }

            Debug.Log($"Saving video to native gallery: {filePath}");

#if UNITY_EDITOR
            Debug.Log("[Editor] Simulating video save to native gallery.");
#elif UNITY_ANDROID
            try
            {
                using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", filePath))
                using (AndroidJavaObject uriObj = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObj))
                using (AndroidJavaObject intentObj = new AndroidJavaObject("android.content.Intent", "android.intent.action.MEDIA_SCANNER_SCAN_FILE", uriObj))
                {
                    currentActivity.Call("sendBroadcast", intentObj);
                }
                Debug.Log("Android MediaScanner broadcast triggered successfully for video.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save video on Android: {ex.Message}");
            }
#elif UNITY_IOS
            SaveVideoToGalleryNative(filePath);
            Debug.Log("iOS SaveVideoToGalleryNative called.");
#else
            Debug.LogWarning("SaveVideo is not supported on this platform.");
#endif
        }
    }
}
