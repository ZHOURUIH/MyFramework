using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildAPK : EditorCommonUtility
{
	public static void buildAndroidAPK()
	{
		string locationPath = "";
		string[] lines = StringUtility.split(Environment.CommandLine, true, " ");
		for(int i = 0; i < lines.Length; ++i)
		{
			string cmdName = lines[i];
			if (cmdName[0] == '-' && cmdName == "-apkpath")
			{
				if(i + 1 >= lines.Length)
				{
					Debug.LogError("参数不足, -apkpath后应该有apk绝对路径");
					return;
				}
				string cmdValue = lines[i + 1];
				string apkFile = StringUtility.getFileName(cmdValue);
				if(!StringUtility.endWith(apkFile, ".apk"))
				{
					Debug.LogError("文件名错误,应该保存为apk格式的文件");
					return;
				}
				// 创建路径
				string filePath = StringUtility.getFilePath(cmdValue);
				if (!FileUtility.isDirExist(filePath))
				{
					FileUtility.createDir(filePath);
				}
				// 删除已存在的文件
				if(FileUtility.isFileExist(cmdValue))
				{
					FileUtility.deleteFile(cmdValue);
				}
				locationPath = cmdValue;
				break;
			}
		}
		if(locationPath == "")
		{
			Debug.LogError("没有找到-apkpath参数");
			return;
		}
		Debug.Log("开始生成APK:" + locationPath);
		DateTime start = DateTime.Now;
		BuildPlayerOptions options = new BuildPlayerOptions();
		options.scenes = new string[] { FrameDefine.P_RESOURCES_SCENE_PATH + "start.unity" };
		options.locationPathName = locationPath;
		options.targetGroup = BuildTargetGroup.Android;
		options.target = BuildTarget.Android;
		var result = BuildPipeline.BuildPlayer(options);
		Debug.Log("生成APK结束:" + result.ToString() + ", 耗时:" + (DateTime.Now - start));
	}
}