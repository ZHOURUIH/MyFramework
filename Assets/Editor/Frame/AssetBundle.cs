using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class AssetBuildBundleInfo
{
	public List<string> mAssetNames;	// 带Resources下相对路径,带后缀
	public List<string> mDependencies;	// 所有依赖的AssetBundle
	public string mBundleName;			// 所属AssetBundle
	public AssetBuildBundleInfo(string bundleName)
	{
		mAssetNames = new List<string>();
		mDependencies = new List<string>();
		mBundleName = bundleName;
	}
	public void AddDependence(string dep)
	{
		mDependencies.Add(dep);
	}
}

public class AssetBundlePack : EditorCommonUtility
{
	protected const string mAssetMenuRoot = "AssetBundle/";
	// key为AssetBundle名,带Resources下相对路径,带后缀,Value是该AssetBundle中包含的所有Asset
	private static Dictionary<string, AssetBuildBundleInfo> mAssetBundleMap = new Dictionary<string, AssetBuildBundleInfo>();
	protected static Dictionary<string, HashSet<string>> mDependencyList = new Dictionary<string, HashSet<string>>();
	[MenuItem(mAssetMenuRoot + "清除AssetBundle名称")]
	public static void clearAllAssetBundleName()
	{
		clearAssetBundleName();
	}
	[MenuItem(mAssetMenuRoot + "pack/Android")]
	public static void packAssetBundleAndroid()
	{
		packAssetBundle(BuildTarget.Android);
	}
	[MenuItem(mAssetMenuRoot + "pack/Windows")]
	public static void packAssetBundleWindows()
	{
		packAssetBundle(BuildTarget.StandaloneWindows);
	}
	[MenuItem(mAssetMenuRoot + "pack/IOS")]
	public static void packAssetBundleiOS()
	{
		packAssetBundle(BuildTarget.iOS);
	}
	// subPath为以Asset开头的相对路径
	public static void packAssetBundle(BuildTarget target, string subPath = "")
	{
		if (isEmpty(subPath))
		{
			subPath = AssetDatabase.GetAssetPath(Selection.activeObject);
		}
		AssetBundleBuild[] buildList = null;
		if (!isEmpty(subPath))
		{
			if (!EditorUtility.DisplayDialog("打包", "确认打包" + subPath + "?", "确认", "取消"))
			{
				return;
			}
			findAssetBundleBuild(subPath, ref buildList);
		}
		else
		{
			Debug.Log("打包全部AssetBundle");
		}
		DateTime time0 = DateTime.Now;
		if (buildList != null)
		{
			// 部分打包,仅重新生成资源包文件,清单文件也会一起生成,但是由于只是部分打包,所以依赖项可能没有打包,需要手动还原清单文件
			int count = buildList.Length;
			for (int i = 0; i < count; ++i)
			{
				string bundleFileName = FrameDefine.P_STREAMING_ASSETS_PATH + buildList[i].assetBundleName;
				if (File.Exists(bundleFileName))
				{
					File.Delete(bundleFileName);
				}
			}
			BuildPipeline.BuildAssetBundles(FrameDefine.P_STREAMING_ASSETS_PATH, buildList, BuildAssetBundleOptions.ChunkBasedCompression, target);
			AssetDatabase.Refresh();
		}
		else
		{
			// 清理输出目录
			createOrClearOutPath();
			// 清理不打包的AssetBundle名
			List<string> allFiles = new List<string>();
			findFiles(FrameDefine.F_GAME_RESOURCES_PATH, allFiles);
			clearUnPackAssetBundleName(allFiles, GameDefine.mUnPackFolder);
			// 设置bunderName
			mAssetBundleMap.Clear();
			List<string> resList = new List<string>();
			getAllSubResDirs(FrameDefine.P_GAME_RESOURCES_PATH, resList);
			foreach (string dir in resList)
			{
				if(!setAssetBundleName(dir))
				{
					return;
				}
			}
			// 打包
			BuildPipeline.BuildAssetBundles(FrameDefine.P_STREAMING_ASSETS_PATH, BuildAssetBundleOptions.ChunkBasedCompression, target);
			AssetDatabase.Refresh();

			// 构建依赖关系
			mDependencyList.Clear();
			AssetBundle assetBundle = AssetBundle.LoadFromFile(FrameDefine.F_STREAMING_ASSETS_PATH + "StreamingAssets");
			AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			string[] assetBundleNameList = manifest.GetAllAssetBundles();
			// 遍历所有AB
			foreach (string bundle in assetBundleNameList)
			{
				string bundleName = bundle;
				if (!mAssetBundleMap.ContainsKey(bundleName))
				{
					continue;
				}
				rightToLeft(ref bundleName);
				string[] deps = manifest.GetAllDependencies(bundleName);
				HashSet<string> dependencySet = new HashSet<string>();
				// 遍历当前AB的所有依赖项
				foreach (string dep in deps)
				{
					string depName = rightToLeft(dep);
					mAssetBundleMap[bundleName].AddDependence(depName);
					dependencySet.Add(depName);
					// 查找依赖项中是否有依赖当前AssetBundle的
					if(mDependencyList.ContainsKey(depName) && mDependencyList[depName].Contains(bundleName))
					{
						messageBox("AssetBundle dependency error! " + depName + ", " + bundleName, true);
					}
				}
				if(mDependencyList.ContainsKey(bundleName))
				{
					messageBox("已经存在一个名为:" + bundleName + "的AssetBundle", true);
				}
				mDependencyList.Add(bundleName, dependencySet);
			}

			// 生成配置文件
			Serializer serializer = new Serializer();
			serializer.write(mAssetBundleMap.Count);
			foreach (var item in mAssetBundleMap)
			{
				AssetBuildBundleInfo bundleInfo = item.Value;
				// AssetBundle名字
				serializer.writeString(bundleInfo.mBundleName);
				// AssetBundle所包含的所有Asset名字
				int assetCount = bundleInfo.mAssetNames.Count;
				serializer.write(assetCount);
				for (int i = 0; i < assetCount; ++i)
				{
					serializer.writeString(bundleInfo.mAssetNames[i]);
				}
				// AssetBundle依赖的所有AssetBundle
				int depCount = bundleInfo.mDependencies.Count;
				serializer.write(depCount);
				for (int j = 0; j < depCount; ++j)
				{
					serializer.writeString(bundleInfo.mDependencies[j]);
				}
			}
			string filePath = FrameDefine.F_STREAMING_ASSETS_PATH + "StreamingAssets.bytes";
			writeFile(filePath, serializer.getBuffer(), serializer.getDataSize());
		}
		messageBox("资源打包结束! 耗时 : " + (DateTime.Now - time0), false);
	}
	protected static void findAssetBundleBuild(string path, ref AssetBundleBuild[] list)
	{
		Dictionary<string, List<string>> assetBundleList = new Dictionary<string, List<string>>();
		// path是文件
		if (!isEmpty(getFileSuffix(path)))
		{
			string bundleName = refreshFileAssetBundleName(path);
			if(bundleName == null)
			{
				return;
			}
			if (!isEmpty(bundleName))
			{
				if (!assetBundleList.ContainsKey(bundleName))
				{
					assetBundleList.Add(bundleName, new List<string>());
				}
				assetBundleList[bundleName].Add(path);
			}
		}
		// path是目录
		else
		{
			List<string> dirList = new List<string>();
			getAllSubResDirs(path, dirList);
			int dirCount = dirList.Count;
			for (int i = 0; i < dirCount; ++i)
			{
				string[] files = Directory.GetFiles(dirList[i]);
				int fileCount = files.Length;
				for (int j = 0; j < fileCount; ++j)
				{
					string bundleName = refreshFileAssetBundleName(files[j]);
					if(isEmpty(bundleName))
					{
						continue;
					}
					if (!assetBundleList.ContainsKey(bundleName))
					{
						assetBundleList.Add(bundleName, new List<string>());
					}
					assetBundleList[bundleName].Add(files[j]);
				}
			}
		}
		list = new AssetBundleBuild[assetBundleList.Count];
		int index = 0;
		foreach (var item in assetBundleList)
		{
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = item.Key;
			int assetCount = item.Value.Count;
			build.assetNames = new string[assetCount];
			for (int i = 0; i < assetCount; ++i)
			{
				build.assetNames[i] = item.Value[i];
			}
			list[index++] = build;
		}
		EditorUtility.UnloadUnusedAssetsImmediate();
	}
	// 获得一个文件的所属AssetBundle名,file是以Assets开头的相对路径
	protected static string getFileAssetBundleName(string file, bool forceSingle = false)
	{
		string bundleName;
		// unity(但是一般情况下unity场景文件不打包)单个文件打包,就是直接替换后缀名,或者强制为单独一个包的
		if (endWith(file, ".unity") || forceSingle)
		{
			bundleName = removeStartString(file, FrameDefine.P_GAME_RESOURCES_PATH, false);
			bundleName = replaceSuffix(bundleName, FrameDefine.ASSET_BUNDLE_SUFFIX);
		}
		// 其他文件的AssetBundle就是所属文件夹
		else
		{
			bundleName = removeStartString(getFilePath(file), FrameDefine.P_GAME_RESOURCES_PATH, false);
			bundleName += FrameDefine.ASSET_BUNDLE_SUFFIX;
		}
		return bundleName;
	}
	// 判断一个路径是否是不需要打包的路径
	protected static bool isUnpackPath(string path, string[] unpackList)
	{
		string pathUnderResources = removeStartString(path, FrameDefine.P_GAME_RESOURCES_PATH, false) + "/";
		rightToLeft(ref pathUnderResources);
		int unpackCount = unpackList.Length;
		for (int i = 0; i < unpackCount; ++i)
		{
			// 如果该文件夹是不打包的文件夹,则直接返回
			if (startWith(pathUnderResources, unpackList[i], false))
			{
				return true;
			}
		}
		return false;
	}
	// 刷新指定文件的所属AssetBundle名字
	protected static string refreshFileAssetBundleName(string file, bool forceSingle = false)
	{
		if (endWith(file, ".meta"))
		{
			return "";
		}
		AssetImporter importer = AssetImporter.GetAtPath(file);
		if (importer == null)
		{
			Debug.LogError("Set AssetName Fail, File:" + file);
			return "";
		}
		// tpsheet文件不打包
		// LightingData.asset文件不能打包AB,这是一个特殊文件,只用于编辑器
		if (endWith(file, ".tpsheet") || endWith(file, "LightingData.asset"))
		{
			importer.assetBundleName = "";
			return "";
		}
		string fileName = rightToLeft(file.ToLower());
		string bundleName = getFileAssetBundleName(fileName, forceSingle);
		if (importer.assetBundleName != bundleName)
		{
			importer.assetBundleName = bundleName;
		}
		// 存储bundleInfo
		// 去除Asset/GameResources/前缀,只保留GameResources下相对路径
		string assetName = removeStartString(fileName, FrameDefine.P_GAME_RESOURCES_PATH, false);
		if (!mAssetBundleMap.ContainsKey(bundleName))
		{
			mAssetBundleMap.Add(bundleName, new AssetBuildBundleInfo(bundleName));
		}
		mAssetBundleMap[bundleName].mAssetNames.Add(assetName);
		return bundleName;
	}
	// fullPath是以Asset开头的路径
	protected static bool setAssetBundleName(string fullPath)
	{
		if (isUnpackPath(fullPath, GameDefine.mUnPackFolder))
		{
			return true;
		}
		string[] files = Directory.GetFiles(fullPath);
		if (files == null || files.Length == 0)
		{
			return true;
		}
		foreach (string file in files)
		{
			if(refreshFileAssetBundleName(rightToLeft(file)) == null)
			{
				return false;
			}
		}
		EditorUtility.UnloadUnusedAssetsImmediate();
		return true;
	}
	// 递归获取所有子目录文件夹
	protected static void getAllSubResDirs(string fullPath, List<string> dirList)
	{
		if (dirList == null || isEmpty(fullPath))
		{
			return;
		}
		string[] dirs = Directory.GetDirectories(fullPath);
		if (dirs != null && dirs.Length > 0)
		{
			for (int i = 0; i < dirs.Length; ++i)
			{
				rightToLeft(ref dirs[i]);
				getAllSubResDirs(dirs[i], dirList);
			}
		}
		dirList.Add(fullPath);
	}
	// 创建和清理输出目录
	protected static void createOrClearOutPath()
	{
		if (!Directory.Exists(FrameDefine.P_STREAMING_ASSETS_PATH))
		{
			Directory.CreateDirectory(FrameDefine.P_STREAMING_ASSETS_PATH);
		}
		else
		{
			// 查找目录下的所有第一级子目录
			string[] dirList = Directory.GetDirectories(FrameDefine.P_STREAMING_ASSETS_PATH);
			int dirCount = dirList.Length;
			for (int i = 0; i < dirCount; ++i)
			{
				// 只删除不需要保留的目录
				if (!isKeepFolderOrMeta(getFolderName(dirList[i])))
				{
					Directory.Delete(dirList[i], true);
				}
			}
			// 查找目录下的所有第一级文件
			string[] files = Directory.GetFiles(FrameDefine.P_STREAMING_ASSETS_PATH);
			int fileCount = files.Length;
			for (int i = 0; i < fileCount; ++i)
			{
				if (!isKeepFolderOrMeta(getFileName(files[i])))
				{
					File.Delete(files[i]);
				}
			}
		}
	}
	protected static bool isKeepFolderOrMeta(string name)
	{
		int count = GameDefine.mKeepFolder.Length;
		for (int i = 0; i < count; ++i)
		{
			if (GameDefine.mKeepFolder[i] == name || GameDefine.mKeepFolder[i] + ".meta" == name)
			{
				return true;
			}
		}
		return false;
	}
	// 清理之前设置的bundleName
	protected static void clearAssetBundleName()
	{
		string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
		int length = bundleNames.Length;
		for (int i = 0; i < length; ++i)
		{
			AssetDatabase.RemoveAssetBundleName(bundleNames[i], true);
			Debug.Log("remove asset bundle name : " + bundleNames[i]);
		}
	}
	protected static void clearUnPackAssetBundleName(List<string> fileList, string[] unpackList)
	{
		foreach(var file in fileList)
		{
			if (endWith(file, ".meta"))
			{
				continue;
			}
			string projectFileName = fullPathToProjectPath(file);
			string fileName = removeStartString(projectFileName, FrameDefine.P_GAME_RESOURCES_PATH, false);
			foreach (var unpack in unpackList)
			{
				if (startWith(fileName, unpack, false))
				{
					AssetImporter importer = AssetImporter.GetAtPath(projectFileName);
					if (importer != null)
					{
						if (!isEmpty(importer.assetBundleName))
						{
							importer.assetBundleName = "";
						}
					}
					break;
				}
			}
		}
		AssetDatabase.Refresh();
	}
}