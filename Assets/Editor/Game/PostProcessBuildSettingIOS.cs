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
			rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            // 保存plist
            plist.WriteToFile(plistPath);

            // 修改项目
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new();
            proj.ReadFromString(File.ReadAllText(projPath));

            //// 获取当前项目名字
            string target = proj.GetUnityMainTargetGuid();
            string frameworkGuid = proj.GetUnityFrameworkTargetGuid();

			// 对所有的编译配置设置选项
			proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lz -lc++ -ObjC");
			proj.SetBuildProperty(target, "PRODUCT_NAME", "这里设置你的游戏名字");

			// 在UnityFramework中添加AppleAuth依赖
			proj.AddFrameworkToProject(frameworkGuid, "AuthenticationServices.framework", false);

			// 添加依赖库,bugly需要
			proj.AddFrameworkToProject(target, "Security.framework", false);
			proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
			proj.AddFrameworkToProject(target, "JavaScriptCore.framework", false);
			proj.AddFrameworkToProject(target, "libc++.tbd", false);
			proj.AddFrameworkToProject(target, "libz.tbd", false);
			proj.AddFrameworkToProject(frameworkGuid, "libz.tbd", false);
			proj.AddFrameworkToProject(frameworkGuid, "libc++.tbd", false);

			// 保存工程
			proj.WriteToFile(projPath);
			// 修改UnityAppController,注入bugly的初始化代码
			AddBuglyToAppDelegate(path, "ios的bugly AppID");
		}
    }
	static void AddBuglyToAppDelegate(string xcodePath, string buglyAppId)
	{
		string appControllerPath = Path.Combine(xcodePath, "Classes/UnityAppController.mm");
		if (!File.Exists(appControllerPath))
		{
			Debug.LogError("未找到 UnityAppController.mm,无法注入 Bugly");
			return;
		}

		string content = File.ReadAllText(appControllerPath);
		// 避免重复注入
		if (content.Contains("BEGIN Bugly Injection"))
		{
			Debug.Log("Bugly 注入已存在，跳过。");
			return;
		}

		// 注入 import 部分
		string importLine = "\n// BEGIN Bugly Injection\n#import <Bugly/Bugly.h>\n";
		string importCode = importLine + "// END Bugly Injection";

		if (!content.Contains("#import <Bugly/Bugly.h>"))
		{
			// 注入到文件最前面（所有 import 之后）
			int insertIndex = content.IndexOf("#import");
			if (insertIndex >= 0)
			{
				content = content.Insert(content.IndexOf("\n", insertIndex) + 1, importCode + "\n");
			}
			else
			{
				content = importCode + "\n" + content;
			}
		}

		// 在 didFinishLaunchingWithOptions 方法中注入初始化代码
		string methodHeader = "didFinishLaunchingWithOptions";
		int methodIndex = content.IndexOf(methodHeader);
		if (methodIndex < 0)
		{
			Debug.LogError("未找到 didFinishLaunchingWithOptions 方法，无法注入 Bugly 初始化代码！");
			return;
		}

		// 找到方法体起始位置
		int braceIndex = content.IndexOf("{", methodIndex);
		if (braceIndex < 0)
		{
			Debug.LogError("didFinishLaunchingWithOptions 方法格式错误！");
			return;
		}

		// 注入代码
		string initCode = $@"
    // BEGIN Bugly Injection
    [Bugly startWithAppId:@""{buglyAppId}""];
    // END Bugly Injection
";

		content = content.Insert(braceIndex + 1, initCode);
		// 最终写回
		File.WriteAllText(appControllerPath, content);
		Debug.Log("成功自动注入 Bugly 到 UnityAppController.mm");
	}
}
#endif