using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using static UnityUtility;
using static StringUtility;
using static FileUtility;
using static FrameDefine;

public class MenuAssetBundle
{
	// key为AssetBundle名,带Resources下相对路径,带后缀,Value是该AssetBundle中包含的所有Asset
	private static Dictionary<string, BuildAssetBundleInfo> mAssetBundleMap = new Dictionary<string, BuildAssetBundleInfo>();
	protected static Dictionary<string, HashSet<string>> mDependencyList = new Dictionary<string, HashSet<string>>();
	[MenuItem("AssetBundle/清除AssetBundle名称")]
	public static void clearAllAssetBundleName()
	{
		clearAssetBundleName();
	}
	[MenuItem("AssetBundle/Pack Android")]
	public static void packAssetBundleAndroid()
	{
		packAssetBundle(BuildTarget.Android, P_ASSET_BUNDLE_ANDROID_PATH);
	}
	[MenuItem("AssetBundle/Pack Windows")]
	public static void packAssetBundleWindows()
	{
		packAssetBundle(BuildTarget.StandaloneWindows, P_ASSET_BUNDLE_WINDOWS_PATH);
	}
	[MenuItem("AssetBundle/Pack iOS")]
	public static void packAssetBundleIOS()
	{
		packAssetBundle(BuildTarget.iOS, P_ASSET_BUNDLE_IOS_PATH);
	}
	[MenuItem("AssetBundle/Pack MacOS")]
	public static void packAssetBundleMacOS()
	{
		packAssetBundle(BuildTarget.StandaloneOSX, P_ASSET_BUNDLE_MACOS_PATH);
	}
	// pathToPack为以Asset开头的相对路径,不为空则表示只单独打包此目录或此文件
	public static bool packAssetBundle(BuildTarget target, string outputPath, string pathToPack = null, bool showMessageBox = true)
	{
		if (isEmpty(pathToPack))
		{
			pathToPack = AssetDatabase.GetAssetPath(Selection.activeObject);
		}
		AssetBundleBuild[] buildList = null;
		if (!isEmpty(pathToPack))
		{
			if (!EditorUtility.DisplayDialog("打包", "确认打包" + pathToPack + "?\n部分打包仅重新生成资源包文件,不会更新其依赖项,如果依赖项有改变,请使用全部打包", "确认", "取消"))
			{
				return false;
			}
			findAssetBundleBuild(pathToPack, ref buildList);
		}
		else
		{
			Debug.Log("打包全部AssetBundle");
		}
		DateTime time0 = DateTime.Now;
		if (buildList != null)
		{
			// 备份清单文件和生成的AssetBundle信息文件
			string folderName = getFolderName(outputPath);
			byte[] streamingAssetsBytes = openFile(outputPath + STREAMING_ASSET_FILE, false);
			byte[] streamingFileBytes = openFile(outputPath + folderName, false);
			byte[] manifestBytes = openFile(outputPath + folderName + ".manifest", false);
			int count = buildList.Length;
			for (int i = 0; i < count; ++i)
			{
				string bundleFileName = outputPath + buildList[i].assetBundleName;
				if (File.Exists(bundleFileName))
				{
					File.Delete(bundleFileName);
				}
			}
			// 使用LZ4压缩,并且不写入资源类型信息
			var option = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree;
			BuildPipeline.BuildAssetBundles(outputPath, buildList, option, target);
			AssetDatabase.Refresh();

			// 还原备份的文件
			if (streamingAssetsBytes != null)
			{
				writeFile(outputPath + STREAMING_ASSET_FILE, streamingAssetsBytes, streamingAssetsBytes.Length);
			}
			if (streamingFileBytes != null)
			{
				writeFile(outputPath + folderName, streamingFileBytes, streamingFileBytes.Length);
			}
			if (manifestBytes != null)
			{
				writeFile(outputPath + folderName + ".manifest", manifestBytes, manifestBytes.Length);
			}
		}
		else
		{
			// 清理输出目录
			createOrClearOutPath(outputPath);
			// 清理不打包的AssetBundle名
			List<string> allFiles = new List<string>();
			findFiles(F_GAME_RESOURCES_PATH, allFiles);
			clearUnPackAssetBundleName(allFiles, mUnPackFolder);
			// 设置bunderName
			mAssetBundleMap.Clear();
			List<string> resList = new List<string>();
			getAllSubResDirs(P_GAME_RESOURCES_PATH, resList);
			foreach (string dir in resList)
			{
				if (!generateAssetBundleName(dir))
				{
					return false;
				}
			}
			// 打包
			var option = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree;
			BuildPipeline.BuildAssetBundles(outputPath, option, target);
			AssetDatabase.Refresh();
			// 构建依赖关系
			mDependencyList.Clear();
			AssetBundle assetBundle = AssetBundle.LoadFromFile(projectPathToFullPath(outputPath) + getFolderName(outputPath));
			AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			string[] assetBundleNameList = manifest.GetAllAssetBundles();
			// 遍历所有AB
			foreach (string bundle in assetBundleNameList)
			{
				string bundleName = bundle;
				if (!mAssetBundleMap.TryGetValue(bundleName, out BuildAssetBundleInfo bundleInfo))
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
					bundleInfo.AddDependence(depName);
					dependencySet.Add(depName);
					// 查找依赖项中是否有依赖当前AssetBundle的
					if (mDependencyList.TryGetValue(depName, out HashSet<string> depList) && depList.Contains(bundleName))
					{
						Debug.LogError("AssetBundle dependency error! " + depName + ", " + bundleName);
						if (showMessageBox)
						{
							messageBox("AssetBundle dependency error! " + depName + ", " + bundleName, true);
						}
					}
				}
				if (mDependencyList.ContainsKey(bundleName))
				{
					Debug.LogError("已经存在一个名为:" + bundleName + "的AssetBundle");
					if (showMessageBox)
					{
						messageBox("已经存在一个名为:" + bundleName + "的AssetBundle", true);
					}
				}
				mDependencyList.Add(bundleName, dependencySet);
			}

			// 生成配置文件
			var serializer = new SerializerWrite();
			serializer.write(mAssetBundleMap.Count);
			foreach (var item in mAssetBundleMap)
			{
				BuildAssetBundleInfo bundleInfo = item.Value;
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
			writeFile(outputPath + STREAMING_ASSET_FILE, serializer.getBuffer(), serializer.getDataSize());
		}
		Debug.Log("资源打包结束! 耗时 : " + (DateTime.Now - time0));
		if (showMessageBox)
		{
			messageBox("资源打包结束! 耗时 : " + (DateTime.Now - time0), false);
		}
		return true;
	}
	protected static void findAssetBundleBuild(string path, ref AssetBundleBuild[] list)
	{
		Dictionary<string, List<string>> assetBundleList = new Dictionary<string, List<string>>();
		// path是文件
		if (!isEmpty(getFileSuffix(path)))
		{
			string bundleName = refreshFileAssetBundleName(path);
			if (bundleName == null)
			{
				return;
			}
			if (!isEmpty(bundleName))
			{
				if (!assetBundleList.TryGetValue(bundleName, out List<string> pathList))
				{
					pathList = new List<string>();
					assetBundleList.Add(bundleName, pathList);
				}
				pathList.Add(path);
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
					if (isEmpty(bundleName))
					{
						continue;
					}
					if (!assetBundleList.TryGetValue(bundleName, out List<string> pathList))
					{
						pathList = new List<string>();
						assetBundleList.Add(bundleName, pathList);
					}
					pathList.Add(files[j]);
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
				Debug.Log("部分打包的文件名:" + item.Value[i] + ", 所属AssetBundle:" + item.Key);
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
			bundleName = removeStartString(file, P_GAME_RESOURCES_PATH, false);
			bundleName = replaceSuffix(bundleName, ASSET_BUNDLE_SUFFIX);
		}
		// 其他文件的AssetBundle就是所属文件夹
		else
		{
			bundleName = removeStartString(getFilePath(file), P_GAME_RESOURCES_PATH, false);
			bundleName += ASSET_BUNDLE_SUFFIX;
		}
		return bundleName;
	}
	// 判断一个路径是否是不需要打包的路径
	protected static bool isUnpackPath(string path, string[] unpackList)
	{
		string pathUnderResources = removeStartString(path, P_GAME_RESOURCES_PATH, false) + "/";
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
	// 判断一个路径是否是不需要打包的路径
	protected static bool isForceSinglePath(string path, string[] singlePathList)
	{
		string pathUnderResources = removeStartString(path, P_GAME_RESOURCES_PATH, false) + "/";
		rightToLeft(ref pathUnderResources);
		int unpackCount = singlePathList.Length;
		for (int i = 0; i < unpackCount; ++i)
		{
			// 如果该文件夹是不打包的文件夹,则直接返回
			if (pathUnderResources == singlePathList[i])
			{
				return true;
			}
		}
		return false;
	}
	// 刷新指定文件的所属AssetBundle名字
	protected static string refreshFileAssetBundleName(string file, bool forceSingle = false, bool forceRefreshAssetBundleName = false)
	{
		// .meta不能设置AssetBundle,.DS_Store是mac的特殊文件,也不能设置AssetBundle
		if (endWith(file, ".meta") || endWith(file, ".DS_Store"))
		{
			return EMPTY;
		}
		AssetImporter importer = AssetImporter.GetAtPath(file);
		if (importer == null)
		{
			Debug.LogError("Set AssetName Fail, File:" + file);
			return EMPTY;
		}
		// tpsheet文件不打包
		// LightingData.asset文件不能打包AB,这是一个特殊文件,只用于编辑器
		if (endWith(file, ".tpsheet") || endWith(file, "LightingData.asset"))
		{
			importer.assetBundleName = EMPTY;
			return EMPTY;
		}
		string fileName = rightToLeft(file.ToLower());
		string bundleName = getFileAssetBundleName(fileName, forceSingle);
		if (importer.assetBundleName != bundleName || forceRefreshAssetBundleName)
		{
			importer.assetBundleName = bundleName;
		}
		// 存储bundleInfo
		// 去除Asset/GameResources/前缀,只保留GameResources下相对路径
		string assetName = removeStartString(fileName, P_GAME_RESOURCES_PATH, false);
		if (!mAssetBundleMap.TryGetValue(bundleName, out BuildAssetBundleInfo bundleInfo))
		{
			bundleInfo = new BuildAssetBundleInfo(bundleName);
			mAssetBundleMap.Add(bundleName, bundleInfo);
		}
		bundleInfo.mAssetNames.Add(assetName);
		return bundleName;
	}
	// fullPath是以Asset开头的路径
	protected static bool generateAssetBundleName(string fullPath)
	{
		if (isUnpackPath(fullPath, mUnPackFolder))
		{
			return true;
		}
		string[] files = Directory.GetFiles(fullPath);
		if (files == null || files.Length == 0)
		{
			return true;
		}
		bool isForceSingle = isForceSinglePath(fullPath, mForceSingleFolder);
		foreach (string file in files)
		{
			if (refreshFileAssetBundleName(rightToLeft(file), isForceSingle) == null)
			{
				return false;
			}
		}
		EditorUtility.UnloadUnusedAssetsImmediate();
		AssetDatabase.Refresh();
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
	protected static void createOrClearOutPath(string outputPath)
	{
		if (!Directory.Exists(outputPath))
		{
			Directory.CreateDirectory(outputPath);
			return;
		}

		// 查找目录下的所有第一级子目录
		string[] dirList = Directory.GetDirectories(outputPath);
		int dirCount = dirList.Length;
		for (int i = 0; i < dirCount; ++i)
		{
			// 只删除不需要保留的目录
			if (!isKeepFolderOrMeta(getFileName(dirList[i])))
			{
				Directory.Delete(dirList[i], true);
			}
		}
		// 查找目录下的所有第一级文件
		string[] files = Directory.GetFiles(outputPath);
		int fileCount = files.Length;
		for (int i = 0; i < fileCount; ++i)
		{
			if (!isKeepFolderOrMeta(getFileName(files[i])))
			{
				File.Delete(files[i]);
			}
		}
	}
	protected static bool isKeepFolderOrMeta(string name)
	{
		int count = mKeepFolder.Length;
		for (int i = 0; i < count; ++i)
		{
			if (mKeepFolder[i] == name || mKeepFolder[i] + ".meta" == name)
			{
				return true;
			}
		}
		return false;
	}
	// 清理之前设置的bundleName
	protected static void clearAssetBundleName()
	{
		List<string> allFiles = new List<string>();
		findFiles(F_GAME_RESOURCES_PATH, allFiles);
		int count = allFiles.Count;
		for(int i = 0; i < count; ++i)
		{
			string fileName = fullPathToProjectPath(allFiles[i]);
			if(endWith(fileName, ".meta"))
			{
				continue;
			}
			AssetImporter importer = AssetImporter.GetAtPath(fileName);
			if (importer == null)
			{
				continue;
			}
			importer.assetBundleName = EMPTY;
		}
		AssetDatabase.RemoveUnusedAssetBundleNames();
		AssetDatabase.Refresh();
	}
	protected static void clearUnPackAssetBundleName(List<string> fileList, string[] unpackList)
	{
		foreach (var file in fileList)
		{
			if (endWith(file, ".meta"))
			{
				continue;
			}
			string projectFileName = fullPathToProjectPath(file);
			string fileName = removeStartString(projectFileName, P_GAME_RESOURCES_PATH, false);
			foreach (var unpack in unpackList)
			{
				if (!startWith(fileName, unpack, false))
				{
					continue;
				}
				AssetImporter importer = AssetImporter.GetAtPath(projectFileName);
				if (importer != null && !isEmpty(importer.assetBundleName))
				{
					importer.assetBundleName = EMPTY;
				}
				break;
			}
		}
		AssetDatabase.Refresh();
	}
}