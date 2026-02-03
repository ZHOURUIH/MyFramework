using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;
using static FrameBaseUtility;
using static StringUtility;
using static FileUtility;
using static FrameDefine;
using static EditorDefine;
using static EditorFileUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;

public class MenuAssetBundle
{
	public static bool mIsPackingAssetBundle;
	[MenuItem("AssetBundle/清除AssetBundle名称")]
	public static void clearAllAssetBundleName()
	{
		clearAssetBundleName();
	}
	[MenuItem("AssetBundle/Pack AssetBundle")]
	public static void packAssetBundleMenu()
	{
		packAssetBundle(getBuildTarget(), getAssetBundlePath(true), true);
	}
	// assetBundleName是StreamingAsset下的相对路径,带后缀
	[MenuItem("AssetBundle/Find AssetBundle Dependency")]
	public static void findDependencyMenu()
	{
		string selection = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (!selection.EndsWith(".unity3d"))
		{
			showInfo("需要选中一个.unity3d", true, true);
			return;
		}
		findAllDependencies(selection.removeStartString(P_ASSET_BUNDLE_ANDROID_PATH));
	}
	protected static bool preProcess()
	{
		// 收集所有图集,然后将信息写入到图集路径的配置文件中,这个文件会在AtlasManager中用到
		List<string> pathList = new();
		HashSet<string> fileNameList = new();
		foreach (string file in findFilesNonAlloc(F_GAME_RESOURCES_PATH, SPRITE_ATLAS_SUFFIX))
		{
			pathList.Add(file.removeStartString(F_GAME_RESOURCES_PATH));
			if (!fileNameList.Add(getFileNameNoSuffixNoDir(file)))
			{
				Debug.LogError("存在重名的图集文件:" + getFileNameNoSuffixNoDir(file));
				return false;
			}
		}
		writeTxtFile(F_GAME_RESOURCES_PATH + R_MISC_PATH + ATLAS_PATH_CONFIG, stringsToString(pathList, "\r\n"));

		// 设置所有图集不打入包体,虽然不太好理解这个,不过设置为false以后AssetBundle中就不会出现冗余的图片,否则AssetBundle将会变得异常大
		foreach (string file in findFilesNonAlloc(F_GAME_RESOURCES_PATH, SPRITE_ATLAS_SUFFIX))
		{
			var atlas = loadAsset<SpriteAtlas>(file);
			if (atlas == null)
			{
				Debug.LogError("图集加载失败:" + file);
			}
			atlas.SetIncludeInBuild(false);
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		return true;
	}
	protected static void postProcess()
	{
		foreach (string file in findFilesNonAlloc(F_GAME_RESOURCES_PATH, SPRITE_ATLAS_SUFFIX))
		{
			var atlas = loadAsset<SpriteAtlas>(file);
			if (atlas == null)
			{
				Debug.LogError("图集加载失败:" + file);
			}
			atlas.SetIncludeInBuild(true);
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
	public static bool packAssetBundle(BuildTarget target, string outputPath, bool showMessageBox)
	{
		Debug.Log("打包全部AssetBundle");
		mIsPackingAssetBundle = true;
		bool result = false;
		do
		{
			if (!preProcess())
			{
				break;
			}
			DateTime time0 = DateTime.Now;
			// 清理输出目录
			createOrClearOutPath(outputPath);
			// 清理不打包的AssetBundle名
			clearUnPackAssetBundleName(findFilesNonAlloc(F_GAME_RESOURCES_PATH), getUnpackFolder());
			// 设置bunderName
			// key为AssetBundle名,带Resources下相对路径,带后缀,Value是该AssetBundle中包含的所有Asset
			Dictionary<string, BuildAssetBundleInfo> assetBundleMap = new();
			foreach (string dir in getAllSubResDirs(P_GAME_RESOURCES_PATH))
			{
				if (!generateAssetBundleName(dir, assetBundleMap, showMessageBox))
				{
					break;
				}
			}
			EditorUtility.UnloadUnusedAssetsImmediate();
			AssetDatabase.Refresh();
			// 打包
			// 使用LZ4压缩,并且不写入资源类型信息
			var option = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode;
#if WEIXINMINIGAME
			// 微信的AssetBundle需要添加hash
			option |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
#endif
			BuildPipeline.BuildAssetBundles(outputPath, option, target);
			AssetDatabase.Refresh();
			// 检查资源是否有互相依赖的问题,然后构建依赖关系
			if (!loadAndReadAssetBundleManifest(outputPath, assetBundleMap, null, showMessageBox, true))
			{
				break;
			}

			// 生成配置文件
			SerializerWrite serializer = new();
			serializer.write(assetBundleMap.Count);
			foreach (BuildAssetBundleInfo bundleInfo in assetBundleMap.Values)
			{
				// AssetBundle名字
				serializer.writeString(bundleInfo.mBundleName);
				// AssetBundle所包含的所有Asset名字
				serializer.writeList(bundleInfo.mAssetNames);
				// AssetBundle依赖的所有AssetBundle
				serializer.writeList(bundleInfo.mDependencies);
			}
			writeFile(outputPath + STREAMING_ASSET_FILE, serializer.getBuffer(), serializer.getDataSize(), false);
			// 删除所有的manifest文件
			foreach (string file in findFilesNonAlloc(outputPath, new List<string>() { ".manifest", ".manifest.meta" }))
			{
				deleteFile(file);
			}
			postProcess();
			showInfo("资源打包结束! 耗时 : " + (DateTime.Now - time0), showMessageBox, false);
			result = true;
		} while (false);
		mIsPackingAssetBundle = false;
		return result;
	}
	// pathToPack为以Asset开头的相对路径,表示只单独打包此目录或此文件
	public static bool packAssetBundle(BuildTarget target, string outputPath, string pathToPack, bool showMessageBox)
	{
		if (pathToPack.isEmpty())
		{
			Debug.Log("没有找到可打包AssetBundle的文件");
			return false;
		}
		Debug.Log("单独打包:" + pathToPack);
		AssetBundleBuild[] buildList = null;
		findAssetBundleBuild(pathToPack, ref buildList);
		DateTime time0 = DateTime.Now;
		if (buildList != null)
		{
			// 备份清单文件和生成的AssetBundle信息文件
			string folderName = getFolderName(outputPath);
			byte[] streamingAssetsBytes = openFile(outputPath + STREAMING_ASSET_FILE, false);
			byte[] streamingFileBytes = openFile(outputPath + folderName, false);
			byte[] manifestBytes = openFile(outputPath + folderName + ".manifest", false);
			// 需要先删除AssetBundle和对应的manifest文件,否则无法生成新的AssetBundle
			foreach (AssetBundleBuild item in buildList)
			{
				string bundleFileName = outputPath + item.assetBundleName;
				string manifestName = bundleFileName + ".manifest";
				deleteFile(bundleFileName);
				deleteFile(bundleFileName + ".meta");
				deleteFile(manifestName);
				deleteFile(manifestName + ".meta");
			}
			// 使用LZ4压缩,并且不写入资源类型信息
			var option = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode;
#if WEIXINMINIGAME
			// 微信的AssetBundle需要添加hash
			option |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
#endif
			BuildPipeline.BuildAssetBundles(outputPath, buildList, option, target);
			AssetDatabase.Refresh();

			// 还原备份的文件
			if (streamingAssetsBytes != null)
			{
				writeFile(outputPath + STREAMING_ASSET_FILE, streamingAssetsBytes);
			}
			if (streamingFileBytes != null)
			{
				writeFile(outputPath + folderName, streamingFileBytes);
			}
			if (manifestBytes != null)
			{
				writeFile(outputPath + folderName + ".manifest", manifestBytes);
			}
		}
		// 删除所有的manifest文件
		foreach (string file in findFilesNonAlloc(outputPath, new List<string>() { ".manifest", ".manifest.meta" }))
		{
			deleteFile(file);
		}
		showInfo("资源打包结束! 耗时 : " + (DateTime.Now - time0), showMessageBox, false);
		return true;
	}
	protected static void findAllDependencies(string assetBundleName)
	{
		Debug.Log("开始查找" + assetBundleName + "的依赖项");
		Dictionary<string, HashSet<string>> dependencyList = new();
		if (!loadAndReadAssetBundleManifest(getAssetBundlePath(true), null, dependencyList, true, false))
		{
			showInfo("加载资源清单文件失败", true, true);
			return;
		}
		// List<string>表示这个依赖项的依赖链
		Dictionary<string, List<string>> allDepList = new();
		findAllDependenciesRecursive(dependencyList, assetBundleName, allDepList);
		Debug.Log("开始查找" + assetBundleName + "的所有递归依赖项,共" + IToS(allDepList.Count) + "个");
		foreach (var item in allDepList)
		{
			string chain = EMPTY;
			foreach (string item0 in item.Value)
			{
				chain += item0 + "->";
			}
			Debug.Log(colorString("00FF00FF", item.Key) + ",依赖链:" + chain.removeEndString("->"));
		}
		Debug.Log("查找" + assetBundleName + "的所有递归依赖项完成");
	}
	protected static void findAllDependenciesRecursive(Dictionary<string, HashSet<string>> dependencyList, string assetBundleName, Dictionary<string, List<string>> allDepList)
	{
		if (!dependencyList.TryGetValue(assetBundleName, out var depList))
		{
			return;
		}
		List<string> curChain = allDepList.get(assetBundleName);
		foreach (string dep in depList)
		{
			List<string> chain = new();
			chain.addRange(curChain);
			chain.Add(assetBundleName);
			allDepList.TryAdd(dep, chain);
			findAllDependenciesRecursive(dependencyList, dep, allDepList);
		}
	}
	// 所有所有AssetBundle的直接依赖项
	protected static bool loadAndReadAssetBundleManifest(string manifesePath, Dictionary<string, BuildAssetBundleInfo> assetBundleMap, Dictionary<string, HashSet<string>> dependencyList, bool showErrorMessageBox, bool checkDep)
	{
		AssetBundle assetBundle = AssetBundle.LoadFromFile(manifesePath + getFolderName(manifesePath));
		if (assetBundle == null)
		{
			showInfo("加载AssetBundleManifest失败", showErrorMessageBox, true);
			return false;
		}
		bool result = true;
		if (checkDep)
		{
			result = checkDependency(assetBundle, showErrorMessageBox);
		}
		Dictionary<string, HashSet<string>> tempDepList = dependencyList;
		tempDepList ??= new();
		result = result && readAssetBundleManifest(assetBundle, assetBundleMap, tempDepList, showErrorMessageBox);
		assetBundle.Unload(true);
		return result;
	}
	protected static bool checkDependency(AssetBundle assetBundle, bool showErrorMessageBox)
	{
		Dictionary<string, HashSet<string>> tempDepList = new();
		var manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		// 先检查一次是否有互相依赖的资源
		foreach (string bundle in manifest.GetAllAssetBundles())
		{
			string bundleName = bundle.rightToLeft();
			// 遍历当前AB的所有依赖项
			foreach (string dep in manifest.GetAllDependencies(bundleName))
			{
				// 查找依赖项中是否有依赖当前AssetBundle的
				if (tempDepList.TryGetValue(dep, out var depList) && depList.Contains(bundleName))
				{
					showInfo("AssetBundle dependency error! " + dep + ", " + bundleName, showErrorMessageBox, true);
					return false;
				}
			}
		}
		return true;
	}
	protected static bool readAssetBundleManifest(AssetBundle assetBundle, Dictionary<string, BuildAssetBundleInfo> assetBundleMap, Dictionary<string, HashSet<string>> tempDepList, bool showErrorMessageBox)
	{
		var manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
		// 遍历所有AB
		foreach (string bundle in manifest.GetAllAssetBundles())
		{
			string bundleName = bundle.rightToLeft();
			BuildAssetBundleInfo bundleInfo = null;
			if (assetBundleMap != null && !assetBundleMap.TryGetValue(bundleName, out bundleInfo))
			{
				continue;
			}
			HashSet<string> dependencySet = new();
			// 遍历当前AB的所有依赖项
			foreach (string dep in manifest.GetDirectDependencies(bundleName))
			{
				string depName = dep.rightToLeft();
				bundleInfo?.AddDependence(depName);
				dependencySet.Add(depName);
				// 查找依赖项中是否有依赖当前AssetBundle的
				if (tempDepList.TryGetValue(depName, out var depList) && depList.Contains(bundleName))
				{
					showInfo("AssetBundle dependency error! " + depName + ", " + bundleName, showErrorMessageBox, true);
					return false;
				}
			}
			if (!tempDepList.TryAdd(bundleName, dependencySet))
			{
				showInfo("已经存在一个名为:" + bundleName + "的AssetBundle", showErrorMessageBox, true);
				return false;
			}
		}
		return true;
	}
	// 显示一个提示信息
	protected static void showInfo(string str, bool showBox, bool isError)
	{
		if (isError)
		{
			Debug.LogError(str);
		}
		else
		{
			Debug.Log(str);
		}
		if (showBox)
		{
			displayDialog(isError ? "错误" : "提示", str, "确认");
		}
	}
	// 查找一个目录中所有需要打包的资源包
	protected static void findAssetBundleBuild(string path, ref AssetBundleBuild[] list)
	{
		Dictionary<string, List<string>> assetBundleList = new();
		// path是文件
		if (!getFileSuffix(path).isEmpty())
		{
			string bundleName = refreshFileAssetBundleName(null, path);
			if (!bundleName.isEmpty())
			{
				assetBundleList.getOrAddNew(bundleName).Add(path);
			}
		}
		// path是目录
		else
		{
			foreach (string dir in getAllSubResDirs(path))
			{
				foreach (string file in Directory.GetFiles(dir))
				{
					string bundleName = refreshFileAssetBundleName(null, file);
					if (!bundleName.isEmpty())
					{
						assetBundleList.getOrAddNew(bundleName).Add(file);
					}
				}
			}
		}
		list = new AssetBundleBuild[assetBundleList.Count];
		int index = 0;
		foreach (var item in assetBundleList)
		{
			AssetBundleBuild build = new();
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
		if (file.endWith(".unity") || forceSingle)
		{
			bundleName = file.removeStartString(P_GAME_RESOURCES_PATH, false);
			bundleName = replaceSuffix(bundleName, ASSET_BUNDLE_SUFFIX);
		}
		// 其他文件的AssetBundle就是所属文件夹
		else
		{
			bundleName = getFilePath(file).removeStartString(P_GAME_RESOURCES_PATH, false);
			bundleName += ASSET_BUNDLE_SUFFIX;
		}
		return bundleName;
	}
	// 判断一个路径是否是不需要打包的路径
	protected static bool isUnpackPath(string path, List<string> unpackList)
	{
		string pathUnderResources = (path.removeStartString(P_GAME_RESOURCES_PATH, false) + "/").rightToLeft();
		foreach (string name in unpackList)
		{
			// 如果该文件夹是不打包的文件夹,则直接返回
			if (pathUnderResources.startWith(name, false))
			{
				return true;
			}
		}
		return false;
	}
	// 判断一个路径是否是不需要打包的路径
	protected static bool isForceSinglePath(string path, List<string> singlePathList)
	{
		string pathUnderResources = (path.removeStartString(P_GAME_RESOURCES_PATH, false) + "/").rightToLeft();
		foreach (string name in singlePathList)
		{
			// 如果该文件夹是不打包的文件夹,则直接返回
			if (pathUnderResources == name)
			{
				return true;
			}
		}
		return false;
	}
	// 刷新指定文件的所属AssetBundle名字
	protected static string refreshFileAssetBundleName(Dictionary<string, BuildAssetBundleInfo> assetBundleMap, string file, bool forceSingle = false)
	{
		AssetImporter importer = AssetImporter.GetAtPath(file);
		if (importer == null)
		{
			return EMPTY;
		}
		// .meta不能设置AssetBundle
		// .DS_Store是mac的特殊文件,也不能设置AssetBundle
		// .cginc是仅编辑器下使用的资源,不能打包AssetBundle
		// tpsheet文件不打包
		// LightingData.asset文件不能打包AB,这是一个特殊文件,只用于编辑器
		if (file.endWith(".meta") || 
			file.endWith(".DS_Store") || 
			file.endWith(".cginc") ||
			file.endWith(".hlsl") ||
			file.endWith(".glslinc") ||
			file.endWith(".tpsheet") ||
			file.endWith("LightingData.asset"))
		{
			importer.assetBundleName = EMPTY;
			return EMPTY;
		}
		
		string fileName = file.ToLower().rightToLeft();
		string bundleName = getFileAssetBundleName(fileName, forceSingle);
		if (importer.assetBundleName != bundleName)
		{
			importer.assetBundleName = bundleName;
		}
		if (assetBundleMap != null)
		{
			// 存储bundleInfo
			// 去除Asset/GameResources/前缀,只保留GameResources下相对路径
			string assetName = fileName.removeStartString(P_GAME_RESOURCES_PATH, false);
			if (!assetBundleMap.TryGetValue(bundleName, out BuildAssetBundleInfo bundleInfo))
			{
				bundleInfo = new(bundleName);
				assetBundleMap.Add(bundleName, bundleInfo);
			}
			bundleInfo.mAssetNames.Add(assetName);
		}
		return bundleName;
	}
	// fullPath是以Asset开头的路径
	protected static bool generateAssetBundleName(string fullPath, Dictionary<string, BuildAssetBundleInfo> assetBundleMap, bool showErrorMessageBox)
	{
		if (isUnpackPath(fullPath, getUnpackFolder()))
		{
			return true;
		}
		string[] files = Directory.GetFiles(fullPath);
		if (files.isEmpty())
		{
			return true;
		}
		bool isForceSingle = isForceSinglePath(fullPath, getForceSingleFolder());
		foreach (string file in files)
		{
			if (!file.endWith(".meta") && refreshFileAssetBundleName(assetBundleMap, file.rightToLeft(), isForceSingle) == null)
			{
				showInfo("生成AssetBundle名字失败:" + file, showErrorMessageBox, true);
				return false;
			}
		}
		return true;
	}
	// 递归获取所有子目录文件夹
	protected static List<string> getAllSubResDirs(string fullPath)
	{
		List<string> dirList = new();
		getAllSubResDirsInternal(fullPath, dirList);
		return dirList;
	}
	// 递归获取所有子目录文件夹
	protected static void getAllSubResDirsInternal(string fullPath, List<string> dirList)
	{
		if (dirList == null || fullPath.isEmpty())
		{
			return;
		}
		foreach (string dir in Directory.GetDirectories(fullPath).safe())
		{
			getAllSubResDirsInternal(dir.rightToLeft(), dirList);
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
		foreach (string dir in Directory.GetDirectories(outputPath))
		{
			// 只删除不需要保留的目录
			if (!isKeepFolderOrMeta(getFileNameWithSuffix(dir)))
			{
				Directory.Delete(dir, true);
			}
		}
		// 查找目录下的所有第一级文件
		foreach (string file in Directory.GetFiles(outputPath))
		{
			if (!isKeepFolderOrMeta(getFileNameWithSuffix(file)))
			{
				File.Delete(file);
			}
		}
	}
	protected static bool isKeepFolderOrMeta(string name)
	{
		foreach (string folder in getKeepFolder())
		{
			if (folder == name || folder + ".meta" == name)
			{
				return true;
			}
		}
		return false;
	}
	// 清理之前设置的bundleName
	protected static void clearAssetBundleName()
	{
		foreach (string file in findFilesNonAlloc(F_GAME_RESOURCES_PATH))
		{
			string fileName = fullPathToProjectPath(file);
			if(fileName.endWith(".meta"))
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
	// 清除所有不需要打包AB的资源meta中记录的AB名
	protected static void clearUnPackAssetBundleName(List<string> fileList, List<string> unpackList)
	{
		foreach (string file in fileList)
		{
			if (file.endWith(".meta"))
			{
				continue;
			}
			string projectFileName = fullPathToProjectPath(file);
			string fileName = projectFileName.removeStartString(P_GAME_RESOURCES_PATH, false);
			foreach (string unpack in unpackList)
			{
				if (!fileName.startWith(unpack, false))
				{
					continue;
				}
				AssetImporter importer = AssetImporter.GetAtPath(projectFileName);
				if (importer != null && !importer.assetBundleName.isEmpty())
				{
					importer.assetBundleName = EMPTY;
				}
				break;
			}
		}
		AssetDatabase.Refresh();
	}
}