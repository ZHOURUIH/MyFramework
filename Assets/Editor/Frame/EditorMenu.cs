using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if USE_ILRUNTIME
using ILRuntime.Runtime.Enviorment;
#endif

public class ClassInfo
{
	public List<string> mLines;
	public string mFilePath;
	public int mFunctionLine;
	public ClassInfo()
	{
		mLines = new List<string>();
	}
}

public class EditorMenu : EditorCommonUtility
{
	protected const string KEY_FUNCTION = "resetProperty";
	protected const string CODE_LOCATE_KEYWORD = "代码检测";
	[MenuItem("快捷操作/启动游戏 _F5", false, 0)]
	public static void startGame()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(FrameDefine.START_SCENE, OpenSceneMode.Single);
		}
		EditorApplication.isPlaying = !EditorApplication.isPlaying;
	}
	[MenuItem("快捷操作/暂停游戏 _F6", false, 1)]
	public static void pauseGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.isPaused = !EditorApplication.isPaused;
		}
	}
	[MenuItem("快捷操作/单帧执行 _F7", false, 2)]
	public static void stepGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.Step();
		}
	}
	[MenuItem("快捷操作/打开初始场景 _F9", false, 3)]
	public static void jumpGameSceme()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(FrameDefine.START_SCENE);
		}
	}
	[MenuItem("快捷操作/调整物体坐标为整数 _F1", false, 30)]
	public static void adjustUIRectToInt()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			return;
		}
		EditorUtility.SetDirty(go);
		roundRectTransformToInt(go.GetComponent<RectTransform>());
	}
	[MenuItem("快捷操作/调整父节点使其完全包含所有子节点的范围", false, 31)]
	public static void adjustParentRect()
	{
		var selection = Selection.activeGameObject.transform as RectTransform;
		if (selection == null)
		{
			Debug.LogError("节点必须有RectTransform组件");
			return;
		}

		// 获得父节点的四个边界
		float left = float.MaxValue;
		float right = float.MinValue;
		float top = float.MinValue;
		float bottom = float.MaxValue;
		int childCount = selection.childCount;
		var childWorldPositionList = new Dictionary<RectTransform, Vector3>();
		for (int i = 0; i < childCount; ++i)
		{
			var child = selection.GetChild(i) as RectTransform;
			if (child == null)
			{
				Debug.LogError("子节点必须有RectTransform组件");
				return;
			}
			Vector2 size = getRectSize(child);
			Vector3 pos = child.position;
			left = getMin(pos.x - size.x * 0.5f, left);
			right = getMax(pos.x + size.x * 0.5f, right);
			top = getMax(pos.y + size.y * 0.5f, top);
			bottom = getMin(pos.y - size.y * 0.5f, bottom);
			childWorldPositionList.Add(child, pos);
		}

		// 设置父节点新的位置和大小,重新设置所有子节点的世界坐标
		selection.position = new Vector3(ceil((right + left) * 0.5f), ceil((top + bottom) * 0.5f));
		setRectSize(selection, new Vector2(ceil(right - left), ceil(top - bottom)));
		foreach (var item in childWorldPositionList)
		{
			item.Key.position = item.Value;
		}
	}
	[MenuItem("快捷操作/删除所有空文件夹", false, 32)]
	public static void deleteAllEmptyFolder()
	{
		deleteEmptyFolder(FrameDefine.F_GAME_RESOURCES_PATH);
	}
	[MenuItem("快捷操作/修复MeshCollider的模型引用", false, 33)]
	public static void fixMeshCollider()
	{
		GameObject[] selectObj = Selection.gameObjects;
		if (selectObj == null || selectObj.Length == 0)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}

		bool modified = false;
		foreach (var item in selectObj)
		{
			modified |= fixMeshOfMeshCollider(item);
		}
		if (modified)
		{
			EditorUtility.SetDirty(selectObj[0]);
		}
	}
	[MenuItem("快捷操作/生成选中对象的角色控制器", false, 34)]
	//--------------------------------------------------------------------------------------------------------------------------------
	public static void createGameObjectCollider()
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

			// 创建角色控制器并设置参数
			var controller = item.GetComponent<CharacterController>();
			if (controller == null)
			{
				controller = item.AddComponent<CharacterController>();
			}

			// 生成的高度不能小于半径的2倍,且不能大于4
			controller.height = bounds.size.y;
			controller.radius = clamp(bounds.size.x * 0.5f, 0.001f, controller.height * 0.5f);
			controller.center = new Vector3(0.0f, bounds.center.y - bounds.size.y * 0.5f + controller.height * 0.5f, 0.0f);
			controller.slopeLimit = 80.0f;
			controller.stepOffset = clampMax(0.3f, controller.height + controller.radius * 2.0f);
			controller.enabled = true;
			controller.skinWidth = 0.01f;
		}
	}
	[MenuItem("检查/资源检测/检查选中对象的原点是否在模型底部", false, 100)]
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
	// 检查是否丢失材质引用
	[MenuItem("检查/资源检测/检查材质引用丢失", false, 101)]
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
	[MenuItem("检查/资源检测/检查材质贴图是否存在", false, 102)]
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
	[MenuItem("检查/资源检测/检查材质是否引用了shader未使用的贴图", false, 103)]
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
	[MenuItem("检查/资源检测/检查资源引用", false, 104)]
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
	// 检查热更与非热更资源是否存在相互引用(如有资源互相引用则为不合法)
	[MenuItem("检查/资源检测/检查热更与非热更资源是否相互引用", false, 105)]
	public static void checkRsourcesRefEachOther()
	{
		Debug.Log("开始检查热更与非热更资源相互引用");
		// 所有热更资源的GUID
		var allGameResourcesAssetGUID = new List<string>(getAllGUIDAndSpriteIDBySuffixInFilePath(FrameDefine.P_GAME_RESOURCES_PATH, ".meta", "所有热更资源").Keys);
		// 所有非热更资源的GUID
		var allResourcesAssetGUID = new List<string>(getAllGUIDAndSpriteIDBySuffixInFilePath(FrameDefine.P_RESOURCES_PATH, ".meta", "所有非热更资源").Keys);

		// 所有热更资源中所有带引用的文件的集合
		var refGameResourcesFilesDic = getAllRefrenceFileText(FrameDefine.P_GAME_RESOURCES_PATH);
		// 所有非热更资源中所有带引用的文件的集合
		var refResourcesFilesDic = getAllRefrenceFileText(FrameDefine.P_RESOURCES_PATH);
		// 错误引用资源的字典
		var errorRefAssetDic = new Dictionary<string, List<string>>();

		// 检测不合法引用
		doCheckRefGUIDInFilePath(errorRefAssetDic, refGameResourcesFilesDic, allResourcesAssetGUID);
		doCheckRefGUIDInFilePath(errorRefAssetDic, refResourcesFilesDic, allGameResourcesAssetGUID);

		// 输出定位丢失脚本引用的资源信息
		foreach (var lineData in errorRefAssetDic)
		{
			// 将丢失引用的资源中的丢失引用对象的列表存入列表中
			var missingRefObjectsList = new List<string>();
			setCheckRefObjectsList(missingRefObjectsList, lineData.Key, lineData.Value);
			var go = AssetDatabase.LoadAssetAtPath<GameObject>(lineData.Key);
			if (go != null)
			{
				Debug.LogError("有错误引用:" + go.name + "\n" + stringsToString(missingRefObjectsList, '\n'), go);
			}
			else
			{
				for (int i = 0; i < missingRefObjectsList.Count; ++i)
				{
					Debug.LogError("有错误引用:" + lineData.Key + "\n" + stringsToString(missingRefObjectsList, '\n'));
				}
			}
		}
		Debug.Log("结束检查热更与非热更资源相互引用");
	}
	[MenuItem("检查/资源检测/检查未引用的资源", false, 106)]
	public static void checkUnusedTexture()
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
	[MenuItem("检查/资源检测/检查图集引用", false, 107)]
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
				doCheckAtlasRefrence(path, getAllFileText(FrameDefine.F_GAME_RESOURCES_PATH + FrameDefine.LAYOUT_PREFAB + "/"));
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
				var allFileText = getAllFileText(FrameDefine.F_GAME_RESOURCES_PATH + FrameDefine.LAYOUT_PREFAB + "/");
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
	[MenuItem("检查/资源检测/检查图集中不存在的图片", false, 108)]
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
	[MenuItem("检查/资源检测/检查是否有相同文件", false, 109)]
	public static void checkSameAsset()
	{
		Debug.Log("开始检查是否有相同文件");
		// 路径下所有资源文件列表
		var resourcesFilesList = new List<string>();
		// 重复的资源的路径字典(key: MD5字符串, value: 相同资源的列表)
		var hasSameAssetsDic = new Dictionary<string, List<string>>();
		findFiles(FrameDefine.P_GAME_RESOURCES_PATH, resourcesFilesList);
		int fileCount = resourcesFilesList.Count;

		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("正在查找是否有相同资源", "进度: ", i + 1, fileCount);
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
			foreach (var filePath in element.Value)
			{
				// 有重复
				if (element.Value.Count > 1)
				{
					var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
					Debug.LogError("出现重复的资源,路径为: " + filePath, obj);
				}
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查是否有相同文件");
	}
	[MenuItem("检查/资源检测/检查重名文件", false, 110)]
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
					string path = fullPathToProjectPath(item0);
					Debug.LogError("文件命名冲突:" + fileList[i] + "\n" + item0, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
					break;
				}
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查重名文件");
	}
	[MenuItem("检查/资源检测/检查预设根节点是否带变换", false, 111)]
	public static void checkPrefabRootTransform()
	{
		Debug.Log("开始检查预设变换...");
		var fileList = new List<string>();
		findFiles(FrameDefine.F_GAME_RESOURCES_PATH, fileList, ".prefab");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查预设变换", "进度: ", i + 1, fileCount);
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPathToProjectPath(fileList[i]));
			bool valid = isVectorZero(prefab.transform.localPosition) &&
						isVectorZero(prefab.transform.localEulerAngles) &&
						isVectorEqual(prefab.transform.localScale, Vector3.one);
			if (!valid)
			{
				Debug.LogError("预设根节点变换错误:" + fileList[i], prefab);
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查预设变换");
	}
	[MenuItem("检查/资源检测/检查所有Prefab文件MeshCollider的模型Read-Write(全局查找)", false, 112)]
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
			var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
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
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(mesh));
				Debug.LogError(targetPrefab.name + "没有开启Read-Write Enable. Path: " + filePath + ", Mesh: " + mesh.name, go);
			}
			displayProgressBar("预设的模型是否开启Read-Write", "进度: ", i + 1, processCount);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("------结束检查预设的模型是否开启Read-Write,耗时: " + (DateTime.Now - startTime));
	}
	[MenuItem("检查/资源检测/检查所有场景的MeshCollider模型Read-Write(全局查找)", false, 113)]
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
					var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(collider.sharedMesh));
					Debug.LogError("MeshCollider的Mesh没有开启Read-Write: " + collider.sharedMesh.name, go);
				}
			}
		}
		EditorUtility.ClearProgressBar();
		// 恢复打开之前的场景
		EditorSceneManager.OpenScene(curScenePath, OpenSceneMode.Single);
		Debug.Log("------结束检查所有场景的模型是否开启Read-Write,耗时: " + (DateTime.Now - startTime));
	}
	[MenuItem("检查/资源检测/检查场景MeshCollider模型Read-Write(在当前打开的场景中查找)", false, 114)]
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
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(collider.sharedMesh));
				Debug.LogError("MeshCollider的Mesh没有开启Read-Write Enable: " + collider.sharedMesh.name, go);
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("------结束检查场景的Mesh是否开启Read-Write,耗时: " + (DateTime.Now - startTime));
	}
	[MenuItem("检查/资源检测/检查当前场景的Layer空对象", false, 115)]
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
	[MenuItem("检查/资源检测/检查资源中是否有代码文件引用丢失", false, 116)]
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
	[MenuItem("检查/资源检测/检查布局预制体的缩放值是否符合规范", false, 116)]
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
			var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
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
	[MenuItem("检查/资源检测/检查布局中是否有Image引用丢失", false, 117)]
	public static void checkLoseImageReferenceObject()
	{
		Debug.Log("开始检查是否有布局文件图片是否引用丢失...");
		var allSpriteIDMap = getAllGUIDAndSpriteIDBySuffixInFilePath(FrameDefine.P_GAME_RESOURCES_PATH, ".meta", "所有对应SpriteID");
		var allImageReferenceMap = getImageReferenceInfo(FrameDefine.P_LAYOUT_PATH, ".prefab", "检查Image的引用");
		var objectList = new Dictionary<string, GameObject>();
		var filePathList = new List<string>();
		//查找路径同时加载Prefab对象
		findFiles(FrameDefine.F_LAYOUT_PATH, filePathList, ".prefab");
		foreach (var path in filePathList)
		{
			objectList.Add(fullPathToProjectPath(path), AssetDatabase.LoadAssetAtPath<GameObject>(fullPathToProjectPath(path)));
		}
		//输入相应日志信息
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
					var prefabObj = objectList[item.Key];
					Debug.LogError("布局 " + prefabObj.name + "图片丢失了引用位于节点 " + outObj.mName, prefabObj);
				}
			}
		}
		Debug.Log("完成检查是否有布局文件图片是否引用丢失");
	}
	[MenuItem("检查/资源检测/检查是否使用了内置图片的UI节点", false, 118)]
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
	[MenuItem("检查/资源检测/检查资源文件名", false, 119)]
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
				Debug.LogError("文件名不能包含空格或斜杠，文件名：" + fileName, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(files[i]));
			}
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查/资源检测/检查所有UI布局中的Z值是否为0", false, 120)]
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
			var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
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
	[MenuItem("检查/资源检测/检查所有UI布局中根节点是否为不保持宽高比的ScaleAnchor", false, 121)]
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
			var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
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
	//--------------------------------------------------------------------------------------------------------------------------------
	[MenuItem("检查/一键检查代码规范", false, 200)]
	public static void doAllScriptCheck()
	{
		detectResetProperty();
		checkCodeEmptyLine();
		checkProtobufMsgOrder();
		checkDifferentNodeName();
		checkSingleCodeLineLength();
		checkPropertyName();
		checkFunctionOrder();
		checkComment();
		checkMemberVariableAssignmentValue();
		checkCommentStandard();
		checkSystemFunction();
		checkCommandName();
		checkCodeSeparateLineWidth();
	}
	[MenuItem("检查/代码检测/检查代码" + KEY_FUNCTION, false, 200)]
	public static void detectResetProperty()
	{
		Debug.Log("开始检查代码" + KEY_FUNCTION);
		// 遍历目录,存储所有文件名和对应文本内容
		var classInfoList = new Dictionary<string, ClassInfo>();
		saveFileInfo(classInfoList);
		// 获取Assembly集合
		Assembly assemly = null;
		Assembly[] assembly = System.AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assembly.Length; ++i)
		{
			// 获取工程
			if (assembly[i].GetName().Name == "Assembly-CSharp")
			{
				assemly = assembly[i];
				break;
			}
		}
		if (assemly == null)
		{
			logError("找不到指定的程序集");
			return;
		}

		// 不需要检测的基类
		List<Type> ignoreBaseClass = new List<Type>();
		ignoreBaseClass.Add(typeof(myUIObject));
		ignoreBaseClass.Add(typeof(FrameSystem));
		ignoreBaseClass.Add(typeof(LayoutScript));
		ignoreBaseClass.Add(typeof(WindowShader));
		ignoreBaseClass.Add(typeof(PooledWindow));
		ignoreBaseClass.Add(typeof(WindowItem));
		ignoreBaseClass.Add(typeof(OBJECT));
		ignoreBaseClass.Add(typeof(ExcelData));
		ignoreBaseClass.Add(typeof(SQLiteData));
		ignoreBaseClass.Add(typeof(SceneProcedure));
		ignoreBaseClass.Add(typeof(NetPacketTCPFrame));
		ignoreBaseClass.Add(typeof(SQLiteTable));
#if USE_ILRUNTIME
		ignoreBaseClass.Add(typeof(CrossBindingAdaptorType));
#endif
		ignoreBaseClass.AddRange(FrameDefineExtension.IGNORE_RESETPROPERTY_CLASS);
		// 获取到类型
		Type[] types = assemly.GetTypes();
		for (int i = 0; i < types.Length; ++i)
		{
			Type type = types[i];
			// 是否继承自需要忽略的基类
			bool isIgnoreClass = false;
			for (int j = 0; j < ignoreBaseClass.Count; ++j)
			{
				if (ignoreBaseClass[j].IsAssignableFrom(type))
				{
					isIgnoreClass = true;
					break;
				}
			}
			// 判断类是否继承自 IClassObject  
			if (isIgnoreClass || !typeof(ClassObject).IsAssignableFrom(type))
			{
				continue;
			}
			// 获取类成员变量
			MemberInfo[] memberInfo = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			var fieldMembers = new List<MemberInfo>();
			for (int k = 0; k < memberInfo.Length; ++k)
			{
				// 成员变量 筛选出类型为字段
				if (memberInfo[k].MemberType == MemberTypes.Field)
				{
					fieldMembers.Add(memberInfo[k]);
				}
			}
			if (fieldMembers.Count == 0)
			{
				continue;
			}
			string className = type.Name;
			// `表示是模板类
			int templateIndex = className.IndexOf('`');
			if (templateIndex >= 0)
			{
				className = className.Substring(0, templateIndex);
			}
			if (!classInfoList.TryGetValue(className, out ClassInfo info))
			{
				Debug.LogError("class:" + className + " 程序集中有此类,但是代码文件中找不到此类");
				continue;
			}
			// LayoutSystem中的文件不需要检测
			if (info.mFilePath.Contains("/" + FrameDefine.LAYOUT_SYSTEM + "/"))
			{
				continue;
			}
			// 判断类是否包含函数resetProperty()
			// BindingFlags.DeclaredOnly 仅考虑在提供的类型的层次结构级别上声明的成员。不考虑继承的成员
			MethodInfo methodInfo = type.GetMethod(KEY_FUNCTION, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
			if (methodInfo == null)
			{
				Debug.LogError("class:" + className + " 没有包含: " + KEY_FUNCTION + "()" + addFileLine(info.mFilePath, info.mFunctionLine));
				continue;
			}

			detectResetAll(className, fieldMembers, info);
		}
		Debug.Log("检查完毕");
	}
	[MenuItem("检查/代码检测/检查代码空行", false, 201)]
	public static void checkCodeEmptyLine()
	{
		Debug.Log("开始检查代码空行...");
		var fileList = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_PATH, fileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, fileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			string file = fileList[i];
			displayProgressBar("检查代码空行", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(file))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckEmptyLine(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查代码空行");
	}
	[MenuItem("检查/代码检测/检查代码空格", false, 202)]
	public static void checkCodeSpace()
	{
		Debug.Log("开始检查代码空格...");
		var fileList = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_PATH, fileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, fileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查代码空格", "进度: ", i + 1, fileCount);
			string file = fileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckSpace(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查代码空格");
	}
	[MenuItem("检查/代码检测/检查代码Protobuf消息字段顺序", false, 203)]
	public static void checkProtobufMsgOrder()
	{
		Debug.Log("开始检查Protobuf的消息字段顺序...");
		var fileList = new List<string>();
		findFiles(FrameDefine.F_ASSETS_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("消息字段顺序", "进度: ", i + 1, fileCount);
			string file = fileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckProtoMemberOrder(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查Protobuf的消息字段顺序");
	}
	[MenuItem("检查/代码检测/检查UI变量名", false, 204)]
	public static void checkDifferentNodeName()
	{
		Debug.Log("开始检查UI变量名与节点名不一致的代码...");
		// 缓存所有预制体的Transform
		var fileListPrefab = new List<string>();
		findFiles(FrameDefine.F_LAYOUT_PATH, fileListPrefab, ".prefab");
		findFiles(FrameDefine.F_RESOURCES_LAYOUT_PREFAB_PATH, fileListPrefab, ".prefab");
		var prefabChildTransform = new Dictionary<string, Transform[]>();
		int processCount = fileListPrefab.Count;
		for (int i = 0; i < processCount; ++i)
		{
			string filePath = fullPathToProjectPath(fileListPrefab[i]);
			var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
			var childTrans = targetPrefab.transform.GetComponentsInChildren<Transform>(true);
			string prefabName = removeStartString(removeSuffix(getFileName(filePath)), "UI");
			prefabChildTransform.Add(prefabName, childTrans);
		}

		// 读取cs文件
		var fileListCS = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_LAYOUT_SYSTEM_PATH, fileListCS, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, fileListCS, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, fileListCS, ".cs");
		int fileCount = fileListCS.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			string file = fileListCS[i];
			string fileNameNoSuffix = removeSuffix(getFileName(fullPathToProjectPath(file)));
			if (!startWith(fileNameNoSuffix, "Script") || isIgnoreFile(file, FrameDefineExtension.IGNORE_LAYOUT_SCRIPT))
			{
				continue;
			}
			string layoutName = removeStartString(fileNameNoSuffix, "Script");
			if (!prefabChildTransform.TryGetValue(layoutName, out Transform[] transforms))
			{
				Debug.LogError("脚本名为:" + layoutName + "的cs文件没找到相对应的预制体");
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			displayProgressBar("UI变量名匹配", "进度: ", i + 1, fileCount);
			doCheckDifferentNodeName(file, layoutName, transforms, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查UI变量名与节点名不一致的代码");
	}
	[MenuItem("检查/代码检测/检查单行代码长度", false, 205)]
	public static void checkSingleCodeLineLength()
	{
		Debug.Log("开始逐行测代码长度");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查代码行长度", "进度: ", +1, fileCount);
			string file = scriptFileList[i];
			// ILRuntime自动生成的代码，文件忽略列表和MyStringBuilder都不需要检测代码长度
			if (isIgnoreFile(file, FrameDefineExtension.IGNORE_CODE_WIDTH))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckSingleCheckCodeLineWidth(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查/代码检测/检查命名规范", false, 206)]
	public static void checkPropertyName()
	{
		Debug.Log("开始检查命名规范");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查命名规范", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameDefineExtension.IGNORE_VARIABLE_CHECK))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckScriptLineByLine(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查/代码检测/检查函数排版", false, 207)]
	public static void checkFunctionOrder()
	{
		Debug.Log("开始检查函数排版");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查函数排版", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckFunctionOrder(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查/代码检测/检查注释", false, 208)]
	public static void checkComment()
	{
		Debug.Log("开始检查注释");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查注释", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameDefineExtension.IGNORE_COMMENT))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckComment(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查/代码检测/检查成员变量赋值", false, 209)]
	public static void checkMemberVariableAssignmentValue()
	{
		Debug.Log("开始检查成员变量赋值");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查成员变量赋值", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameDefineExtension.IGNORE_CONSTRUCT_VALUE))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckScriptMemberVariableValueAssignment(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查/代码检测/检查注释后是否添加空格", false, 210)]
	public static void checkCommentStandard()
	{
		Debug.Log("开始检查注释后是否添加空格");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			string filePath = scriptFileList[i];
			displayProgressBar("检查注释后是否添加空格", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(filePath))
			{
				continue;
			}
			openTxtFileLines(filePath, out string[] lines);
			doCheckCommentStandard(filePath, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem("检查/代码检测/检查内置函数的调用", false, 211)]
	public static void checkSystemFunction()
	{
		Debug.Log("开始检查内置函数调用");
		var scriptFileList = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");

		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查UnityEngine", "进度: ", i, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameDefineExtension.IGNORE_SYSTEM_FUNCTION_CHECK))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckSystemFunction(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查/代码检测/检查命令命名规范", false, 212)]
	// 根据命名规范中的规则去检测命令的目录和文件名,类名是否正确
	public static void checkCommandName()
	{
		Debug.Log("开始检查检查命令的命名规范");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 加载程序集
		var assembly = getAssembly("Assembly-CSharp");
		var hotFixAssembly = loadHotFixAssembly();

		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			string filePath = scriptFileList[i];
			displayProgressBar("检查命令的命名规范", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(scriptFileList[i]))
			{
				continue;
			}
			openTxtFileLines(filePath, out string[] lines);
			doCheckCommandName(filePath, lines, assembly, hotFixAssembly);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem("检查/代码检测/代码分隔行长度", false, 213)]
	public static void checkCodeSeparateLineWidth()
	{
		Debug.Log("开始检查分隔行的宽度");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(FrameDefine.F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(FrameDefine.F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(FrameDefine.F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			string filePath = scriptFileList[i];
			displayProgressBar("检查分隔行的宽度", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(filePath))
			{
				continue;
			}
			openTxtFileLines(filePath, out string[] lines);
			doCheckCodeSeparateLineWidth(filePath, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	//---------------------------------------------------------------------------------------------------------------------------
	// 是否忽略该文件
	protected static bool isIgnoreFile(string filePath, IEnumerable<string> ignoreArr = null)
	{
		// ILRuntime自动生成的代码需要忽略
		foreach (var str in FrameDefineExtension.IGNORE_SCRIPTS_CHECK)
		{
			if (filePath.Contains(str))
			{
				return true;
			}
		}
		if (ignoreArr != null)
		{
			foreach (var element in ignoreArr)
			{
				if (filePath.Contains(element))
				{
					return true;
				}
			}
		}
		return false;
	}
	// 获取常量变量名称,如果这行不是常量返回值为空
	protected static string findConstVariableName(string codeLine)
	{
		// 因为常量定义行必须包含 const 关键字,所以不包含时返回空值
		if (!codeLine.Contains(" const "))
		{
			return null;
		}

		// 从等号前找到第一个字段命名字符,开始获取常量名,继续往前找,知道再找到一个非字段命名字符,截取出常量名
		int equalIndex = codeLine.IndexOf('=');
		if (equalIndex < 0)
		{
			return null;
		}
		return findLastFunctionString(codeLine, equalIndex, out _);
	}
	// 获取枚举类型名称,如果这行不是枚举类型则返回空
	protected static string findEnumVariableName(string codeLine)
	{
		string[] codeLis = split(codeLine, ' ', '\t', ':');
		if (codeLis == null)
		{
			return null;
		}
		for (int i = 0; i < codeLis.Length; ++i)
		{
			if (codeLis[i] == "enum")
			{
				if (i + 1 < codeLis.Length)
				{
					return codeLis[i + 1];
				}
				break;
			}
		}
		return null;
	}
	// 返回成员变量的名字 如果不是成员变量返回null
	protected static string findMemberVariableName(string codeLine)
	{
		// 移除注释
		codeLine = removeComment(codeLine);

		// 移除;
		int semiIndex = codeLine.IndexOf(';');
		if (semiIndex >= 0)
		{
			codeLine = codeLine.Remove(semiIndex);
		}

		// 移除=以及后面所有的字符
		int euqalIndex = codeLine.IndexOf('=');
		if (euqalIndex >= 0)
		{
			codeLine = codeLine.Remove(euqalIndex);
		}

		// 从后往前把所有的空格和制表符移除
		codeLine = removeEndEmpty(codeLine);

		// 移除模板参数
		codeLine = removeFirstBetweenPairChars(codeLine, '<', '>', out _, out _);

		// 先根据空格分割字符串
		string[] elements = split(codeLine, ' ', '\t');
		if (elements == null || elements.Length < 2)
		{
			return null;
		}
		List<string> elementList = new List<string>(elements);
		// 移除开始的public,protected,private,static等修饰符
		bool hasPermission = false;
		while (elementList.Count > 0)
		{
			string firstString = elementList[0];
			if (firstString == "public" || firstString == "protected" || firstString == "private")
			{
				hasPermission = true;
			}
			if (firstString == "public" ||
				firstString == "protected" ||
				firstString == "private" ||
				firstString == "static" ||
				firstString == "const" ||
				firstString == "volatile" ||
				firstString == "readonly")
			{
				elementList.RemoveAt(0);
			}
			else
			{
				break;
			}
		}
		// 成员变量需要有访问权限设置
		if (!hasPermission)
		{
			return null;
		}
		// 剩下的字符串应该是一个类型和变量名,所以需要判断出除了最后一个元素以外,其他元素是否能组成一个类型,如果只有2个元素了,则就认为是变量定义
		string variableName = elementList[elementList.Count - 1];
		if (elementList.Count != 2)
		{
			return null;
		}
		// 如果最终获取的不符合变量名的语法,则不是变量名
		if (!isFunctionName(variableName))
		{
			return null;
		}
		return variableName;
	}
	// 获取一行中的类名,如果这一行不是类定义行返回值为空
	protected static string findClassName(string codeLine)
	{
		int startIndex = -1;
		if (!isClassLine(codeLine))
		{
			startIndex = findFirstSubstr(codeLine, " struct ", 0, true);
		}
		else
		{
			startIndex = findFirstSubstr(codeLine, " class ", 0, true);
		}
		if (startIndex < 0)
		{
			return null;
		}
		int spaceIndex = codeLine.IndexOf(' ', startIndex);
		int colonIndex = codeLine.IndexOf(':', startIndex);
		// 两个符号均未找到
		if (spaceIndex < 0 && colonIndex < 0)
		{
			return null;
		}
		// 两个符号找到其中一个
		if (spaceIndex < 0 || colonIndex < 0)
		{
			return codeLine.Substring(startIndex, getMax(spaceIndex, colonIndex) - startIndex);
		}
		// 两个符号全部都找到
		return codeLine.Substring(startIndex, getMin(spaceIndex, colonIndex) - startIndex);
	}
	// 查找作用域(代码行数组, 类声明下标, 结束下标)
	protected static bool findRegionBody(string[] lines, int classNameIndex, out int endIndex)
	{
		// 未配对的大括号数量
		int num = 0;
		for (int i = classNameIndex; i < lines.Length; ++i)
		{
			foreach (var item in lines[i])
			{
				if (item == '{')
				{
					++num;
				}
				if (item == '}')
				{
					if (--num == 0)
					{
						endIndex = i;
						return true;
					}
				}
			}
		}
		endIndex = -1;
		return false;
	}
	// 检测注释标准
	protected static void doCheckCommentStandard(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 如果是文件流,网址行,移除干扰字符串
			int index = findFirstSubstr(lines[i], "://", 0, true);
			if (index >= 0)
			{
				lines[i] = lines[i].Substring(index);
			}
			// 注释下标
			int noteIndex = findFirstSubstr(lines[i], "//", 0, true);
			if (noteIndex < 0)
			{
				continue;
			}
			string noteStr = lines[i].Substring(noteIndex);
			// 超过10个'-'代表该行为分割行,忽略
			if (noteStr.Contains("----------"))
			{
				continue;
			}
			if (noteStr.Length > 0 && noteStr[0] != ' ')
			{
				Debug.LogError("注释双斜线后一位应该为空格" + addFileLine(filePath, i + 1));
				continue;
			}
			// 移除所有空格和制表符
			noteStr = removeAllEmpty(noteStr);
			if (noteStr.Length == 0)
			{
				Debug.LogError("注释后没有内容, 应当移除注释" + addFileLine(filePath, i + 1));
				continue;
			}
		}
	}
	// 逐行检测成员变量的赋值
	protected static void doCheckScriptMemberVariableValueAssignment(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 移除注释
			string line = lines[i];
			line = removeComment(line);
			// 查找类名,查找不到类名说明不是类声明行,跳过,检索下一行
			if (findClassName(line) == null)
			{
				continue;
			}
			// 查找类块
			if (!findRegionBody(lines, i, out int endIndex))
			{
				Debug.LogError("查找类体失败!!!" + addFileLine(filePath, i + 1));
				continue;
			}
			// 查找基类如果继承自MonoBehaviour就忽略
			if (findBaseClassName(line) == typeof(MonoBehaviour).Name)
			{
				for (int j = i; j <= endIndex; ++j)
				{
					lines[j] = EMPTY;
				}
				i = endIndex + 1;
				continue;
			}

			// 不符合忽略条件的在类块内进行检查
			for (int j = i + 1; j < endIndex; ++j)
			{
				string codeLine = lines[j];
				if (findMemberVariableName(codeLine) == null)
				{
					continue;
				}
				// 不检测常量与全局变量的赋值
				string[] codeList = split(codeLine, ' ');
				if (arrayContains(codeList, "static") || arrayContains(codeList, "const"))
				{
					continue;
				}
				// 移除前面所有制表符
				codeLine = removeStartEmpty(codeLine);
				// 说明变量有特性修饰
				if (codeLine[0] == '[')
				{
					codeLine = codeLine.Substring(codeLine.IndexOf(']') + 1);
				}
				if (codeLine.IndexOf('=') >= 0)
				{
					Debug.LogError("有成员变量在定义时被赋值: " + addFileLine(filePath, j + 1));
				}
			}
		}
	}
	// 根据名称获取程序集
	protected static Assembly getAssembly(string assemblyName)
	{
		// 获取Assembly集合
		Assembly[] assembly = System.AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assembly.Length; ++i)
		{
			// 获取工程
			if (assembly[i].GetName().Name == assemblyName)
			{
				return assembly[i];
			}
		}
		return null;
	}
	// 加载热更程序集
	protected static Assembly loadHotFixAssembly()
	{
		if (!isFileExist(FrameDefine.F_ASSET_BUNDLE_PATH + FrameDefine.ILR_FILE))
		{
			return null;
		}
		return Assembly.LoadFile(FrameDefine.F_ASSET_BUNDLE_PATH + FrameDefine.ILR_FILE);
	}
	protected static Type findClass(Assembly assembly, string className)
	{
		if (assembly == null)
		{
			return null;
		}
		// 获取到类型
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; ++i)
		{
			if (types[i].Name == className)
			{
				return types[i];
			}
		}
		return null;
	}
	// 检测命令命名规范
	protected static void doCheckCommandName(string filePath, string[] lines, Assembly csharpAssembly, Assembly hotfixAssembly)
	{
		if (!filePath.Contains("/CommandSystem/"))
		{
			return;
		}
		string folder = getFolderName(filePath);
		//处于CommandSystem文件夹下的脚本忽略
		if (folder == "CommandSystem")
		{
			return;
		}
		if (!folder.StartsWith("Cmd"))
		{
			checkScriptTip("该命令文件夹没有以Cmd开头: " + folder, filePath, 0);
			return;
		}
		if (folder != "CmdGlobal" && folder != "CmdWindow")
		{
			string receiverClass = removeStartString(folder, "Cmd");
			if (findClass(csharpAssembly, receiverClass) == null && findClass(hotfixAssembly, receiverClass) == null)
			{
				checkScriptTip("未找到命令接收者的类: " + folder, filePath, 0);
				return;
			}
		}
		if (!getFileName(filePath).StartsWith(folder))
		{
			checkScriptTip("该命令脚本没有以目录名开头", filePath, 0);
			return;
		}
		for (int i = 0; i < lines.Length; ++i)
		{
			string className = findClassName(lines[i]);
			// 该行代码不是类的命名行
			if (className == null)
			{
				continue;
			}
			int templateIndex = className.IndexOf('<');
			// 包含'<'符号说明是泛型类
			if (templateIndex > 0)
			{
				className = className.Substring(0, templateIndex);
			}
			// 判断类名是否与脚本文件名相同
			if (className != getFileNameNoSuffix(filePath, true))
			{
				checkScriptTip("该命令类名与脚本名不一致", filePath, i + 1);
			}
			break;
		}
	}
	// 获取一类声明行中的该类的父类的名字,如果这一行不是类声明行或不继承任何类返回值为空
	protected static string findBaseClassName(string codeLine)
	{
		if (!isClassLine(codeLine))
		{
			return null;
		}
		int colonIndex = codeLine.IndexOf(':');
		// 该类没有继承对象
		if (colonIndex < 0)
		{
			return null;
		}
		int startIndex = -1;
		int endIndex = -1;
		for (int i = colonIndex + 1; i < codeLine.Length; ++i)
		{
			if (codeLine[i] == ' ')
			{
				continue;
			}
			if (startIndex < 0)
			{
				startIndex = i;
				continue;
			}
			if (codeLine[i] == ' ' || codeLine[i] == ',')
			{
				endIndex = i;
				break;
			}
		}
		if (startIndex > 0 && endIndex < 0)
		{
			endIndex = codeLine.Length;
		}
		if (startIndex < 0 || endIndex < 0)
		{
			return null;
		}
		return codeLine.Substring(startIndex, endIndex - startIndex);
	}
	// 逐行检查脚本中的命名规范
	protected static void doCheckScriptLineByLine(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 移除注释
			string codeLine = removeComment(lines[i]);
			// 查找类名
			string className = findClassName(codeLine);
			if (className == null)
			{
				continue;
			}
			// 类名以Debug结尾且继承自MonoBehaviour的类, 需要忽略
			if (findBaseClassName(codeLine) == typeof(MonoBehaviour).Name && endWith(className, "Debug"))
			{
				continue;
			}
			if (isIgnoreFile(filePath, FrameDefineExtension.IGNORE_FILE_CHECK_FUNCTION))
			{
				return;
			}
			// 查找类块
			if (!findRegionBody(lines, i, out int endIndex))
			{
				Debug.LogError("查找类体失败!!!" + addFileLine(filePath, i + 1));
				continue;
			}

			// 忽略包含SCJsonHttp, SCJsonHttp, JsonUDPData的类,并将类块内容置空
			bool ignoreClass = false;
			foreach (var item in FrameDefineExtension.IGNORE_CHECK_CLASS)
			{
				// 包含忽略字符串
				if (className.Contains(item))
				{
					ignoreClass = true;
					break;
				}
			}
			// 类名内包含忽略字符串,将内容置空
			if (ignoreClass)
			{
				for (int j = i; j <= endIndex; ++j)
				{
					lines[j] = EMPTY;
				}
				i = endIndex + 1;
				continue;
			}
			// 不包含忽略字符串在类块内进行检查
			for (int j = i + 1; j < endIndex; ++j)
			{
				string line = lines[j];
				if (!line.Contains("public ") && !line.Contains("protected ") && !line.Contains("private "))
				{
					continue;
				}
				// 检查命名规范
				doCheckFunctionName(filePath, j, line, className);
				doCheckConstVariable(filePath, j, line);
				doCheckEnumVariable(filePath, j, line);
				doCheckNormalVariable(filePath, j, line, lines[getMin(j + 1, lines.Length - 1)]);
			}
		}
	}
	// 检查常量
	protected static void doCheckConstVariable(string filePath, int index, string codeLine)
	{
		string constVarName = findConstVariableName(codeLine);
		if (constVarName == null)
		{
			return;
		}
		if (!isUpperString(constVarName))
		{
			Debug.LogError("常量命名不规范" + addFileLine(filePath, index + 1));
		}
	}
	// 检查枚举
	protected static void doCheckEnumVariable(string filePath, int index, string codeLine)
	{
		string enumName = findEnumVariableName(codeLine);
		// 返回值为空说明这一行不是枚举声明行
		if (isEmpty(enumName))
		{
			return;
		}

		// 枚举必须全部为大写
		if (!isUpperString(enumName))
		{
			Debug.LogError("枚举必须全部是大写" + addFileLine(filePath, index + 1));
		}
	}
	// 检查函数命名规范(所属文件路径, 所处行数, 代码行内容, 所属类名)
	protected static void doCheckFunctionName(string filePath, int index, string codeLine, string className)
	{
		// 返回值为空说明这一行不是函数声明行
		if (!findFunctionName(codeLine, out bool isContructor, out string functionName))
		{
			return;
		}
		// 忽略构造函数
		if (isContructor)
		{
			return;
		}
		// 忽略指定的函数
		foreach(var item in FrameDefineExtension.IGNORE_CHECK_FUNCTION)
		{
			if (codeLine.Contains(item))
			{
				return;
			}
		}
		// 全大写的函数名可忽略
		if (isUpperString(functionName))
		{
			return;
		}
		if (!isLower(functionName[0]))
		{
			Debug.LogError("函数命名不规范: 需要以小写字母开头" + addFileLine(filePath, index + 1));
		}
	}
	// 检查普通变量
	protected static void doCheckNormalVariable(string filePath, int index, string codeLine, string nextLine)
	{
		string[] codeList = split(codeLine, ' ');
		if (arrayContains(codeList, "static") || arrayContains(codeList, "const"))
		{
			return;
		}
		string normalVariable = findMemberVariableName(codeLine);
		if (normalVariable == null)
		{
			return;
		}
		// 忽略属性变量
		if (nextLine.IndexOf('{') >= 0)
		{
			return;
		}
		// 变量名长度必须大于2并且首字母必须以m开头且m后的首字母要大写
		if (normalVariable.Length < 2)
		{
			Debug.LogError("变量命名不规范: 成员变量长度小于2" + addFileLine(filePath, index + 1));
			return;
		}
		if (isUpper(normalVariable[0]))
		{
			Debug.LogError("变量命名不规范: 成员变量以大写字母开头" + addFileLine(filePath, index + 1));
			return;
		}
		if (normalVariable[0] != 'm')
		{
			Debug.LogError("变量命名不规范: 成员变量没有以m开头" + addFileLine(filePath, index + 1));
			return;
		}
		if (!isUpper(normalVariable[1]))
		{
			Debug.LogError("变量命名不规范: 成员变量m前缀后字母应该大写" + addFileLine(filePath, index + 1));
			return;
		}
	}
	// 检查单行代码长度
	protected static void doCheckSingleCheckCodeLineWidth(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			// 设置忽略,忽略函数命名,函数命名行,有长字符串行,成员变量定义,委托定义
			if (findFunctionName(line, out _, out _) ||
				hasLongStr(line) ||
				findMemberVariableName(line) != null ||
				line.Contains(" delegate "))
			{
				continue;
			}
			if (generateCharWidth(line) > 140)
			{
				Debug.LogError("单行代码太长,超出了140个字符宽度" + addFileLine(filePath, i + 1));
				findMemberVariableName(line);
			}
		}
	}
	// 检查分隔代码行宽度
	protected static void doCheckCodeSeparateLineWidth(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i];
			if (!line.Contains("//----------"))
			{
				continue;
			}
			if (line != "	//------------------------------------------------------------------------------------------------------------------------------")
			{
				checkScriptTip("分隔行字符数应该为130", filePath, i + 1);
				continue;
			}
		}
	}
	// 检查当前代码行是否有长字符串
	protected static bool hasLongStr(string codeLine)
	{
		string tmpStr = "";
		bool startRecord = false;
		// 先根据双引号分割字符串
		foreach (var element in codeLine)
		{
			do
			{
				if (element != '\"')
				{
					continue;
				}
				if (!startRecord)
				{
					startRecord = true;
					continue;
				}
				if (generateCharWidth(tmpStr) > 10)
				{
					// 有长字符串 结束判断
					return true;
				}
				tmpStr = "";
				startRecord = false;
			} while (false);
			if (startRecord)
			{
				tmpStr += element;
			}
		}
		// 检测到最后没有长字符串
		return false;
	}
	// 设置在资源中丢失引用的对象列表
	protected static void setCheckRefObjectsList(List<string> missingRefObjectsList, string fileName, List<string> missingList)
	{
		openTxtFileLines(fileName, out string[] lines);
		for (int i = 0; i < lines.Length; ++i)
		{
			foreach (var guid in missingList)
			{
				if (lines[i].Contains(guid))
				{
					missingRefObjectsList.Add(findObjectNameWithScriptGUID(lines, i));
					break;
				}
			}
		}
	}
	// 根据对象中的GUID获取引用对应资源的对象的名字
	protected static string findObjectNameWithScriptGUID(string[] lines, int scriptLineIndex)
	{
		string refID = null;
		int refIDIndex = 0;
		for (int i = scriptLineIndex; i > 0; --i)
		{
			if (refID == null && lines[i].IndexOf('&') >= 0)
			{
				refID = lines[i].Substring(lines[i].IndexOf('&') + 1);
			}
			if (!isEmpty(refID) && lines[i].Contains("fileID: " + refID))
			{
				refIDIndex = i;
				break;
			}
		}
		for (int i = refIDIndex; i < scriptLineIndex; ++i)
		{
			if (lines[i].Contains("m_Name: "))
			{
				return lines[i].Substring(lines[i].IndexOf(": ") + 1);
			}
		}
		return null;
	}
	// 输出丢失资源引用信息
	protected static void debugMissingRefInformation(Dictionary<string, List<string>> missingRefAssetsList, string missingType)
	{
		// 输出定位丢失脚本引用的资源信息
		foreach (var lineData in missingRefAssetsList)
		{
			// 将丢失引用的资源中的丢失引用对象的列表存入列表中
			var missingRefObjectsList = new List<string>();
			setCheckRefObjectsList(missingRefObjectsList, lineData.Key, lineData.Value);
			string assetPath = lineData.Key;
			var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
			Debug.LogError("有" + missingType + "的引用丢失" + obj.name + "\n" + stringsToString(missingRefObjectsList, '\n'), obj);
			if (endWith(assetPath, ".unity"))
			{
				for (int i = 0; i < missingRefObjectsList.Count; ++i)
				{
					Debug.LogError("有" + missingType + "的引用丢失,Scene:" + assetPath + "\n" + stringsToString(missingRefObjectsList, '\n'));
				}
			}
		}
	}
	// 检查是否引用了该列表中的GUID,如果引用就加入到错误引用字典中(参数: 错误引用列表, 带有引用的文件信息字典, 要进行比对的GUID列表)
	protected static void doCheckRefGUIDInFilePath(Dictionary<string, List<string>> errorRefAssetDic,
												   Dictionary<string, FileGUIDLines> fileDic,
												   List<string> GUIDsList)
	{
		foreach (var fileData in fileDic)
		{
			FileGUIDLines projectFile = fileData.Value;
			foreach (var guid in projectFile.mContainGUIDLines)
			{
				if (!GUIDsList.Contains(guid))
				{
					continue;
				}
				if (!errorRefAssetDic.TryGetValue(fileData.Value.mProjectFileName, out List<string> errorRefGUIDList))
				{
					errorRefGUIDList = new List<string>();
					errorRefAssetDic.Add(projectFile.mProjectFileName, errorRefGUIDList);
				}
				errorRefGUIDList.Add(guid);
			}
		}
	}
	// 检查Protobuf的消息字段的顺序
	protected static void doCheckProtoMemberOrder(string file, string[] lines)
	{
		int realOrder = 0;
		bool findProtoContract = false;
		for (int i = 0; i < lines.Length - 1; ++i)
		{
			if (lines[i] == "[ProtoContract]")
			{
				findProtoContract = true;
				continue;
			}
			if (!findProtoContract)
			{
				continue;
			}
			if (lines[i].Contains("[ProtoMember("))
			{
				int startIndex = findFirstSubstr(lines[i], "[ProtoMember(", 0, true);
				int endIndex = findFirstSubstr(lines[i], ',', startIndex);
				if (SToI(lines[i].Substring(startIndex, endIndex - startIndex)) - 1 != realOrder++)
				{
					Debug.LogError("Protobuf的消息字段顺序检测:有不符合规定的字段顺序." + addFileLine(file, i + 1));
				}
			}
		}
	}
	// 检查空行
	protected static void doCheckEmptyLine(string file, string[] lines)
	{
		// 移除开始的空格和空行
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = removeStartEmpty(lines[i]);
		}

		// 函数名的上一行不能为空行,需要保留空白字符进行检测
		for (int i = 1; i < lines.Length; ++i)
		{
			if (findFunctionName(lines[i], out _, out _) && lines[i - 1].Length == 0)
			{
				Debug.LogError("函数名的上一行发现空行." + addFileLine(file, i));
			}
		}

		// 先去除所有行的空白字符
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = removeAllEmpty(lines[i]);
		}
		// 不能有两个连续空行
		for (int i = 0; i < lines.Length - 1; ++i)
		{
			if (lines[i].Length == 0 && lines[i + 1].Length == 0)
			{
				Debug.LogError("有连续两个空行." + addFileLine(file, i + 1));
			}
		}
		// 左大括号的下一行不能为空行
		for (int i = 1; i < lines.Length - 1; ++i)
		{
			if (lines[i] == "{" && lines[i + 1].Length == 0)
			{
				Debug.LogError("左大括号的下一行发现空行." + addFileLine(file, i + 2));
			}
		}
		// 右大括号的上一行不能为空行
		for (int i = 1; i < lines.Length; ++i)
		{
			if (lines[i] == "}" && lines[i - 1].Length == 0)
			{
				Debug.LogError("右大括号的上一行发现空行." + addFileLine(file, i));
			}
		}
		// 文件第一行不能为空行
		if (lines[0].Length == 0)
		{
			Debug.LogError("文件第一行发现空行." + addFileLine(file, 1));
		}
		// 文件最后一行不能为空行
		if (lines[lines.Length - 1].Length == 0)
		{
			Debug.LogError("文件最后一行发现空行." + addFileLine(file, lines.Length));
		}
	}
	// 检查空格
	protected static void doCheckSpace(string file, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i];
			// 当前字符是否在字符串中
			bool stringing = false;
			for (int j = 0; j < line.Length; ++j)
			{
				if (line[j] == '"')
				{
					stringing = !stringing;
					continue;
				}
				if (stringing)
				{
					continue;
				}
				// 运算符两边需要添加空格,如果遇到+-*/=,则需要判断后面的紧接的字符是否为=,因为这些符号只能跟=连接
				// 先判断两个字符的
				if (j < line.Length - 1 && isDoubleOperator(line[j], line[j + 1]))
				{
					// 连续两个/是注释符号,后面也需要加空格
					if (line[j] == '/' && line[j + 1] == '/')
					{
						int commentSpacePos = j + 2;
						// 如果是分隔行,则允许不带空格
						if (commentSpacePos < line.Length && line[commentSpacePos] != ' ' && line.IndexOf("--------------", commentSpacePos) < 0)
						{
							Debug.LogError("注释的双斜杠后面需要有空格" + addFileLine(file, i + 1));
						}
						// 如果已经检测到了注释,则不需要再继续检测了
						break;
					}

					int expectedFrontSpacePos = j - 1;
					int expectedBackSpacePos = j + 2;
					// 如果是++或--,则只能在一边加空格
					if (line[j] == '+' && line[j + 1] == '+' || line[j] == '-' && line[j + 1] == '-')
					{
						// 如果++前面有变量,则前面不需要加空格
						if (j > 0 && isLetter(line[j - 1]))
						{
							expectedFrontSpacePos = -1;
							// 如果后面已经有;了,则后面也不需要再添加空格
							if (j + 2 < line.Length && line[j + 2] == ';')
							{
								expectedBackSpacePos = -1;
							}
						}
						// 如果后面有变量,且前面没有任何非空字符串,则前面也不需要添加空格
						else
						{
							expectedBackSpacePos = -1;
							if (!hasNonEmptyCharInFront(line, j))
							{
								expectedFrontSpacePos = -1;
							}
						}
					}
					if (expectedBackSpacePos >= 0 && expectedBackSpacePos < line.Length && line[expectedBackSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "后面需要有空格" + addFileLine(file, i + 1));
					}
					if (expectedFrontSpacePos >= 0 && line[expectedFrontSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "前面需要有空格" + addFileLine(file, i + 1));
					}
					// 跳过下一个字符,进入下一次循环
					++j;
					continue;
				}

				// 只是一个单独的运算符
				if (isSingleOperator(line[j]))
				{
					// 需要特殊判断<>符号
					// 遇到<符号时,如果后面能找到一个>,则认为不是一个运算符
					if (line[j] == '<' && line.IndexOf('>', j) >= 0)
					{
						continue;
					}
					// 遇到>符号时,如果前面能找到一个<,则认为不是一个运算符
					if (line[j] == '>' && line.IndexOf('<', 0) >= 0 && line.IndexOf('<', 0) < j)
					{
						continue;
					}
					int expectedFrontSpacePos = j - 1;
					int expectedBackSpacePos = j + 1;
					if (expectedBackSpacePos >= 0 && expectedBackSpacePos < line.Length && line[expectedBackSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "后面需要有空格" + addFileLine(file, i + 1));
					}
					if (expectedFrontSpacePos >= 0 && line[expectedFrontSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "前面需要有空格" + addFileLine(file, i + 1));
					}
				}

				// 逗号后面需要添加空格
				if (line[j] == ',' && hasNonEmptyCharInBack(line, j + 1))
				{
					if (j + 1 < line.Length && line[j + 1] != ' ')
					{
						Debug.LogError("逗号后面需要有空格" + addFileLine(file, i + 1));
					}
				}
			}
		}
	}
	//检查UI变量名与节点名不一致的代码
	protected static void doCheckDifferentNodeName(string file, string fileName, Transform[] allChildTrans, string[] lines)
	{
		if (lines == null || lines.Length == 0)
		{
			Debug.LogError("fields 有问题 file：" + file);
			return;
		}
		if (allChildTrans == null || allChildTrans.Length == 0)
		{
			Debug.LogError("prefab 有问题 file：" + file);
			return;
		}

		// 用于过滤UI变量
		string myUGUI = " myUGUI";
		// 用于存储变量行
		var linesDic = new Dictionary<string, int>();
		bool finish = false;
		bool start = false;
		for (int i = 0; i < lines.Length; ++i)
		{
			// 遇到函数就可以停止遍历了
			if (lines[i].IndexOf('(') >= 0)
			{
				break;
			}
			if (lines[i].Contains(myUGUI))
			{
				if (finish)
				{
					Debug.LogError("检测UI变量名规范:  在布局:" + fileName + " 中请保持UI变量中不要混入其他变量." + addFileLine(file, i));
					return;
				}
				start = true;
				// 数组变量可以忽略
				if (lines[i].Contains("[]"))
				{
					continue;
				}
				string[] variableArr = split(lines[i], ' ');
				string variableStr = variableArr[variableArr.Length - 1];
				removeStartString(ref variableStr, "m");
				removeEndString(ref variableStr, ";");
				linesDic.Add(variableStr, i + 1);
			}
			else
			{
				if (start)
				{
					finish = true;
				}
			}
		}

		// 查看类的成员变量是与prefab节点名称匹配
		int beforeIndex = -1;
		foreach (var item in linesDic)
		{
			bool found = false;
			for (int i = 0; i < allChildTrans.Length; ++i)
			{
				if (item.Key != allChildTrans[i].name)
				{
					continue;
				}
				found = true;
				// 找到的下标比上一次找到的高
				if (i > beforeIndex)
				{
					beforeIndex = i;
					break;
				}
				if (i <= beforeIndex)
				{
					Debug.LogError("在布局:" + fileName + " 变量名:m" + item.Key + " 的顺序错了." + addFileLine(file, item.Value));
					return;
				}
			}
			if (!found)
			{
				Debug.LogError("在布局:" + fileName + " 变量名:m" + item.Key + " 没有找到." + addFileLine(file, item.Value));
				return;
			}
		}
	}
	protected static bool hasNonEmptyCharInFront(string str, int endIndex)
	{
		for (int i = 0; i < endIndex; ++i)
		{
			if (str[i] != '\t' && str[i] != ' ')
			{
				return true;
			}
		}
		return false;
	}
	protected static bool hasNonEmptyCharInBack(string str, int startIndex)
	{
		for (int i = startIndex; i < str.Length; ++i)
		{
			// 如果遇到注释则直接返回
			if (i + 1 < str.Length && str[i] == '/' && str[i + 1] == '/')
			{
				return false;
			}
			if (str[i] != '\t' && str[i] != ' ')
			{
				return true;
			}
		}
		return false;
	}
	// 是否为运算符
	protected static bool isSingleOperator(char c)
	{
		return c == '+' || c == '-' || c == '*' || c == '/' || c == '=' || c == '<' || c == '>';
	}
	// 两个连续的字符是否为合法的运算符组合
	protected static bool isDoubleOperator(char c0, char c1)
	{
		// 如果第二个字符是=,则第一个字符允许是以下字符
		if (c1 == '=')
		{
			return c0 == '+' || c0 == '-' || c0 == '*' || c0 == '/' || c0 == '=' || c0 == '<' || c0 == '>' || c0 == '|' || c0 == '&' || c0 == '!';
		}
		return c0 == '+' && c1 == '+' ||
				c0 == '-' && c1 == '-' ||
				c0 == '/' && c1 == '/' ||
				c0 == '|' && c1 == '|' ||
				c0 == '&' && c1 == '&' ||
				c0 == '=' && c1 == '>' ||
				c0 == '>' && c1 == '>' ||
				c0 == '>' && c1 == '>';
	}
	// 判断一行字符是不是类声明行
	protected static bool isClassLine(string codeLine)
	{
		return findFirstSubstr(codeLine, " class ") >= 0;
	}
	// 判断一行字符串是不是函数名声明行,如果是,则返回是否为构造函数,函数名
	protected static bool findFunctionName(string line, out bool isConstructor, out string functionName)
	{
		functionName = null;
		isConstructor = false;
		// 移除注释
		line = removeComment(line);
		// 消除所有的尖括号内的字符
		line = removeFirstBetweenPairChars(line, '<', '>', out _, out _);
		
		// 先根据空格分割字符串
		string[] elements = split(line, ' ', '\t');
		if (elements == null || elements.Length < 2)
		{
			return false;
		}

		// 移除可能存在的where约束
		List<string> elementList = new List<string>(elements);
		int whereIndex = elementList.IndexOf("where");
		if (whereIndex >= 0)
		{
			elementList.RemoveRange(whereIndex, elementList.Count - whereIndex);
		}

		// 移除开始的public,protected,private,static,virtual,override修饰符
		bool isAbstract = false;
		while (elementList.Count > 0)
		{
			string firstString = elementList[0];
			// 包含委托关键字则直接返回false
			if (firstString == "delegate")
			{
				return false;
			}
			if (firstString == "public" ||
				firstString == "protected" ||
				firstString == "private" ||
				firstString == "static" ||
				firstString == "virtual" ||
				firstString == "abstract" ||
				firstString == "new" ||
				firstString == "override")
			{
				if (firstString == "abstract")
				{
					isAbstract = true;
				}
				elementList.RemoveAt(0);
			}
			else
			{
				break;
			}
		}
		string str0 = elementList[0];
		if (str0 == "if" ||
			str0 == "while" ||
			str0 == "else" ||
			str0 == "foreach" ||
			str0 == "for" ||
			startWith(str0, "if(") ||
			startWith(str0, "for(") ||
			startWith(str0, "while(") ||
			startWith(str0, "foreach("))
		{
			return false;
		}

		// 然后将剩下的元素通过空格拼接在一起,还原出原始的字符串
		string newLine = stringsToString(elementList, ' ');
		// 函数名肯定会包含括号
		int leftBracketIndex = newLine.IndexOf('(');
		if (leftBracketIndex < 0)
		{
			return false;
		}
		// '('前最多有两个元素（返回值，函数名）
		List<string> functionElements = new List<string>();
		int lastElementIndex = -1;
		for (int i = leftBracketIndex; i >= 0; --i)
		{
			if (lastElementIndex < 0 && isFunctionNameChar(newLine[i]))
			{
				lastElementIndex = i;
				continue;
			}
			if (lastElementIndex >= 0)
			{
				if (!isFunctionNameChar(newLine[i]))
				{
					functionElements.Add(newLine.Substring(i + 1, lastElementIndex - i));
					lastElementIndex = -1;
				}
				else if (i == 0)
				{
					functionElements.Add(newLine.Substring(i, lastElementIndex + 1));
					lastElementIndex = -1;
				}
				if (functionElements.Count> 2)
				{
					return false;
				}
			}
		}
		isConstructor = functionElements.Count == 1;
		if (functionElements.Count > 0)
		{
			functionName = functionElements[0];
		}

		// 抽象函数没有定义,包含abstract关键字,括号
		if (isAbstract)
		{
			return newLine[newLine.Length - 1] == ';';
		}

		// 函数名与函数定义不在同一行的
		if (newLine.IndexOf('{') < 0 && newLine.IndexOf('}') < 0)
		{
			// 函数名肯定是以括号结尾
			return newLine[newLine.Length - 1] == ')';
		}
		// 函数名与函数定义在同一行
		else
		{
			// 函数定义需要包含大括号
			return newLine.IndexOf('{') >= 0;
		}
	}
	protected static void doCheckAtlasNotExistSprite(string path)
	{
		var notExistSprites = checkAtlasNotExistSprite(path);
		foreach (var item in notExistSprites)
		{
			Debug.Log("图集:" + path + "中的图片:" + item.Value + "不存在", AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
		}
	}
	protected static void doCheckAtlasRefrence(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		var refrenceList = new Dictionary<string, SpriteRefrenceInfo>();
		searchSpriteRefrence(path, refrenceList, allFileText);
		foreach (var item in refrenceList)
		{
			Debug.Log("图集:" + path + "被布局:" + item.Key + "所引用, sprite:" + item.Value.mSpriteName, item.Value.mObject);
		}
		Debug.Log("图集" + path + "被" + refrenceList.Count + "个布局引用");
	}
	protected static void doCheckUnusedTexture(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		var refrenceMaterialList = new Dictionary<string, UnityEngine.Object>();
		// 先查找引用该贴图的材质
		searchFileRefrence(path, false, refrenceMaterialList, allFileText);
		if (refrenceMaterialList.Count == 0)
		{
			Debug.Log("资源未引用:" + path, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
		}
	}
	protected static void doSearchRefrence(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		var refrenceList = new Dictionary<string, UnityEngine.Object>();
		string fileName = getFileName(path);
		Debug.Log("<<<<<<<开始查找" + fileName + "的引用.......");
		searchFileRefrence(path, false, refrenceList, allFileText);
		foreach (var item in refrenceList)
		{
			Debug.Log(item.Key, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.Key));
		}
		Debug.Log(">>>>>>>完成查找" + fileName + "的引用, 共有" + refrenceList.Count + "处引用");
	}
	[OnOpenAsset(1)]
	protected static bool OnOpenAsset(int instanceID, int line)
	{
		// 自定义函数，用来获取log中的stacktrace，定义在后面。
		string stack_trace = findStackTrace();
		// 通过stacktrace来定位是否是我们自定义的log，我的log中有特殊文字 "检测resetProperty()"
		if (isEmpty(stack_trace))
		{
			return false;
		}

		// 只有包含特定的关键字才会跳转到代码
		if (!stack_trace.Contains(CODE_LOCATE_KEYWORD))
		{
			return false;
		}

		// 打开代码文件的指定行
		string filePath = null;
		int fileLine = 0;
		string[] debugInfoLines = split(stack_trace, false, '\n');
		for (int i = 0; i < debugInfoLines.Length; ++i)
		{
			if (startWith(debugInfoLines[i], "File:"))
			{
				filePath = removeStartString(debugInfoLines[i], "File:");
			}
			else if (startWith(debugInfoLines[i], "Line:"))
			{
				fileLine = SToI(removeStartString(debugInfoLines[i], "Line:"));
			}
		}

		if (filePath == null)
		{
			return false;
		}
		// 如果以Asset开头打开Asset文件并定位到该行
		if (filePath.StartsWith(FrameDefine.P_ASSETS_PATH))
		{
			UnityEngine.Object codeObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object));
			if (codeObject == null || codeObject.GetInstanceID() == instanceID && fileLine == line)
			{
				return false;
			}
			AssetDatabase.OpenAsset(codeObject, fileLine);
		}
		else
		{
			// 如果以HotFix开头只打开文件不定位到行
			EditorUtility.OpenWithDefaultApp(FrameDefine.F_PROJECT_PATH + filePath);
		}
		return true;
	}
	// 确保路径为相对于Project的路径
	protected static string ensureProjectPath(string filePath)
	{
		if (!startWith(filePath, FrameDefine.P_ASSETS_PATH) || !startWith(filePath, FrameDefine.P_HOT_FIX_PATH))
		{
			// Assets资源路径
			if (filePath.Contains("/" + FrameDefine.P_ASSETS_PATH))
			{
				filePath = fullPathToProjectPath(filePath);
			}
			if (filePath.Contains("/" + FrameDefine.P_HOT_FIX_PATH))
			{
				// 转换HotFix中文件的绝对路径到相对路径
				filePath = FrameDefine.P_HOT_FIX_PATH + filePath.Substring(FrameDefine.F_HOT_FIX_PATH.Length);
			}
		}
		return filePath;
	}
	// 检查代码提示
	protected static void checkScriptTip(string tipInfo, string filePath, int lineNumber)
	{
		filePath = ensureProjectPath(filePath);
		// 非热更文件
		if (filePath.StartsWith(FrameDefine.P_ASSETS_PATH))
		{
			Debug.LogError(tipInfo + addFileLine(filePath, lineNumber), AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath));
		}
		else
		{
			Debug.LogError(tipInfo + addFileLine(filePath, lineNumber));
		}
	}
	protected static string addFileLine(string file, int line)
	{
		return "\nFile:" + ensureProjectPath(file) + "\nLine:" + line + "\n" + CODE_LOCATE_KEYWORD;
	}
	protected static void removeMetaFile(List<string> fileList)
	{
		for (int i = 0; i < fileList.Count; ++i)
		{
			if (endWith(fileList[i], ".meta"))
			{
				fileList.RemoveAt(i);
				--i;
			}
		}
	}
	protected static void roundRectTransformToInt(RectTransform rectTransform)
	{
		if (rectTransform == null)
		{
			return;
		}
		rectTransform.localPosition = round(rectTransform.localPosition);
		setRectSize(rectTransform, round(getRectSize(rectTransform)));
		int childCount = rectTransform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			roundRectTransformToInt(rectTransform.GetChild(i) as RectTransform);
		}
	}
	protected static bool detectResetAll(string className, List<MemberInfo> fieldMembers, ClassInfo classInfo)
	{
		int index = 0;
		bool find = false;
		int startIndex = -1;
		int endIndex = -1;
		bool hasOverride = false;

		for (int i = 0; i < classInfo.mLines.Count; ++i)
		{
			string line = classInfo.mLines[i];
			if (line.Contains("void " + KEY_FUNCTION + "()"))
			{
				hasOverride = line.Contains("override");
				find = true;
				classInfo.mFunctionLine += i;
			}
			if (!find)
			{
				continue;
			}
			if (line.IndexOf('{') >= 0)
			{
				if (index == 0)
				{
					startIndex = i;
				}
				++index;
			}
			if (line.IndexOf('}') >= 0)
			{
				--index;
				if (index == 0)
				{
					endIndex = i;
				}
			}
			if (startIndex >= 0 && endIndex >= 0)
			{
				break;
			}
		}

		if (!find)
		{
			logError("class:" + className + " 没有包含: " + KEY_FUNCTION + "()" + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}
		if (!hasOverride && className != "ClassObject")
		{
			logError("class:" + className + " 没有重写: " + KEY_FUNCTION + "()" + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}

		// 所有待重置的成员变量列表
		List<string> notResetMemberList = new List<string>();
		for (int i = 0; i < fieldMembers.Count; ++i)
		{
			notResetMemberList.Add(fieldMembers[i].Name);
		}

		List<string> resetFunctionLines = new List<string>();
		for (int i = 0; i < endIndex - startIndex + 1; ++i)
		{
			resetFunctionLines.Add(classInfo.mLines[startIndex + i]);
		}
		List<char> letters = new List<char>();
		for (int i = 0; i < 'z' - 'a' + 1; ++i)
		{
			letters.Add((char)('A' + i));
			letters.Add((char)('a' + i));
		}
		for (int i = 0; i < 10; ++i)
		{
			letters.Add((char)('0' + i));
		}
		letters.Add('_');
		char[] seperates = generateOtherASCII(letters.ToArray());
		for (int i = 0; i < resetFunctionLines.Count; ++i)
		{
			// 文本用分隔符拆分,判断其中是否有变量名,一行最多只允许出现一个成员变量
			string[] strList = split(resetFunctionLines[i], seperates);
			for (int j = 0; j < notResetMemberList.Count; ++j)
			{
				// 如果检测到已经重置了,则将其从待重置列表中移除
				if (arrayContains(strList, notResetMemberList[j]))
				{
					notResetMemberList.RemoveAt(j);
					break;
				}
			}
		}

		// 是否调用了基类的resetProperty
		bool callBaseReset = false;
		for (int i = 0; i < resetFunctionLines.Count; ++i)
		{
			if (resetFunctionLines[i].Contains("base." + KEY_FUNCTION + "();"))
			{
				callBaseReset = true;
				break;
			}
		}
		if (!callBaseReset && className != "ClassObject")
		{
			logError("class:" + className + " 没有调用基类resetProperty" + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}

		if (notResetMemberList.Count > 0)
		{
			string memberLines = "有如下成员未重置:\n";
			for (int i = 0; i < notResetMemberList.Count; ++i)
			{
				memberLines += notResetMemberList[i] + "\n";
			}
			logError("class:" + className + " 成员变量未能全部重置\n" + memberLines + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}
		return true;
	}
	protected static void saveFileInfo(Dictionary<string, ClassInfo> fileInfos)
	{
		List<string> fileList = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_PATH, fileList, ".cs");
		foreach (var item in fileList)
		{
			string[] fileLines = File.ReadAllLines(item);
			int classBeginIndex = -1;
			string nameSpace = EMPTY;
			for (int i = 0; i < fileLines.Length; ++i)
			{
				string line = fileLines[i];
				if (line.Contains("namespace "))
				{
					int startIndex = findFirstSubstr(line, "namespace ", 0, true);
					if (line.IndexOf('{') >= 0)
					{
						nameSpace = line.Substring(startIndex, line.IndexOf('{') - startIndex) + ".";
					}
					else
					{
						nameSpace = line.Substring(startIndex) + ".";
					}
				}
				if (line.Contains("public class") || line.Contains("public abstract class") || line.Contains("public partial class"))
				{
					if (classBeginIndex >= 0)
					{
						parseClass(fileInfos, nameSpace, fileLines, classBeginIndex, i - 1, item);
					}
					classBeginIndex = i;
				}
			}
			if (classBeginIndex >= 0)
			{
				parseClass(fileInfos, nameSpace, fileLines, classBeginIndex, fileLines.Length - 1, item);
			}
		}
	}
	protected static void parseClass(Dictionary<string, ClassInfo> fileInfos, string nameSpace, string[] fileLines, int startIndex, int endIndex, string path)
	{
		List<string> classLines = new List<string>();
		for (int i = 0; i < endIndex - startIndex + 1; ++i)
		{
			classLines.Add(removeAll(fileLines[i + startIndex], '\t'));
		}
		string headLine = fileLines[startIndex];
		int nameStartIndex = findFirstSubstr(headLine, "class ", 0, true);
		string className;
		int colonIndex = headLine.IndexOf(':');
		if (colonIndex >= 0)
		{
			className = removeAll(nameSpace + headLine.Substring(nameStartIndex, colonIndex - nameStartIndex), ' ');
		}
		else
		{
			className = removeAll(nameSpace + headLine.Substring(nameStartIndex), ' ');
		}
		// 模板类,则去除模板属性,只保留类名
		int templateIndex = className.IndexOf('<');
		if (templateIndex >= 0)
		{
			className = className.Remove(templateIndex, className.IndexOf('>') - templateIndex + 1);
		}
		// 因为有些类是内部类,所以仍然存在重名情况,需要排除
		if (!fileInfos.ContainsKey(className))
		{
			ClassInfo info = new ClassInfo();
			info.mLines.AddRange(classLines);
			info.mFilePath = fullPathToProjectPath(path);
			info.mFunctionLine = startIndex + 1;
			fileInfos.Add(className, info);
		}
	}
	protected static string findStackTrace()
	{
		// 找到UnityEditor.EditorWindow的assembly
		var assembly_unity_editor = Assembly.GetAssembly(typeof(EditorWindow));
		if (assembly_unity_editor == null)
		{
			return null;
		}

		// 找到类UnityEditor.ConsoleWindow
		var type_console_window = assembly_unity_editor.GetType("UnityEditor.ConsoleWindow");
		if (type_console_window == null)
		{
			return null;
		}
		// 找到UnityEditor.ConsoleWindow中的成员ms_ConsoleWindow
		var field_console_window = type_console_window.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
		if (field_console_window == null)
		{
			return null;
		}
		// 获取ms_ConsoleWindow的值
		var instance_console_window = field_console_window.GetValue(null);
		if (instance_console_window == null)
		{
			return null;
		}

		// 如果console窗口是焦点窗口的话，获取stacktrace信息
		if ((object)EditorWindow.focusedWindow == instance_console_window)
		{
			// 通过assembly获取类ListViewState
			var type_list_view_state = assembly_unity_editor.GetType("UnityEditor.ListViewState");
			if (type_list_view_state == null)
			{
				return null;
			}

			// 找到类UnityEditor.ConsoleWindow中的成员m_ListView
			var field_list_view = type_console_window.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field_list_view == null)
			{
				return null;
			}

			// 获取m_ListView的值
			var value_list_view = field_list_view.GetValue(instance_console_window);
			if (value_list_view == null)
			{
				return null;
			}

			// 找到类UnityEditor.ConsoleWindow中的成员m_ActiveText
			var field_active_text = type_console_window.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field_active_text == null)
			{
				return null;
			}

			// 获得m_ActiveText的值，就是我们需要的stacktrace
			return field_active_text.GetValue(instance_console_window).ToString();
		}
		return null;
	}
	protected static void doCheckFunctionOrder(string filePath, string[] lines)
	{
		bool checkingPublic = false;
		bool checkingProtected = false;
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			if (!findFunctionName(line, out _, out _))
			{
				continue;
			}

			string[] elements = split(line, true, ' ', '\t');
			string firstString = elements[0];
			if (firstString == "public")
			{
				if (checkingPublic)
				{
					continue;
				}
				if (!checkingPublic && !checkingProtected)
				{
					checkingPublic = true;
				}
				if (checkingProtected)
				{
					Debug.LogError("顺序不符" + addFileLine(filePath, i + 1));
					return;
				}
			}
			else if (firstString == "protected")
			{
				if (checkingPublic)
				{
					int checkLineNum = i - 1;
					if (lines[checkLineNum].Contains("//") && !lines[checkLineNum].Contains("----"))
					{
						checkLineNum -= 1;
					}
					if (!lines[checkLineNum].Contains("//----"))
					{
						Debug.LogError("请添加//----" + addFileLine(filePath, i));
						return;
					}
				}
				checkingProtected = true;
				checkingPublic = false;
			}
		}
	}
	protected static void doCheckComment(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 查找类名
			string className = findClassName(lines[i]);
			if (!isEmpty(className))
			{
				doCheckClassComment(filePath, lines, i, className);
				continue;
			}
			// 类名未找到的情况下查找枚举名
			string enumName = findEnumVariableName(lines[i]);
			if (!isEmpty(enumName))
			{
				doCheckEnumComment(filePath, lines, i, enumName);
				continue;
			}
		}
	}
	// 查看枚举的注释
	protected static void doCheckEnumComment(string filePath, string[] lines, int index, string enumName)
	{
		int lastIndex = index - 1;
		// 类名的上一行需要写对于该类的注释
		while (true)
		{
			string lastLine = removeAllEmpty(lines[lastIndex]);
			if (lastLine.Contains("//"))
			{
				break;
			}

			if (lastLine.IndexOf('}') >= 0 || lastIndex == 0 || lastLine.Length == 0)
			{
				Debug.LogError("枚举的上一行需要添加注释" + addFileLine(filePath, lastIndex + 1));
				break;
			}
			--lastIndex;
		}
		// 查找类块
		if (!findRegionBody(lines, index, out int endIndex))
		{
			Debug.LogError("查找枚举定义失败!!!" + addFileLine(filePath, index + 1));
			return;
		}
		// 检测枚举值的注释
		for (int j = index + 1; j < endIndex; ++j)
		{
			if (lines[j].IndexOf('{') < 0 && !lines[j].Contains("//"))
			{
				Debug.LogError("添加" + enumName + "." + lines[j] + "的注释!!!" + addFileLine(filePath, j + 1));
			}
			continue;
		}
	}
	protected static void doCheckClassComment(string filePath, string[] lines, int index, string className)
	{
		int lastIndex = index - 1;
		// 类名的上一行需要写对于该类的注释
		while (true)
		{
			string lastLine = removeAllEmpty(lines[lastIndex]);
			if (lastLine.Contains("//"))
			{
				break;
			}
			if (lastLine.IndexOf('}') >= 0 || lastIndex == 0 || lastLine.Length == 0)
			{
				Debug.LogError("类的上一行需要添加注释" + addFileLine(filePath, lastIndex + 1));
				break;
			}
			--lastIndex;
		}

		// 类成员变量的后面需要写该成员变量的注释,myUGUI的变量除外.GameBase,FrameBase的除外
		if (className == typeof(FrameBase).Name)
		{
			return;
		}
		// 查找类块
		if (!findRegionBody(lines, index, out int endIndex))
		{
			Debug.LogError("查找类体失败!!!" + addFileLine(filePath, index + 1));
			return;
		}
		for (int j = index + 1; j < endIndex; ++j)
		{
			// 成员变量
			string normalVarlable = findMemberVariableName(lines[j]);
			if (!isEmpty(normalVarlable) && !lines[j].Contains("myUGUI") && !lines[j].Contains("//"))
			{
				Debug.LogError("需要在成员变量的后面增加注释!!!" + addFileLine(filePath, j + 1));
			}
		}
	}
	protected static void doCheckSystemFunction(string filePath, string[] lines)
	{
		string[] vector3IgnoreList = new string[]
		{
			"right",
			"left",
			"up",
			"back",
			"forward",
			"one",
			"zero",
			"down"
		};
		for (int i = 0; i < lines.Length; ++i)
		{
			// 过滤特殊的类
			string className = findClassName(lines[i]);
			if (isEmpty(className))
			{
				continue;
			}
			// 查找类块
			if (!findRegionBody(lines, i, out int endIndex))
			{
				Debug.LogError("查找类体失败!!!" + addFileLine(filePath, i + 1));
				return;
			}
			// 查找UnityEngine.Debug
			if (findBaseClassName(lines[i]) == typeof(MonoBehaviour).Name ||
				className == typeof(BinaryUtility).Name ||
				className == typeof(CSharpUtility).Name ||
				className == typeof(FileUtility).Name ||
				className == typeof(FrameUtility).Name ||
				className == typeof(MathUtility).Name ||
				className == typeof(StringUtility).Name ||
				className == typeof(TimeUtility).Name ||
				className == typeof(UnityUtility).Name ||
#if USE_ILRUNTIME
				className == typeof(ILRSystem).Name ||
#endif
				className == typeof(WidgetUtility).Name)
			{
				i = endIndex + 1;
				continue;
			}
			for (int j = i + 1; j < endIndex + 1; ++j)
			{
				doCheckFunctionCall(lines[j], filePath, j + 1, typeof(Debug), "DrawLine");
				doCheckFunctionCall(lines[j], filePath, j + 1, typeof(Mathf));
				doCheckFunctionCall(lines[j], filePath, j + 1, typeof(Vector3), vector3IgnoreList);
			}
		}
	}
	protected static void doCheckFunctionCall(string lineString, string filePath, int index, Type classType, params string[] ignoreFuncionList)
	{
		// 移除注释
		string codeLine = removeComment(lineString);
		// 移除字符串
		codeLine = removeQuotedStrings(codeLine);
		// 函数名中不包含非常量的使用 即使是常量也可以忽略
		if (findFunctionName(lineString, out _, out _))
		{
			return;
		}

		string className = classType.ToString();
		// 一行字符串中会出现多个className
		while (true)
		{
			// 移除className之前的字符串。
			int firstIndex = findFirstSubstr(codeLine, className, 0, false);
			if (firstIndex < 0)
			{
				break;
			}

			// 过滤以className结尾的类 例如MyDebug
			if (firstIndex > 0 && isFunctionNameChar(codeLine[firstIndex - 1]))
			{
				// 移除这部分字符串
				codeLine = codeLine.Substring(firstIndex + className.Length);
				continue;
			}

			// 找到一个有效的方法
			string functionString = findFirstFunctionString(codeLine, firstIndex + className.Length, out int resultIndex);
			if (isEmpty(functionString))
			{
				break;
			}

			// 在方法名之前查找. 如果没找到，或者找到的下标在resultIndex之后，就返回
			int dotIndex = codeLine.IndexOf('.', firstIndex + className.Length);
			if (dotIndex == -1 || dotIndex > resultIndex)
			{
				break;
			}

			// 如果在忽略函数名集合中，就移除掉再继续找
			if (arrayContains(ignoreFuncionList, functionString))
			{
				codeLine = codeLine.Substring(resultIndex + functionString.Length);
				continue;
			}

			Debug.LogError("不允许使用" + className + "!!!" + addFileLine(filePath, index));
			break;
		}
	}
	// 在一个字符串中从前往后找,找到第一个符合函数名(或者类名,变量名,判断方式都是一样的)标准的字符串
	protected static string findFirstFunctionString(string code, int startIndex, out int resultIndex)
	{
		resultIndex = -1;
		for (int i = startIndex; i < code.Length; ++i)
		{
			if (resultIndex < 0)
			{
				if (isFunctionNameChar(code[i]))
				{
					resultIndex = i;
				}
			}
			else
			{
				if (!isFunctionNameChar(code[i]))
				{
					return code.Substring(resultIndex, i - resultIndex);
				}
				if (i == code.Length - 1)
				{
					return code.Substring(resultIndex);
				}
			}
		}
		return null;
	}
	// 在一个字符串中从后往前找,找到第一个符合函数名(或者类名,变量名,判断方式都是一样的)标准的字符串,endIndex表示从后往前找时开始的下标
	protected static string findLastFunctionString(string code, int endIndex, out int resultIndex)
	{
		int lastIndex = -1;
		resultIndex = -1;
		for (int i = endIndex; i >= 0; --i)
		{
			if (lastIndex < 0)
			{
				if (isFunctionNameChar(code[i]))
				{
					lastIndex = i;
				}
			}
			else
			{
				if (!isFunctionNameChar(code[i]))
				{
					resultIndex = i + 1;
					return code.Substring(resultIndex, lastIndex - resultIndex + 1);
				}
				if (i == 0)
				{
					resultIndex = 0;
					return code.Substring(resultIndex, lastIndex - resultIndex);
				}
			}
		}
		return null;
	}
	// 判断字符串是否是类名,函数名,变量名也都可以使用这个来判断
	public static bool isFunctionName(string str)
	{
		int length = str.Length;
		for (int i = 0; i < length; ++i)
		{
			if (!isFunctionNameChar(str[i]))
			{
				return false;
			}
		}
		return true;
	}
	// 查询c在类名,函数名,变量名命名中是否允许使用
	protected static bool isFunctionNameChar(char c)
	{
		return c == '_' || isNumeric(c) || isLetter(c);
	}
	// 移除注释,只考虑最后出现的双斜杠,如果注释的双斜杠后面还有双斜杠,则不能准确判断
	protected static string removeComment(string codeLine)
	{
		int index = codeLine.LastIndexOf("//");
		if (index >= 0)
		{
			// 判断双斜杠之前有多少个双引号,奇数个则表示双斜杠在字符串内
			string preString = codeLine.Remove(index);
			int quotCount = getCharCount(preString, '"');
			if ((quotCount & 1) == 0)
			{
				return preString;
			}
		}
		return codeLine;
	}
	// 移除字符串 一行代码可有多个字符串拼接组成
	protected static string removeQuotedStrings(string codeLine)
	{
		int lastIndex = -1;
		for (int i = codeLine.Length - 1; i >= 0; --i)
		{
			if (codeLine[i] != '"')
			{
				continue;
			}

			if (lastIndex == -1)
			{
				lastIndex = i;
			}
			else
			{
				codeLine = codeLine.Remove(i, lastIndex - i + 1);
				lastIndex = -1;
			}
		}
		return codeLine;
	}
	// 查询.prefab文件中m_Sprite是否引用了unity内置资源
	protected static void doCheckBuiltinUI(string filePath, string[] lines)
	{
		filePath = fullPathToProjectPath(filePath);
		int lineCount = lines.Length;
		for (int i = 0; i < lineCount; ++i)
		{
			string code = lines[i];
			if (!code.Contains("m_Sprite:") && !code.Contains("m_Texture:"))
			{
				continue;
			}

			int index = findFirstSubstr(code, " guid: ", 0, true);
			if (index < 0)
			{
				Debug.LogError("UI节点图片为空，filePath:" + filePath, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath));
				continue;
			}
			code = code.Substring(index);

			string guid = code.Substring(0, findFirstSubstr(code, ",", 0, false));
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			// Resources下还有内置的资源，所有需要指定文件夹
			if (!assetPath.Contains(FrameDefine.P_GAME_RESOURCES_PATH) &&
				!assetPath.Contains(FrameDefine.P_RESOURCES_ATLAS_PATH) &&
				!assetPath.Contains(FrameDefine.P_RESOURCES_TEXTURE_PATH))
			{
				Debug.LogError("UI节点图片引用了内置资源，filePath:" + filePath, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath));
			}
		}
	}
	// 查询.prefab文件中m_Font是否引用了unity内置资源
	protected static void doCheckBuiltinFont(string filePath, string[] lines)
	{
		filePath = fullPathToProjectPath(filePath);
		int lineCount = lines.Length;
		for (int i = 0; i < lineCount; ++i)
		{
			string code = lines[i];
			if (!code.Contains("m_Font:"))
			{
				continue;
			}

			int index = findFirstSubstr(code, " guid: ", 0, true);
			if (index < 0)
			{
				Debug.LogError("UI节点字体为空，filePath:" + filePath, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath));
				continue;
			}
			string guid = code.Substring(index, code.IndexOf(',', index) - index);
			Debug.Log("Font assetPath:" + AssetDatabase.GUIDToAssetPath(guid) + ", file:" + filePath, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath));
		}
	}
}