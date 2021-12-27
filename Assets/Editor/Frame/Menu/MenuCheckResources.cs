using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuCheckResources : EditorCommonUtility
{
	[MenuItem("检查资源/查找重名文件", false, 1)]
	public static void checkFileNameConflict()
	{
		Debug.Log("开始检查重名文件...");
		var fileList = new List<string>();
		// 查找资源文件中的所有文件
		findFiles(FrameDefine.F_GAME_RESOURCES_PATH, fileList);
		// 去除meta文件
		removeMetaFile(fileList);
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("查找重名文件", "进度: ", i + 1, fileCount);
			string curFileName = getFileName(fileList[i]);
			foreach (var item0 in fileList)
			{
				if (fileList[i] != item0 && curFileName == getFileName(item0))
				{
					Debug.LogError("文件命名冲突:" + fileList[i] + "\n" + item0, loadAsset(fullPathToProjectPath(item0)));
					break;
				}
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查重名文件");
	}
	[MenuItem("检查资源/查找文件引用", false, 2)]
	public static void searchRefrence()
	{
		// 查找该文件的所有引用
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (isEmpty(path))
		{
			if (!EditorUtility.DisplayDialog("查找所有资源引用", "未选中任何目录,是否想要查找GameResources中所有资源的引用?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = FrameDefine.P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始查找资源引用:" + path + "...");
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
				var allFileText = getAllResourceFileText();
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				// 不查找meta文件的引用
				var validFiles = new List<string>();
				foreach (var item in files)
				{
					if (!item.EndsWith(".meta"))
					{
						validFiles.Add(item);
					}
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("查找所有资源引用", "进度: ", i + 1, count);
					doSearchRefrence(validFiles[i], allFileText);
				}
				EditorUtility.ClearProgressBar();
			}
		}
		Debug.Log("完成查找资源引用");
	}
	[MenuItem("检查资源/查找图集引用", false, 4)]
	public static void checkAtlasRefrence()
	{
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (isEmpty(path))
		{
			if (!EditorUtility.DisplayDialog("检查图集引用", "未选中任何目录,是否想要检查GameResources中所有图集的引用?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = FrameDefine.P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查图集引用:" + path + "...");
		if (isFileExist(path))
		{
			if (endWith(path, "png", false))
			{
				doCheckAtlasRefrence(path, getAllFileText(FrameDefine.F_GAME_RESOURCES_PATH + FrameDefine.LAYOUT + "/"));
			}
		}
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("检查图集引用", "确认查找文件夹中图集引用? " + path, "确认", "取消"))
			{
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				// 只查找png
				var validFiles = new List<string>();
				foreach (var item in files)
				{
					if (endWith(item, "png", false))
					{
						validFiles.Add(item);
					}
				}
				var allFileText = getAllFileText(FrameDefine.F_GAME_RESOURCES_PATH + FrameDefine.LAYOUT + "/");
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					string filePath = validFiles[i];
					displayProgressBar("检查图集引用", "进度: ", i + 1, count);
					doCheckAtlasRefrence(filePath, allFileText);
				}
				EditorUtility.ClearProgressBar();
			}
		}
		Debug.Log("完成检查图集引用");
	}
	[MenuItem("检查资源/检查未引用的资源", false, 99)]
	public static void checkUnusedResources()
	{
		bool checkAll = false;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (isEmpty(path))
		{
			if (!EditorUtility.DisplayDialog("查找未引用的资源", "未选中任何目录,是否想要查找GameResources中所有的未引用的资源?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = FrameDefine.P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始查找未引用的资源:" + path + "...");
		if (isFileExist(path))
		{
			doCheckUnusedTexture(path, getAllResourceFileText());
		}
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("查找未引用的资源", "确认查找文件夹中所有未使用资源? " + path, "确认", "取消"))
			{
				var allFileText = getAllResourceFileText();
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				var validFiles = new List<string>();
				foreach (var item in files)
				{
					if (!endWith(item, ".meta", false))
					{
						validFiles.Add(item);
					}
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					string filePath = validFiles[i];
					displayProgressBar("查找未引用的资源", "进度: ", i + 1, count);
					doCheckUnusedTexture(filePath, allFileText);
				}
				EditorUtility.ClearProgressBar();
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
		foreach (var item in objects)
		{
			var renderers = item.GetComponentsInChildren<Renderer>();
			if (renderers == null)
			{
				continue;
			}
			Vector3 center = Vector3.zero;
			foreach (var render in renderers)
			{
				center += render.bounds.center;
			}
			if (renderers.Length > 0)
			{
				center /= renderers.Length;
			}
			// 计算包围盒
			Bounds bounds = new Bounds(center, Vector3.zero);
			foreach (var render in renderers)
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
		var materialGUIDsList = new List<string>(getAllGUIDBySuffixInFilePath(FrameDefine.F_ASSETS_PATH, ".mat.meta", "材质").Keys);
		// 所有引用Material的.prefab与.unity文件的集合
		var refMaterialFilesDic = getMaterialRefrenceFileText(FrameDefine.F_ASSETS_PATH);
		// 丢失脚本引用的资源字典(key = "引用了丢失材质的资源路径",value = 该资源丢失的材质的guid列表)
		var missingRefAssetsList = new Dictionary<string, List<string>>();
		foreach (var item in refMaterialFilesDic)
		{
			FileGUIDLines fileInfo = item.Value;
			foreach (var guidsStr in fileInfo.mContainGUIDLines)
			{
				string[] materialGUIDsArr = split(guidsStr, '-');
				foreach (var guid in materialGUIDsArr)
				{
					// 与存着所有的材质球GUID的列表进行比对
					if (materialGUIDsList.Contains(guid))
					{
						continue;
					}
					if (!missingRefAssetsList.TryGetValue(fileInfo.mProjectFileName, out List<string> missingGUIDList))
					{
						missingGUIDList = new List<string>();
						missingRefAssetsList.Add(fileInfo.mProjectFileName, missingGUIDList);
					}
					missingGUIDList.Add(guid);
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
		if (isEmpty(path))
		{
			if (!EditorUtility.DisplayDialog("材质贴图是否存在", "未选中任何目录,是否想要查找GameResources中所有材质的贴图是否存在?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = FrameDefine.P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查材质贴图是否存在:" + path);
		// 选择的是文件,则只查找文件的引用
		if (isFileExist(path))
		{
			checkMaterialTextureValid(path, getAllResourceFileText());
		}
		// 选择的是目录,则查找目录中所有文件的引用
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("材质贴图是否存在", "确认查找文件夹中所有材质的贴图是否存在? " + path, "确认", "取消"))
			{
				var allFileText = getAllResourceFileText();
				string[] files = Directory.GetFiles(path, "*.mat", SearchOption.AllDirectories);
				// 因为后缀长度小于等于3时会查找出所有包含此后缀的文件,并不一定只有指定后缀的文件
				// 所以此处需要过滤掉不需要的文件
				var validFiles = new List<string>();
				foreach (var item in files)
				{
					if (item.EndsWith(".mat"))
					{
						validFiles.Add(item);
					}
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("材质贴图是否存在", "进度: ", i + 1, count);
					checkMaterialTextureValid(validFiles[i], allFileText);
				}
				EditorUtility.ClearProgressBar();
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
		if (isEmpty(path))
		{
			if (!EditorUtility.DisplayDialog("材质是否引用了shader未使用的贴图", "未选中任何目录,是否想要检查GameResources中所有材质的shader的贴图?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = FrameDefine.P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查材质是否引用了shader未使用的贴图:" + path);
		// 选择的是文件,则只查找文件的引用
		if (isFileExist(path))
		{
			checkMaterialTexturePropertyValid(path, getAllResourceFileText(), true);
		}
		// 选择的是目录,则查找目录中所有文件的引用
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("材质是否引用了shader未使用的贴图", "确认查找文件夹中所有材质的贴图属性? " + path, "确认", "取消"))
			{
				var allFileText = getAllResourceFileText();
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				// 因为后缀长度小于等于3时会查找出所有包含此后缀的文件,并不一定只有指定后缀的文件
				// 所以此处需要过滤掉不需要的文件
				var validFiles = new List<string>();
				foreach (var item in files)
				{
					if (item.EndsWith(".mat"))
					{
						validFiles.Add(item);
					}
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					displayProgressBar("材质是否引用了shader未使用的贴图", "进度: ", i + 1, count);
					checkMaterialTexturePropertyValid(validFiles[i], allFileText, true);
				}
				EditorUtility.ClearProgressBar();
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
		var allGameResourcesAssetGUID = getAllGUIDAndSpriteIDBySuffixInFilePath(FrameDefine.P_GAME_RESOURCES_PATH, ".meta", "所有热更资源");
		// 所有非热更资源的GUID
		var allResourcesAssetGUID = getAllGUIDAndSpriteIDBySuffixInFilePath(FrameDefine.P_RESOURCES_PATH, ".meta", "所有非热更资源");

		// 所有热更资源中所有带引用的文件的集合
		var refGameResourcesFilesDic = getAllRefrenceFileText(FrameDefine.P_GAME_RESOURCES_PATH);
		// 所有非热更资源中所有带引用的文件的集合
		var refResourcesFilesDic = getAllRefrenceFileText(FrameDefine.P_RESOURCES_PATH);
		// 错误引用资源的字典
		var errorRefAssetDic = new Dictionary<string, Dictionary<string, string>>();

		// 检测不合法引用
		doCheckRefGUIDInFilePath(errorRefAssetDic, refGameResourcesFilesDic, allResourcesAssetGUID);
		doCheckRefGUIDInFilePath(errorRefAssetDic, refResourcesFilesDic, allGameResourcesAssetGUID);

		// 输出定位丢失脚本引用的资源信息
		foreach (var item in errorRefAssetDic)
		{
			// 将丢失引用的资源中的丢失引用对象的列表存入列表中
			var missingRefObjectsList = new List<string>();
			setCheckRefObjectsList(missingRefObjectsList, item.Key, item.Value.Keys);
			var go = loadAsset(item.Key);
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
		if (isEmpty(path))
		{
			if (!EditorUtility.DisplayDialog("检查图集中不存在的图片", "未选中任何目录,是否想要检查GameResources中所有图集中不存在的图片?", "确认", "取消"))
			{
				return;
			}
			checkAll = true;
			path = FrameDefine.P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		Debug.Log("开始检查图集中不存在的图片:" + path + "...");
		if (isFileExist(path))
		{
			if (endWith(path, "png", false))
			{
				doCheckAtlasNotExistSprite(path);
			}
		}
		else if (isDirExist(path))
		{
			if (checkAll || EditorUtility.DisplayDialog("检查图集中不存在的图片", "确认查找文件夹中图集中不存在的图片? " + path, "确认", "取消"))
			{
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				// 只查找png
				var validFiles = new List<string>();
				foreach (var item in files)
				{
					if (endWith(item, "png", false))
					{
						validFiles.Add(item);
					}
				}
				// 开始查找所有文件的引用
				int count = validFiles.Count;
				for (int i = 0; i < count; ++i)
				{
					string filePath = validFiles[i];
					displayProgressBar("检查图集中不存在的图片", "进度: ", i + 1, count);
					doCheckAtlasNotExistSprite(filePath);
				}
				EditorUtility.ClearProgressBar();
			}
		}
		Debug.Log("完成检查图集中不存在的图片");
	}
	[MenuItem("检查资源/检查是否有重复文件", false, 109)]
	public static void checkSameAsset()
	{
		Debug.Log("开始检查是否有重复文件");
		// 路径下所有资源文件列表
		// 重复的资源的路径字典(key: MD5字符串, value: 相同资源的列表)
		var hasSameAssetsDic = new Dictionary<string, List<string>>();
		var resourcesFilesList = new List<string>();
		findFiles(FrameDefine.P_GAME_RESOURCES_PATH, resourcesFilesList);
		int fileCount = resourcesFilesList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("正在查找是否有重复资源", "进度: ", i + 1, fileCount);
			// 忽略.meta文件
			if (resourcesFilesList[i].Contains(".meta"))
			{
				continue;
			}
			string MD5Str = generateFileMD5(resourcesFilesList[i]);
			if (!hasSameAssetsDic.TryGetValue(MD5Str, out List<string> sameFilelPath))
			{
				sameFilelPath = new List<string>();
				hasSameAssetsDic.Add(MD5Str, sameFilelPath);
			}
			sameFilelPath.Add(resourcesFilesList[i]);
		}

		// 输出结果
		foreach (var element in hasSameAssetsDic)
		{
			if (element.Value.Count > 1)
			{
				Debug.LogError("出现重复的资源,路径为: " + stringsToString(element.Value, '\n'), loadAsset(element.Value[0]));
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查是否有相同文件");
	}
	[MenuItem("检查资源/检查预设根节点是否带变换", false, 111)]
	public static void checkPrefabRootTransform()
	{
		Debug.Log("开始检查预设变换...");
		var fileList = new List<string>();
		findFiles(FrameDefine.F_GAME_RESOURCES_PATH, fileList, ".prefab");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查预设变换", "进度: ", i + 1, fileCount);
			var prefab = loadGameObject(fullPathToProjectPath(fileList[i]));
			if (!isVectorZero(prefab.transform.localPosition) ||
				!isVectorZero(prefab.transform.localEulerAngles) ||
				!isVectorEqual(prefab.transform.localScale, Vector3.one))
			{
				Debug.LogError("预设根节点变换错误:" + fileList[i], prefab);
			}
		}
		EditorUtility.ClearProgressBar();
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
		HashSet<Mesh> saveErrorObj = new HashSet<Mesh>();
		List<string> fileList = new List<string>();
		findFiles(FrameDefine.F_GAME_RESOURCES_PATH, fileList, ".prefab");
		int processCount = fileList.Count;
		for (int i = 0; i < fileList.Count; ++i)
		{
			string filePath = fullPathToProjectPath(fileList[i]);
			var targetPrefab = loadGameObject(filePath);
			var collider = targetPrefab.GetComponent<MeshCollider>();
			if (collider == null)
			{
				continue;
			}
			var meshFiliter = targetPrefab.GetComponent<MeshFilter>();
			if (meshFiliter == null)
			{
				continue;
			}
			Mesh mesh = meshFiliter.sharedMesh;
			if (mesh == null)
			{
				Debug.LogError(meshFiliter.gameObject.name + "的Mesh丢失", targetPrefab);
				continue;
			}
			if (saveErrorObj.Contains(collider.sharedMesh))
			{
				continue;
			}

			// 检查读写属性
			saveErrorObj.Add(collider.sharedMesh);
			if (!mesh.isReadable)
			{
				var go = loadAsset(AssetDatabase.GetAssetPath(mesh));
				Debug.LogError(targetPrefab.name + "没有开启Read-Write Enable. Path: " + filePath + ", Mesh: " + mesh.name, go);
			}
			displayProgressBar("预设的模型是否开启Read-Write", "进度: ", i + 1, processCount);
		}
		EditorUtility.ClearProgressBar();
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
		List<string> fileList = new List<string>();
		findFiles(FrameDefine.P_GAME_RESOURCES_PATH, fileList, ".unity");
		HashSet<Mesh> saveErrorObj = new HashSet<Mesh>();
		int sceneCount = fileList.Count;
		for (int i = 0; i < sceneCount; ++i)
		{
			string currentScene = fileList[i];
			Debug.Log("进入场景 ==>> " + currentScene);
			displayProgressBar("MeshCollider的Read-Write是否启用", "进度: ", i + 1, sceneCount);
			EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
			GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();
			int objectCount = sceneObjects.Length;
			for (int j = 0; j < objectCount; ++j)
			{
				var collider = sceneObjects[j].GetComponent<MeshCollider>();
				if (collider == null)
				{
					continue;
				}
				var meshFiliter = sceneObjects[j].GetComponent<MeshFilter>();
				if (meshFiliter == null)
				{
					continue;
				}
				Mesh mesh = meshFiliter.sharedMesh;
				if (mesh == null)
				{
					Debug.LogError(meshFiliter.gameObject.name + "的Mesh丢失");
					continue;
				}
				if (collider.sharedMesh == null)
				{
					Debug.LogError("MeshCollider的Mesh不存在: " + sceneObjects[j].name + ", 场景:" + currentScene);
					continue;
				}
				if (saveErrorObj.Contains(collider.sharedMesh))
				{
					continue;
				}

				// 检查读写属性
				saveErrorObj.Add(collider.sharedMesh);
				if (!collider.sharedMesh.isReadable)
				{
					var go = loadAsset(AssetDatabase.GetAssetPath(collider.sharedMesh));
					Debug.LogError("MeshCollider的Mesh没有开启Read-Write: " + collider.sharedMesh.name, go);
				}
			}
		}
		EditorUtility.ClearProgressBar();
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
		HashSet<Mesh> saveErrorObj = new HashSet<Mesh>();
		GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();
		int objectCount = sceneObjects.Length;
		for (int i = 0; i < objectCount; ++i)
		{
			displayProgressBar("MeshCollider的Mesh的Read-Write是否启用", "进度: ", i + 1, objectCount);
			var collider = sceneObjects[i].GetComponent<MeshCollider>();
			if (collider == null)
			{
				continue;
			}
			var meshFiliter = sceneObjects[i].GetComponent<MeshFilter>();
			if (meshFiliter == null)
			{
				continue;
			}
			Mesh mesh = meshFiliter.sharedMesh;
			if (mesh == null)
			{
				Debug.LogError(meshFiliter.gameObject.name + "的Mesh丢失");
				continue;
			}
			if (collider.sharedMesh == null)
			{
				Debug.LogError("MeshCollider的Mesh不存在: " + sceneObjects[i].name, sceneObjects[i]);
				continue;
			}
			if (saveErrorObj.Contains(collider.sharedMesh))
			{
				continue;
			}

			// 检查读写属性
			saveErrorObj.Add(collider.sharedMesh);
			if (!collider.sharedMesh.isReadable)
			{
				var go = loadAsset(AssetDatabase.GetAssetPath(collider.sharedMesh));
				Debug.LogError("MeshCollider的Mesh没有开启Read-Write Enable: " + collider.sharedMesh.name, go);
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("------结束检查场景的Mesh是否开启Read-Write,耗时: " + (DateTime.Now - startTime));
	}
	[MenuItem("检查资源/【当前场景】检查场景的Layer空对象", false, 115)]
	public static void findSceneLayerNull()
	{
		Debug.Log("------开始检查当前Scene是否含有Layer空对象------");
		GameObject[] allObj = GameObject.FindObjectsOfType<GameObject>();
		foreach (var item in allObj)
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
		var scripGUIDList = new List<string>(getAllGUIDBySuffixInFilePath(FrameDefine.F_ASSETS_PATH, ".cs.meta", "脚本").Keys);
		// 所有引用了脚本的.prefab与.unity文件
		var refSriptsFilesDic = getScriptRefrenceFileText(FrameDefine.F_ASSETS_PATH);
		// 丢失脚本引用的资源字典(key = "引用了丢失脚本的资源路径",value = 该资源丢失的脚本的guid列表)
		var missingRefAssetsList = new Dictionary<string, List<string>>();
		// UGUI路径常量
		const string uiPath = "Packages/com.unity.ugui";
		foreach (var item in refSriptsFilesDic)
		{
			FileGUIDLines fileInfo = item.Value;
			foreach (var guid in fileInfo.mContainGUIDLines)
			{
				// 与存着所有的脚本GUID的列表进行比对,剔除UGUI脚本的GUID
				if (scripGUIDList.Contains(guid) || AssetDatabase.GUIDToAssetPath(guid).Contains(uiPath))
				{
					continue;
				}
				if (!missingRefAssetsList.TryGetValue(fileInfo.mProjectFileName, out List<string> missingGUIDList))
				{
					missingGUIDList = new List<string>();
					missingRefAssetsList.Add(fileInfo.mProjectFileName, missingGUIDList);
				}
				missingGUIDList.Add(guid);
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
		List<string> fileList = new List<string>();
		findFiles(FrameDefine.F_LAYOUT_PATH, fileList, ".prefab");
		int processCount = fileList.Count;
		for (int i = 0; i < processCount; ++i)
		{
			//加载指定路径预制体
			string filePath = fullPathToProjectPath(fileList[i]);
			var targetPrefab = loadGameObject(filePath);
			//获取所有子对象
			var childTransformList = targetPrefab.transform.GetComponentsInChildren<Transform>(true);
			int count = childTransformList.Length;
			for (int n = 0; n < count; ++n)
			{
				var child = childTransformList[n];
				if (child.localScale != Vector3.one)
				{
					Debug.LogError(targetPrefab.name + " 该Layout预制体的子节点 " + child.name + "不符合规范", targetPrefab);
				}
			}
		}
		Debug.Log("完成检查布局预制体资源的缩放值是否符合规范");
	}
	[MenuItem("检查资源/检查布局中是否有Image引用丢失", false, 117)]
	public static void checkLoseImageReferenceObject()
	{
		Debug.Log("开始检查是否有布局文件图片是否引用丢失...");
		var objectList = new Dictionary<string, UnityEngine.Object>();
		var filePathList = new List<string>();
		// 查找路径同时加载Prefab对象
		findFiles(FrameDefine.F_LAYOUT_PATH, filePathList, ".prefab");
		foreach (var path in filePathList)
		{
			objectList.Add(fullPathToProjectPath(path), loadAsset(fullPathToProjectPath(path)));
		}

		// 输入相应日志信息
		var allSpriteIDMap = getAllGUIDAndSpriteIDBySuffixInFilePath(FrameDefine.P_GAME_RESOURCES_PATH, ".meta", "所有对应SpriteID");
		var allImageReferenceMap = getImageReferenceInfo(FrameDefine.P_LAYOUT_PATH, ".prefab", "检查Image的引用");
		foreach (var item in allImageReferenceMap)
		{
			foreach (var node in item.Value)
			{
				if (node.Value.mSpriteID == null)
				{
					continue;
				}
				if (!allSpriteIDMap.ContainsKey(node.Value.mSpriteID) && item.Value.TryGetValue(node.Value.mGameObjectID, out PrefabNodeItem outObj))
				{
					UnityEngine.Object prefabObj = objectList[item.Key];
					Debug.LogError("布局 " + prefabObj.name + "图片丢失了引用位于节点 " + outObj.mName, prefabObj);
				}
			}
		}
		Debug.Log("完成检查是否有布局文件图片是否引用丢失");
	}
	[MenuItem("检查资源/检查是否使用了内置图片的UI节点", false, 118)]
	public static void checkBuiltInImageAndReplace()
	{
		Debug.Log("开始检查是否有使用了内置图片...");
		var fileListPrefab = new List<string>();
		findFiles(FrameDefine.F_LAYOUT_PATH, fileListPrefab, ".prefab");
		findFiles(FrameDefine.F_RESOURCES_LAYOUT_PREFAB_PATH, fileListPrefab, ".prefab");
		int fileCount = fileListPrefab.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查内置图片", "进度: ", i + 1, fileCount);
			string file = fileListPrefab[i];
			openTxtFileLines(file, out string[] lines);
			doCheckBuiltinUI(file, lines);
			doCheckBuiltinFont(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查是否有使用了内置图片");
	}
	[MenuItem("检查资源/检查资源文件名规范", false, 119)]
	public static void checkResourcesName()
	{
		Debug.Log("开始检查资源文件名");
		List<string> files = new List<string>();
		findFiles(FrameDefine.P_GAME_RESOURCES_PATH, files);
		findFiles(FrameDefine.P_RESOURCES_PATH, files);

		int fileCount = files.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查资源文件名", "进度: ", i + 1, fileCount);
			string fileName = getFileName(files[i]);
			if (!endWith(fileName, ".meta") && (fileName.IndexOf(' ') >= 0 || fileName.IndexOf('/') >= 0))
			{
				Debug.LogError("文件名不能包含空格或斜杠，文件名：" + fileName, loadAsset(files[i]));
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查资源/检查所有UI布局中的Z值是否为0", false, 120)]
	public static void checkLayoutIsZero()
	{
		Debug.Log("开始检查所有UI布局中的Z值是否为0");
		List<string> fileList = new List<string>();
		findFiles(FrameDefine.F_LAYOUT_PATH, fileList, ".prefab");
		int processCount = fileList.Count;
		for (int i = 0; i < processCount; ++i)
		{
			displayProgressBar("检查所有UI布局中的Z值是否为0", "进度: ", i + 1, processCount);
			// 加载指定路径预制体
			string filePath = fullPathToProjectPath(fileList[i]);
			GameObject targetPrefab = loadGameObject(filePath);
			// 获取所有子对象
			var childTransformList = targetPrefab.transform.GetComponentsInChildren<Transform>(true);
			int count = childTransformList.Length;
			for (int n = 0; n < count; ++n)
			{
				var child = childTransformList[n];
				if (!isFloatZero(child.localPosition.z))
				{
					Debug.LogError(targetPrefab.name + " 该Layout预制体的子节点 " + child.name + "中Z值不为0", targetPrefab);
				}
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查所有UI布局中的Z值是否为0");
	}
	[MenuItem("检查资源/检查所有UI布局中根节点是否为不保持宽高比的ScaleAnchor", false, 121)]
	public static void checkLayoutRootScaleAnchor()
	{
		Debug.Log("开始检查所有UI布局中根节点是否为不保持宽高比的ScaleAnchor");
		List<string> fileList = new List<string>();
		findFiles(FrameDefine.F_LAYOUT_PATH, fileList, ".prefab");
		int processCount = fileList.Count;
		for (int i = 0; i < processCount; ++i)
		{
			displayProgressBar("检查UI布局中根节点是否为不保持宽高比的ScaleAnchor", "进度: ", i + 1, processCount);
			// 加载指定路径预制体
			string filePath = fullPathToProjectPath(fileList[i]);
			GameObject targetPrefab = loadGameObject(filePath);
			// 获取布局的根节点ScaleAnchor
			var anchor = targetPrefab.transform.root.GetComponent<ScaleAnchor>();
			if (anchor == null)
			{
				Debug.LogError(targetPrefab.name + " 该Layout预制体的根节点没有ScaleAnchor组件", targetPrefab);
				continue;
			}
			if (anchor.mKeepAspect)
			{
				Debug.LogError(targetPrefab.name + " 该Layout预制体的根节点ScaleAnchor不满足不保持宽高比条件", targetPrefab);
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查所有UI布局中根节点是否为不保持宽高比的ScaleAnchor");
	}
}