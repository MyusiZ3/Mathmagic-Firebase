using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildSettingsHelper : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Limit the number of parallel compiler threads to reduce RAM usage during IL2CPP compilation
        PlayerSettings.SetAdditionalIl2CppArgs("--jobs=4");
        UnityEngine.Debug.Log("[BuildSettingsHelper] Automatically set IL2CPP job concurrency limit to 4 threads.");
    }
}
