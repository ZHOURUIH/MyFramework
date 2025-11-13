#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class UnityiOSPostProcessBuildSetting
{
    //括号(10)是函数调用的优先度
    [PostProcessBuild(10)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // 修改info.plist的代码
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;
            if (!rootDict.values.TryGetValue("LSApplicationQueriesSchemes", out PlistElement array))
            {
                array = rootDict.CreateArray("LSApplicationQueriesSchemes");
            }
			array.AsArray().AddString("mqqapi");

			// 缺少合规证明
			plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            // 保存plist
            plist.WriteToFile(plistPath);

            // 修改项目
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new();
            proj.ReadFromString(File.ReadAllText(projPath));

            //// 获取当前项目名字
            string target = proj.GetUnityMainTargetGuid();
            string frameworkGuid = proj.GetUnityFrameworkTargetGuid();

            //// 对所有的编译配置设置选项
            //proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
            //proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            //proj.SetBuildProperty(target, "VALIDATE_WORKSPACE", "YES");

            // 在UnityFramework中添加AppleAuth依赖
            proj.AddFrameworkToProject(frameworkGuid, "AuthenticationServices.framework", false);

            //proj.AddFrameworkToProject(target, "HuoSuperSDK.framework", false);
            //// 添加依赖库,bugly需要
            //proj.AddFrameworkToProject(target, "Security.framework", false);
            //proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
            //proj.AddFrameworkToProject(target, "JavaScriptCore.framework", false);
            //proj.AddFrameworkToProject(target, "libc++.tdb", false);
            //proj.AddFrameworkToProject(target, "libz.tdb", false);

            //string frameworkTarget = proj.GetUnityFrameworkTargetGuid();
            //proj.SetBuildProperty(frameworkTarget, "ENABLE_BITCODE", "NO");

            // 保存工程
            proj.WriteToFile(projPath);
		}
    }
    static void AddLibToProject(PBXProject inst, string targetGuid, string lib)
    {
        string fileGuid = inst.AddFile("usr/lib/" + lib, "Frameworks/" + lib, PBXSourceTree.Sdk);
        inst.AddFileToBuild(targetGuid, fileGuid);
    }
}
#endif