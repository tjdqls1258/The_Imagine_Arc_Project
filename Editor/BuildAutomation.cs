using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.IO;
using UnityEditor.AddressableAssets.Build;

public class BuildAutomation
{
    [MenuItem("Build Automation/1. Full Build (Local Only)/Android")]
    public static void BuildFullLocalAndroid()
    {
        ExecuteFullBuild(BuildTargetGroup.Android, BuildTarget.Android, "Android");
    }

    [MenuItem("Build Automation/1. Full Build (Local Only)/StandaloneWindows")]
    public static void BuildFullLocalWindows()
    {
        ExecuteFullBuild(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "PC");
    }

    private static void ExecuteFullBuild(BuildTargetGroup targetGroup, BuildTarget target, string folderName)
    {
        Debug.Log($"=== {folderName} 풀빌드 프로세스 시작 ===");

        EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);

        ChangeAddressableProfile("Local");

        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"[어드레서블 빌드 실패] {result.Error}");
            return;
        }

        string extension = (target == BuildTarget.Android) ? "apk" : "exe";

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            
            locationPathName = $"Builds/{folderName}/FullRelease.{extension}",
            target = target, 
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);
        Debug.Log($"=== {folderName} 풀빌드 종료. 결과: {report.summary.result} ===");
    }

    [MenuItem("Build Automation/2. Addressables Only (Remote)")]
    public static void BuildAddressablesForCDN()
    {
        Debug.Log("=== 어드레서블 CDN(Remote) 빌드 시작 ===");

        ChangeAddressableProfile("Download"); 

        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"[어드레서블 빌드 실패] {result.Error}");
        }
        else
        {
            Debug.Log("=== 어드레서블 빌드 성공! ServerData 폴더의 내용물을 CDN에 업로드하세요. ===");
        }
    }

    private static void ChangeAddressableProfile(string profileName)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("AddressableAssetSettings를 찾을 수 없습니다.");
            return;
        }

        string profileId = settings.profileSettings.GetProfileId(profileName);
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogError($"'{profileName}' 프로필을 찾을 수 없습니다!");
            return;
        }

        settings.activeProfileId = profileId;
        Debug.Log($"어드레서블 프로필이 [{profileName}] (으)로 변경되었습니다.");
    }

    private static string[] GetEnabledScenes()
    {
        int sceneCount = EditorBuildSettings.scenes.Length;
        string[] scenes = new string[sceneCount];
        int enabledCount = 0;

        for (int i = 0; i < sceneCount; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                scenes[enabledCount] = EditorBuildSettings.scenes[i].path;
                enabledCount++;
            }
        }

        System.Array.Resize(ref scenes, enabledCount);
        return scenes;
    }
}