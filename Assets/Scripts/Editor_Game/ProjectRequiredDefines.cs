#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
// Required project macro; preserve platform, protection and all other project symbols.
[InitializeOnLoad]
public class ProjectRequiredDefines
{
    public const string REQUIRED_DEFINE = "USE_QI_NIU_YUN";
    static ProjectRequiredDefines()
    {
        EditorApplication.delayCall += enforce;
    }
    public static string includeRequired(string definitions)
    {
        var values = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string entry in (definitions ?? "").Split(';'))
        {
            string value = entry.Trim();
            if (value.Length != 0 && seen.Add(value)) values.Add(value);
        }
        if (seen.Add(REQUIRED_DEFINE)) values.Add(REQUIRED_DEFINE);
        return string.Join(";", values);
    }
    public static void enforce()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
        {
            EditorApplication.delayCall += enforce;
            return;
        }
        var groups = new HashSet<BuildTargetGroup> { BuildTargetGroup.Standalone,
            BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.WebGL,
            BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget) };
        groups.Remove(BuildTargetGroup.Unknown);
        foreach (BuildTargetGroup group in groups)
        {
            string current = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string required = includeRequired(current);
            if (current != required) PlayerSettings.SetScriptingDefineSymbolsForGroup(group, required);
        }
    }
}
#endif
