using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class RestartApp : MonoBehaviour
{
    public void RestartGame()
    {
        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        yield return new WaitForSeconds(0.5f); // Delay sebentar untuk efek transisi

        #if UNITY_ANDROID
            RestartOnAndroid();
        #elif UNITY_STANDALONE_WIN
            RestartOnWindows();
        #endif
    }

    private void RestartOnAndroid()
    {
        string packageName = Application.identifier; // Ambil package name aplikasi
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        AndroidJavaObject intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
        
        intent.Call<AndroidJavaObject>("addFlags", 0x20000000); // FLAG_ACTIVITY_CLEAR_TOP
        
        currentActivity.Call("startActivity", intent); // Mulai ulang aplikasi
        currentActivity.Call("finish"); // Tutup aplikasi lama
        System.Diagnostics.Process.GetCurrentProcess().Kill(); // Paksa keluar
    }

    private void RestartOnWindows()
    {
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        System.Diagnostics.Process.Start(exePath); // Buka ulang aplikasi
        Application.Quit(); // Tutup aplikasi lama
    }
}
