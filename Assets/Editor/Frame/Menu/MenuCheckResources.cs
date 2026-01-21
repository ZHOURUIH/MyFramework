using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using static FileUtility;
using static StringUtility;
using static MathUtility;
using static EditorCommonUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static EditorFileUtility;

public class MenuCheckResources
{
	[MenuItem("检查资源/查找文件引用  %Q", false, 2)]
	public static void searchRefrence()
	{
		// 查找该文件的所有引用
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("查找所有资源引用", "未选中任何目录,是否想要查找GameResources中所有资源的引用?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		// 选择的是文件,则只查找文件的引用
		if (isFileExist(path))
		{
			doSearchRefrence(path, getAllResourceFileText());
		}
		// 选择的是目录,则查找目录中所有文件的引用
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("查找所有资源引用", "确认查找文件夹中所有文件的引用? " + path, "确认", "取消"))
			{
				Debug.Log("开始查找资源引用:" + path + "...");
				var allFileText = getAllResourceFileText();
				// 不查找meta文件的引用
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, !item.EndsWith(".meta"));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("查找所有资源引用", "进度: ", i + 1, count);
					doSearchRefrence(validFiles[i], allFileText);
				}
				clearProgressBar();
				Debug.Log("完成查找资源引用");
			}
		}
	}
	[MenuItem("检查资源/查找资源引用了哪些文件", false, 2)]
	public static void searchResourceRefOther()
	{
		// 查找该文件的所有引用
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("查找资源引用了哪些文件", "未选中任何目录,是否想要查找GameResources中所有资源引用的文件?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		// 选择的是文件,则只查找文件的引用
		if (isFileExist(path))
		{
			doSearchResourceRefOther(path, getAllResourceMeta());
		}
		// 选择的是目录,则查找目录中所有文件的引用
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("查找资源引用了哪些文件", "确认查找文件夹中所有资源引用的文件? " + path, "确认", "取消"))
			{
				Debug.Log("开始查找资源引用:" + path + "...");
				var allFileText = getAllResourceMeta();
				// 不查找meta文件的引用
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, !item.EndsWith(".meta"));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("查找资源引用了哪些文件", "进度: ", i + 1, count);
					doSearchResourceRefOther(validFiles[i], allFileText);
				}
				clearProgressBar();
				Debug.Log("完成查找资源引用了哪些文件");
			}
		}
	}
	[MenuItem("检查资源/查找TPAtlas图集引用", false, 4)]
	public static void checkTPAtlasRefrence()
	{
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("检查图集引用", "未选中任何目录,是否想要检查GameResources中所有图集的引用?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查图集引用:" + path + "...");
		if (isFileExist(path))
		{
			if (path.endWith("png", false))
			{
				doCheckTPAtlasRefrence(path, getAllFileText(F_UI_PREFAB_PATH));
			}
		}
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("检查图集引用", "确认查找文件夹中图集引用? " + path, "确认", "取消"))
			{
				// 只查找png
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, item.endWith("png", false));
				}
				var allFileText = getAllFileText(F_UI_PREFAB_PATH);
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					string filePath = validFiles[i];
					displayProgressBar("检查图集引用", "进度: ", i + 1, count);
					doCheckTPAtlasRefrence(filePath, allFileText);
				}
				clearProgressBar();
			}
		}
		Debug.Log("完成检查图集引用");
	}
	[MenuItem("检查资源/检查所有界面是否添加了适配组件", false, 5)]
	public static void checkUIHasScaleAnchor()
	{
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("检查是否添加了适配组件", "未选中任何目录,是否想要检查GameResources中所有的界面?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_UI_PREFAB_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查是否添加了适配组件:" + path + "...");
		if (isFileExist(path))
		{
			doCheckUIHasScaleAnchor(path);
		}
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("检查是否添加了适配组件", "确认查找文件夹中所有的界面? " + path, "确认", "取消"))
			{
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories))
				{
					string file = item.rightToLeft();
					validFiles.addIf(file, !file.endWith(".meta", false));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					string filePath = validFiles[i];
					displayProgressBar("检查是否添加了适配组件", "进度: ", i + 1, count);
					doCheckUIHasScaleAnchor(filePath);
				}
				clearProgressBar();
			}
		}
		Debug.Log("完成检查适配组件");
	}
	[MenuItem("检查资源/检查未引用的资源", false, 99)]
	public static void checkUnusedResources()
	{
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("查找未引用的资源", "未选中任何目录,是否想要查找GameResources中所有的未引用的资源?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始查找未引用的资源:" + path + "...");
		if (isFileExist(path))
		{
			doCheckUnusedFile(path, getAllResourceFileText());
		}
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("查找未引用的资源", "确认查找文件夹中所有未使用资源? " + path, "确认", "取消"))
			{
				var allFileText = getAllResourceFileText();
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, !item.endWith(".meta", false));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					string filePath = validFiles[i];
					displayProgressBar("查找未引用的资源", "进度: ", i + 1, count);
					doCheckUnusedFile(filePath, allFileText);
				}
				clearProgressBar();
			}
		}
		Debug.Log("完成查找未引用的资源");
	}
	[MenuItem("检查资源/检查选中对象的原点是否在模型底部", false, 100)]
	public static void checkZeroPoint()
	{
		GameObject[] objects = Selection.gameObjects;
		if (objects == null)
		{
			EditorUtility.DisplayDialog("错误", "请先Project中选中需要设置的文件", "确定");
			return;
		}

		// 对选中的所有对象遍历生成角色控制器
		foreach (GameObject item in objects)
		{
			var renderers = item.GetComponentsInChildren<Renderer>(true);
			if (renderers == null)
			{
				continue;
			}
			Vector3 center = Vector3.zero;
			foreach (Renderer render in renderers)
			{
				center += render.bounds.center;
			}
			if (renderers.Length > 0)
			{
				center /= renderers.Length;
			}
			// 计算包围盒
			Bounds bounds = new(center, Vector3.zero);
			foreach (Renderer render in renderers)
			{
				bounds.min = getMinVector3(bounds.min, render.bounds.min);
				bounds.max = getMaxVector3(bounds.max, render.bounds.max);
			}
			if (bounds.center.y < bounds.size.y * 0.5f - 0.01f)
			{
				Debug.LogError("原点不在模型底部: " + item.name + ", 误差值: " + (bounds.size.y * 0.5f - bounds.center.y), item);
			}
		}
	}
	[MenuItem("检查资源/检查材质引用丢失", false, 101)]
	public static void checkMaterialMissingRefrence()
	{
		Debug.Log("开始检查是否有材质引用丢失");
		// 所有Material的GUID集合
		List<string> materialGUIDsList = new(getAllGUIDBySuffixInFilePath(F_ASSETS_PATH, ".mat.meta", "材质").Keys);
		// 所有引用Material的.prefab与.unity文件的集合
		// 丢失脚本引用的资源字典(key = "引用了丢失材质的资源路径",value = 该资源丢失的材质的guid列表)
		Dictionary<string, List<string>> missingRefAssetsList = new();
		foreach (var item in getMaterialRefrenceFileText(F_ASSETS_PATH))
		{
			FileGUIDLines fileInfo = item.Value;
			foreach (string guidsStr in fileInfo.mContainGUIDLines)
			{
				foreach (string guid in split(guidsStr, '-'))
				{
					// 与存着所有的材质球GUID的列表进行比对
					if (!materialGUIDsList.Contains(guid))
					{
						missingRefAssetsList.getOrAddNew(fileInfo.mProjectFileName).Add(guid);
					}
				}
			}
		}

		debugMissingRefInformation(missingRefAssetsList, "材质");
		Debug.Log("完成检查是否有材质引用丢失");
	}
	[MenuItem("检查资源/检查材质贴图是否存在", false, 102)]
	public static void checkMaterialTextureValid()
	{
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("材质贴图是否存在", "未选中任何目录,是否想要查找GameResources中所有材质的贴图是否存在?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查材质贴图是否存在:" + path);
		// 选择的是文件,则只查找文件的引用
		if (isFileExist(path))
		{
			EditorCommonUtility.checkMaterialTextureValid(path, getAllResourceMeta());
		}
		// 选择的是目录,则查找目录中所有文件的引用
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("材质贴图是否存在", "确认查找文件夹中所有材质的贴图是否存在? " + path, "确认", "取消"))
			{
				var allFileMeta = getAllResourceMeta();
				// 因为后缀长度小于等于3时会查找出所有包含此后缀的文件,并不一定只有指定后缀的文件
				// 所以此处需要过滤掉不需要的文件
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.mat", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, item.EndsWith(".mat"));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("材质贴图是否存在", "进度: ", i + 1, count);
					EditorCommonUtility.checkMaterialTextureValid(validFiles[i], allFileMeta);
				}
				clearProgressBar();
			}
		}
		Debug.Log("完成检查材质贴图是否存在");
	}
	[MenuItem("检查资源/检查材质是否引用了shader未使用的贴图", false, 103)]
	public static void checkMaterialTextureRefrence()
	{
		// 查找该文件的所有引用
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("材质是否引用了shader未使用的贴图", "未选中任何目录,是否想要检查GameResources中所有材质的shader的贴图?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查材质是否引用了shader未使用的贴图:" + path);
		// 选择的是文件,则只查找文件的引用
		if (isFileExist(path))
		{
			checkMaterialTexturePropertyValid(path, getAllResourceMeta());
		}
		// 选择的是目录,则查找目录中所有文件的引用
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("材质是否引用了shader未使用的贴图", "确认查找文件夹中所有材质的贴图属性? " + path, "确认", "取消"))
			{
				var allFileMeta = getAllResourceMeta();
				// 因为后缀长度小于等于3时会查找出所有包含此后缀的文件,并不一定只有指定后缀的文件
				// 所以此处需要过滤掉不需要的文件
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, item.EndsWith(".mat"));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("材质是否引用了shader未使用的贴图", "进度: ", i + 1, count);
					checkMaterialTexturePropertyValid(validFiles[i], allFileMeta);
				}
				clearProgressBar();
			}
		}
		Debug.Log("完成检查材质引用未使用的贴图");
	}
	// 检查热更与非热更资源是否存在相互引用(如有资源互相引用则为不合法)
	[MenuItem("检查资源/检查热更与非热更资源是否相互引用", false, 105)]
	public static void checkRsourcesRefEachOther()
	{
		Debug.Log("开始检查热更与非热更资源相互引用");
		// 所有热更资源的GUID
		var allGameResourcesAssetGUID = getAllGUIDAndSpriteIDBySuffixInFilePath(P_GAME_RESOURCES_PATH, ".meta", "所有热更资源");
		// 所有非热更资源的GUID
		var allResourcesAssetGUID = getAllGUIDAndSpriteIDBySuffixInFilePath(P_RESOURCES_PATH, ".meta", "所有非热更资源");

		// 所有热更资源中所有带引用的文件的集合
		var refGameResourcesFilesDic = getAllRefrenceFileText(P_GAME_RESOURCES_PATH);
		// 所有非热更资源中所有带引用的文件的集合
		var refResourcesFilesDic = getAllRefrenceFileText(P_RESOURCES_PATH);
		// 错误引用资源的字典
		Dictionary<string, Dictionary<string, string>> errorRefAssetDic = new();

		// 检测不合法引用
		doCheckRefGUIDInFilePath(errorRefAssetDic, refGameResourcesFilesDic, allResourcesAssetGUID);
		doCheckRefGUIDInFilePath(errorRefAssetDic, refResourcesFilesDic, allGameResourcesAssetGUID);

		// 输出定位丢失脚本引用的资源信息
		foreach (var item in errorRefAssetDic)
		{
			// 将丢失引用的资源中的丢失引用对象的列表存入列表中
			List<string> missingRefObjectsList = new();
			setCheckRefObjectsList(missingRefObjectsList, item.Key, item.Value.Keys);
			UObject go = loadAsset(item.Key);
			if (go != null)
			{
				Debug.LogError("引用了错误的资源,热更资源与非热更资源不能互相引用:" + go.name + "\n节点名称:" + stringsToString(missingRefObjectsList, '\n'), go);
			}
			else
			{
				Debug.LogError("引用了不存在的资源:" + item.Key + "\n节点名称:" + stringsToString(missingRefObjectsList, '\n'));
			}
		}
		Debug.Log("结束检查热更与非热更资源相互引用");
	}
	[MenuItem("检查资源/检查图集中不存在的图片", false, 108)]
	public static void checkAtlasNotExistSprite()
	{
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("检查图集中不存在的图片", "未选中任何目录,是否想要检查GameResources中所有图集中不存在的图片?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查图集中不存在的图片:" + path + "...");
		if (isFileExist(path))
		{
			if (path.endWith("png", false))
			{
				doCheckAtlasNotExistSprite(path);
			}
		}
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("检查图集中不存在的图片", "确认查找文件夹中图集中不存在的图片? " + path, "确认", "取消"))
			{
				// 只查找png
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, item.endWith("png", false));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					string filePath = validFiles[i];
					displayProgressBar("检查图集中不存在的图片", "进度: ", i + 1, count);
					doCheckAtlasNotExistSprite(filePath);
				}
				clearProgressBar();
			}
		}
		Debug.Log("完成检查图集中不存在的图片");
	}
	[MenuItem("检查资源/检查是否有重复文件", false, 109)]
	public static void checkSameAsset()
	{
		Debug.Log("开始检查是否有重复文件");
		// 路径下所有资源文件列表
		List<string> tempFileList = findFilesNonAlloc(P_GAME_RESOURCES_PATH);
		
		// 需要排除Unused目录的文件和meta文件
		List<string> resourcesFilesList = new();
		foreach (string file in tempFileList)
		{
			if (!file.Contains("/Unused/") && !file.endWith(".meta"))
			{
				resourcesFilesList.Add(file);
			}
		}

		// 重复的资源的路径字典(key: MD5字符串, value: 相同资源的列表)
		Dictionary<string, List<string>> hasSameAssetsDic = new();
		int fileCount = resourcesFilesList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("正在查找是否有重复资源", "进度: ", i + 1, fileCount);
			string MD5Str = EditorFileUtility.generateFileMD5(openFile(resourcesFilesList[i], true));
			hasSameAssetsDic.getOrAddNew(MD5Str).Add(resourcesFilesList[i]);
		}

		// 输出结果
		foreach (var element in hasSameAssetsDic.Values)
		{
			if (element.Count > 1)
			{
				Debug.LogError("出现重复的资源,路径为:\n" + stringsToString(element, '\n'), loadAsset(element[0]));
			}
		}
		clearProgressBar();
		Debug.Log("结束检查是否有相同文件");
	}
	[MenuItem("检查资源/检查预设根节点是否带变换", false, 111)]
	public static void checkPrefabRootTransform()
	{
		Debug.Log("开始检查预设变换...");
		List<string> fileList = findFilesNonAlloc(F_GAME_RESOURCES_PATH, ".prefab");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查预设变换", "进度: ", i + 1, fileCount);
			GameObject prefab = loadGameObject(fullPathToProjectPath(fileList[i]));
			if (!isVectorZero(prefab.transform.localPosition) ||
				!isVectorZero(prefab.transform.localEulerAngles) ||
				!isVectorEqual(prefab.transform.localScale, Vector3.one))
			{
				Debug.LogError("预设根节点变换错误:" + fileList[i], prefab);
			}
		}
		clearProgressBar();
		Debug.Log("完成检查预设变换");
	}
	[MenuItem("检查资源/检查所有Prefab文件MeshCollider的模型Read-Write", false, 112)]
	public static void findMeshColliderFBXReadAndWirte()
	{
		if (!EditorUtility.DisplayDialog("提示", "是否开始检查所有预设的MeshCollider的模型是否开启Read-Write? ", "确认", "取消"))
		{
			return;
		}

		Debug.Log("------开始检查预设的模型是否开启Read-Write------");
		DateTime startTime = DateTime.Now;
		HashSet<Mesh> saveErrorObj = new();
		List<string> fileList = findFilesNonAlloc(F_GAME_RESOURCES_PATH, ".prefab");
		int processCount = fileList.Count;
		for (int i = 0; i < fileList.Count; ++i)
		{
			string filePath = fullPathToProjectPath(fileList[i]);
			GameObject targetPrefab = loadGameObject(filePath);
			if (!targetPrefab.TryGetComponent<MeshCollider>(out var collider) ||
				!targetPrefab.TryGetComponent<MeshFilter>(out var meshFiliter))
			{
				continue;
			}
			Mesh mesh = meshFiliter.sharedMesh;
			if (mesh == null)
			{
				Debug.LogError(meshFiliter.gameObject.name + "的Mesh丢失", targetPrefab);
				continue;
			}
			if (!saveErrorObj.Add(collider.sharedMesh))
			{
				continue;
			}

			// 检查读写属性
			if (!mesh.isReadable)
			{
				UObject go = loadAsset(AssetDatabase.GetAssetPath(mesh));
				Debug.LogError(targetPrefab.name + "没有开启Read-Write Enable. Path: " + filePath + ", Mesh: " + mesh.name, go);
			}
			displayProgressBar("预设的模型是否开启Read-Write", "进度: ", i + 1, processCount);
		}
		clearProgressBar();
		Debug.Log("------结束检查预设的模型是否开启Read-Write,耗时: " + (DateTime.Now - startTime));
	}
	[MenuItem("检查资源/检查所有场景的MeshCollider模型Read-Write", false, 113)]
	public static void findAllSceneMeshColliderFBXReadAndWirte()
	{
		if (!EditorUtility.DisplayDialog("提示", "是否开始检查所有场景的MeshCollider模型是否开启Read-Write? ", "确认", "取消"))
		{
			return;
		}

		string curScenePath = SceneManager.GetActiveScene().path;
		Debug.Log("------开始检查场景的模型是否开启Read-Write------");
		DateTime startTime = DateTime.Now;
		List<string> fileList = findFilesNonAlloc(P_GAME_RESOURCES_PATH, ".unity");
		HashSet<Mesh> saveErrorObj = new();
		int sceneCount = fileList.Count;
		for (int i = 0; i < sceneCount; ++i)
		{
			string currentScene = fileList[i];
			Debug.Log("进入场景 ==>> " + currentScene);
			displayProgressBar("MeshCollider的Read-Write是否启用", "进度: ", i + 1, sceneCount);
			EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
			foreach (GameObject go in UObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
			{
				if (!go.TryGetComponent<MeshCollider>(out var collider) ||
					!go.TryGetComponent<MeshFilter>(out var meshFiliter))
				{
					continue;
				}
				if (meshFiliter.sharedMesh == null)
				{
					Debug.LogError(meshFiliter.gameObject.name + "的Mesh丢失");
					continue;
				}
				if (collider.sharedMesh == null)
				{
					Debug.LogError("MeshCollider的Mesh不存在: " + go.name + ", 场景:" + currentScene);
					continue;
				}
				if (!saveErrorObj.Add(collider.sharedMesh))
				{
					continue;
				}

				// 检查读写属性
				if (!collider.sharedMesh.isReadable)
				{
					UObject obj = loadAsset(AssetDatabase.GetAssetPath(collider.sharedMesh));
					Debug.LogError("MeshCollider的Mesh没有开启Read-Write: " + collider.sharedMesh.name, obj);
				}
			}
		}
		clearProgressBar();
		// 恢复打开之前的场景
		EditorSceneManager.OpenScene(curScenePath, OpenSceneMode.Single);
		Debug.Log("------结束检查所有场景的模型是否开启Read-Write,耗时: " + (DateTime.Now - startTime));
	}
	[MenuItem("检查资源/【当前场景】检查场景MeshCollider模型Read-Write", false, 114)]
	public static void findSceneMeshColliderFBXReadAndWirte()
	{
		if (!EditorUtility.DisplayDialog("提示", "是否开始检查当前场景的MeshCollider模型是否开启Read-Write? ", "确认", "取消"))
		{
			return;
		}

		Debug.Log("------开始检查Scene的Mesh是否开启Read-Write------");
		DateTime startTime = DateTime.Now;
		HashSet<Mesh> saveErrorObj = new();
		GameObject[] sceneObjects = UObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
		int objectCount = sceneObjects.Length;
		for (int i = 0; i < objectCount; ++i)
		{
			displayProgressBar("MeshCollider的Mesh的Read-Write是否启用", "进度: ", i + 1, objectCount);
			if (!sceneObjects[i].TryGetComponent<MeshCollider>(out var collider) ||
				!sceneObjects[i].TryGetComponent<MeshFilter>(out var meshFiliter))
			{
				continue;
			}
			if (meshFiliter.sharedMesh == null)
			{
				Debug.LogError(meshFiliter.gameObject.name + "的Mesh丢失");
				continue;
			}
			if (collider.sharedMesh == null)
			{
				Debug.LogError("MeshCollider的Mesh不存在: " + sceneObjects[i].name, sceneObjects[i]);
				continue;
			}
			if (!saveErrorObj.Add(collider.sharedMesh))
			{
				continue;
			}

			// 检查读写属性
			if (!collider.sharedMesh.isReadable)
			{
				UObject go = loadAsset(AssetDatabase.GetAssetPath(collider.sharedMesh));
				Debug.LogError("MeshCollider的Mesh没有开启Read-Write Enable: " + collider.sharedMesh.name, go);
			}
		}
		clearProgressBar();
		Debug.Log("------结束检查场景的Mesh是否开启Read-Write,耗时: " + (DateTime.Now - startTime));
	}
	[MenuItem("检查资源/【当前场景】检查场景的Layer空对象", false, 115)]
	public static void findSceneLayerNull()
	{
		Debug.Log("------开始检查当前Scene是否含有Layer空对象------");
		foreach (GameObject item in UObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
		{
			string layerName = LayerMask.LayerToName(item.layer);
			if (layerName == EMPTY)
			{
				Debug.LogError("Find Null Layer Object ==>" + item.name, item);
			}
		}
		Debug.Log("------结束检查当前Scene是否含有Layer空对象------");
	}
	[MenuItem("检查资源/检查资源中是否有代码文件引用丢失", false, 116)]
	public static void checkLoseScriptsReferenceObject()
	{
		Debug.Log("开始检查是否有代码文件引用丢失...");
		// 查找所有的脚本的GUID
		List<string> scripGUIDList = new(getAllGUIDBySuffixInFilePath(F_ASSETS_PATH, ".cs.meta", "脚本").Keys);
		// 丢失脚本引用的资源字典(key = "引用了丢失脚本的资源路径",value = 该资源丢失的脚本的guid列表)
		Dictionary<string, List<string>> missingRefAssetsList = new();
		// UGUI路径常量
		const string uiPath = "Packages/com.unity.ugui";
		// 所有引用了脚本的.prefab与.unity文件
		foreach (FileGUIDLines fileInfo in getScriptRefrenceFileText(F_ASSETS_PATH).Values)
		{
			foreach (string guid in fileInfo.mContainGUIDLines)
			{
				// 与存着所有的脚本GUID的列表进行比对,剔除UGUI脚本的GUID
				if (scripGUIDList.Contains(guid) || AssetDatabase.GUIDToAssetPath(guid).Contains(uiPath))
				{
					continue;
				}
				missingRefAssetsList.getOrAddNew(fileInfo.mProjectFileName).Add(guid);
			}
		}

		// 输出定位丢失脚本引用的资源信息
		debugMissingRefInformation(missingRefAssetsList, "代码");
		Debug.Log("完成检查是否有代码引用丢失");
	}
	[MenuItem("检查资源/检查布局预制体的缩放值是否符合规范", false, 116)]
	public static void checkLayoutPrefabScale()
	{
		Debug.Log("开始检查检查布局预制体资源的缩放值是否符合规范...");
		foreach (string file in findFilesNonAlloc(F_UI_PREFAB_PATH, ".prefab"))
		{
			// 加载指定路径预制体
			GameObject targetPrefab = loadGameObject(fullPathToProjectPath(file));
			// 获取所有子对象
			foreach (Transform child in targetPrefab.transform.GetComponentsInChildren<Transform>(true))
			{
				if (child.localScale != Vector3.one)
				{
					Debug.LogError(targetPrefab.name + " 该Layout预制体的子节点 " + child.name + "不符合规范", targetPrefab);
				}
			}
		}
		Debug.Log("完成检查布局预制体资源的缩放值是否符合规范");
	}
	[MenuItem("检查资源/检查布局中是否有空的Image或Sprite", false, 117)]
	public static void checkEmptyImageOrSpriteObject()
	{
		Debug.Log("开始检查是否有空的Image或Sprite...");
		Dictionary<string, GameObject> objectList = new();
		// 查找路径同时加载Prefab对象
		foreach (string path in findFilesNonAlloc(F_GAME_RESOURCES_PATH, ".prefab"))
		{
			objectList.Add(fullPathToProjectPath(path), loadAsset(path) as GameObject);
		}
		foreach (var item in objectList)
		{
			foreach (Image image in item.Value.GetComponentsInChildren<Image>(true))
			{
				if (image.sprite == null)
				{
					Debug.LogError("布局 " + item.Value.name + "的Image组件没有设置Sprite,obj:" + image.name, item.Value);
				}
			}
			foreach (SpriteRenderer spriteRenderer in item.Value.GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (spriteRenderer.sprite == null)
				{
					Debug.LogError("Prefab " + item.Value.name + "的SpriteRenderer组件没有设置Sprite,obj:" + spriteRenderer.name, item.Value);
				}
			}
		}
		Debug.Log("完成检查空的Image或Sprite");
	}
	[MenuItem("检查资源/检查是否使用了内置图片的UI节点", false, 118)]
	public static void checkBuiltInImageAndReplace()
	{
		Debug.Log("开始检查是否有使用了内置图片...");
		List<string> fileListPrefab = new();
		findFiles(F_UI_PREFAB_PATH, fileListPrefab, ".prefab");
		findFiles(F_RESORUCES_UI_PREFAB_PATH, fileListPrefab, ".prefab");
		int fileCount = fileListPrefab.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查内置图片", "进度: ", i + 1, fileCount);
			string file = fileListPrefab[i];
			openTxtFileLines(file, out string[] lines);
			doCheckBuiltinUI(file, lines);
			doCheckBuiltinFont(file, lines);
		}
		clearProgressBar();
		Debug.Log("完成检查是否有使用了内置图片");
	}
	[MenuItem("检查资源/检查资源文件名规范", false, 119)]
	public static void checkResourcesName()
	{
		Debug.Log("开始检查资源文件名");
		List<string> files = new();
		findFiles(P_GAME_RESOURCES_PATH, files);
		findFiles(P_RESOURCES_PATH, files);

		int fileCount = files.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查资源文件名", "进度: ", i + 1, fileCount);
			string fileName = getFileNameWithSuffix(files[i]);
			if (!fileName.endWith(".meta") && (fileName.Contains(' ') || fileName.Contains('/')))
			{
				Debug.LogError("文件名不能包含空格或斜杠，文件名：" + fileName, loadAsset(files[i]));
			}
		}
		clearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查资源/检查所有UI布局中根节点是否为不保持宽高比的ScaleAnchor", false, 121)]
	public static void checkLayoutRootScaleAnchor()
	{
		Debug.Log("开始检查所有UI布局中根节点是否为不保持宽高比的ScaleAnchor");
		List<string> fileList = findFilesNonAlloc(F_UI_PREFAB_PATH, ".prefab");
		int processCount = fileList.Count;
		for (int i = 0; i < processCount; ++i)
		{
			displayProgressBar("检查UI布局中根节点是否为不保持宽高比的ScaleAnchor", "进度: ", i + 1, processCount);
			// 加载指定路径预制体
			GameObject targetPrefab = loadGameObject(fullPathToProjectPath(fileList[i]));
			// 获取布局的根节点ScaleAnchor
			if (!targetPrefab.transform.root.TryGetComponent<ScaleAnchor>(out var anchor))
			{
				Debug.LogError(targetPrefab.name + " 该Layout预制体的根节点没有ScaleAnchor组件", targetPrefab);
				continue;
			}
			if (anchor.mKeepAspect)
			{
				Debug.LogError(targetPrefab.name + " 该Layout预制体的根节点ScaleAnchor不满足不保持宽高比条件", targetPrefab);
			}
		}
		clearProgressBar();
		Debug.Log("结束检查所有UI布局中根节点是否为不保持宽高比的ScaleAnchor");
	}
	[MenuItem("检查资源/检查所有UI Prefab上的多语言文本组件", false, 125)]
	public static void checkAllUIPrefabLocalizationComponent()
	{
		Debug.Log("开始检查所有UIPrefab...");
		// 查找资源文件中的所有prefab文件
		List<string> fileList = findFilesNonAlloc(F_ASSETS_PATH, ".prefab");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查文本", "进度: ", i + 1, fileCount);
			GameObject prefabObj = loadGameObject(fullPathToProjectPath(fileList[i]));
			if (prefabObj == null)
			{
				continue;
			}
			foreach (Text text in prefabObj.GetComponentsInChildren<Text>(true))
			{
				if (!text.gameObject.TryGetComponent<LocalizationText>(out _) && !text.gameObject.TryGetComponent<LocalizationRuntimeText>(out _))
				{
					Debug.Log("文本:" + text.text + ", name:" + text.name + ", " + colorString("FF0000", "没有多语言组件") + ", prefab:" + getFileNameNoSuffixNoDir(fileList[i]), prefabObj);
				}
			}
			AssetDatabase.SaveAssetIfDirty(prefabObj);
		}
		clearProgressBar();
		Debug.Log("完成检查");
	}
	[MenuItem("检查资源/查找所有UI Prefab上的多语言文本", false, 126)]
	public static void checkAllUIPrefabChineseText()
	{
		Debug.Log("开始检查所有UIPrefab...");
		// 查找资源文件中的所有prefab文件
		List<string> fileList = findFilesNonAlloc(F_ASSETS_PATH, ".prefab");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查文本", "进度: ", i + 1, fileCount);
			GameObject prefabObj = loadGameObject(fullPathToProjectPath(fileList[i]));
			if (prefabObj == null)
			{
				continue;
			}
			foreach (Text text in prefabObj.GetComponentsInChildren<Text>(true))
			{
				if (hasChinese(text.text))
				{
					Debug.Log("文本:" + text.text + ", name:" + text.name + ", prefab:" + getFileNameNoSuffixNoDir(fileList[i]), prefabObj);
				}
			}
			AssetDatabase.SaveAssetIfDirty(prefabObj);
		}
		clearProgressBar();
		Debug.Log("完成检查");
	}
	[MenuItem("检查资源/检查所有代码中的中文文本", false, 127)]
	public static void checkAllCodeChineseText()
	{
		Debug.Log("开始检查所有文本...");
		// 查找资源文件中的所有代码文件
		List<string> fileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查代码中文", "进度: ", i + 1, fileCount);
			List<string> lines = new();
			List<string> chineseList = new();
			doCheckCodeChinese(fileList[i], lines, chineseList);
			int count = lines.Count;
			for (int j = 0; j < count; ++j)
			{
				Debug.Log("包含中文字符串的代码:" + lines[j].TrimStart() + ",提取到的中文:" + chineseList[j] + ", 所在文件:" + fullPathToProjectPath(fileList[i]));
			}
		}
		clearProgressBar();
		Debug.Log("完成检查");
	}
	[MenuItem("检查资源/检查所有UI Prefab上的没有添加多语言的带文字图片", false, 128)]
	public static void checkAllUIPrefabChineseImageNoLocalization()
	{
		Debug.Log("开始检查所有UIPrefab...");
		// 查找资源文件中的所有prefab文件
		List<string> fileList = findFilesNonAlloc(F_ASSETS_PATH, ".prefab");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查文本", "进度: ", i + 1, fileCount);
			GameObject prefabObj = loadGameObject(fullPathToProjectPath(fileList[i]));
			if (prefabObj == null)
			{
				continue;
			}
			foreach (Image image in prefabObj.GetComponentsInChildren<Image>(true))
			{
				// 使用的图片带_Chinese后缀,但是没有添加LocalizationImage的
				if (image.sprite != null && 
					image.sprite.name.EndsWith("_Chinese") && 
					!image.gameObject.TryGetComponent<LocalizationImage>(out _))
				{
					Debug.Log("图片:" + image.name + ", sprite:" + image.sprite.name + ", " + colorString("FF0000", "没有多语言图片组件") + ", prefab:" + getFileNameNoSuffixNoDir(fileList[i]), prefabObj);
				}
			}
			AssetDatabase.SaveAssetIfDirty(prefabObj);
		}
		clearProgressBar();
		Debug.Log("完成检查");
	}
	[MenuItem("检查资源/检查被单一引用的文件是否与引用其的文件在同一个目录", false, 129)]
	public static void checkBeSingleUsedFileStayWith()
	{
		// 查找该文件的所有引用
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path.isEmpty())
		{
			if (!EditorUtility.DisplayDialog("检查所有被单一引用的文件", "未选中任何目录,是否想要查找GameResources中所有资源?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}

		Debug.Log("开始检查所有被单一引用的文件...");
		// 选择的是文件,则只查找文件的引用
		if (isFileExist(path))
		{
			doCheckSingleUsedFile(path, getAllResourceFileText(), false);
		}
		// 选择的是目录,则查找目录中所有文件的引用
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("检查所有被单一引用的文件", "确认查找文件夹中所有文件? " + path, "确认", "取消"))
			{
				var allFileText = getAllResourceFileText();
				// 不查找meta文件的引用
				List<string> validFiles = new();
				foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
				{
					validFiles.addIf(item, !item.EndsWith(".meta"));
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("检查所有被单一引用的文件", "进度: ", i + 1, count);
					doCheckSingleUsedFile(validFiles[i], allFileText, false);
				}
				clearProgressBar();
			}
		}
		AssetDatabase.Refresh();
		Debug.Log("完成检查");
	}
	[MenuItem("检查资源/打开检查资源界面", false, 131)]
	public static void checkResWindows()
	{
		EditorWindow.GetWindow<CheckResourcesWindow>(true, "检查资源", true).start();
	}
	[MenuItem("检查资源/打开提取文本组件界面", false, 131)]
	public static void checkFindTextComponentWindows()
	{
		EditorWindow.GetWindow<FindTextWindow>(true, "提取文本组件", true).start();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void doCheckCodeChinese(string fileName, List<string> chineseLineList, List<string> chineseStrList)
	{
		// 查找资源文件中的所有prefab文件
		foreach (string line in openTxtFileLines(fileName))
		{
			if (line.Contains("logError") ||
				line.Contains("logWarning") ||
				line.Contains("Debug.") ||
				line.Contains("log") ||
				line.Contains("logErrorBase") ||
				line.Contains("logBase") ||
				line.Contains("CustomLabel") ||
				line.Contains("//"))
			{
				continue;
			}
			string chineseStr = "";
			int strStartIndex = -1;
			int length = line.Length;
			for (int j = 0; j < length; ++j)
			{
				if (line[j] == '\"')
				{
					strStartIndex = strStartIndex < 0 ? j : -1;
					continue;
				}
				if (strStartIndex >= 0 && isChinese(line[j]))
				{
					chineseStr = line.rangeToFirst(strStartIndex + 1, '\"');
					break;
				}
			}
			if (chineseStr.Length > 0)
			{
				if (line.Contains("setText"))
				{
					Debug.LogError("多语言文字需要使用setLocalizationText,不能使用setText:" + line);
				}
				chineseLineList.Add(line);
				chineseStrList.Add(chineseStr);
			}
		}
	}
}