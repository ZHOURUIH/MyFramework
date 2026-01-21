using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static MathUtility;
using static FrameBaseUtility;
using static EditorCommonUtility;
using static FileUtility;
using static FrameDefine;
using static FrameBaseDefine;

public class MenuShortcutOperation
{
	public const string mMenuName = "快捷操作/";
	[MenuItem(mMenuName + "启动游戏 _F5", false, 0)]
	public static void startGame()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(START_SCENE, OpenSceneMode.Single);
		}
		EditorApplication.isPlaying = !EditorApplication.isPlaying;
	}
	[MenuItem(mMenuName + "暂停游戏 _F6", false, 1)]
	public static void pauseGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.isPaused = !EditorApplication.isPaused;
		}
	}
	[MenuItem(mMenuName + "单帧执行 _F7", false, 2)]
	public static void stepGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.Step();
		}
	}
	[MenuItem(mMenuName + "打开初始场景 _F9", false, 3)]
	public static void jumpGameScene()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(START_SCENE);
		}
	}
	[MenuItem(mMenuName + "修复MeshCollider的模型引用", false, 33)]
	public static void fixMeshCollider()
	{
		GameObject[] selectObj = Selection.gameObjects;
		if (selectObj.isEmpty())
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}

		bool modified = false;
		foreach (GameObject item in selectObj)
		{
			modified = fixMeshOfMeshCollider(item) || modified;
		}
		if (modified)
		{
			EditorUtility.SetDirty(selectObj[0]);
		}
	}
	[MenuItem(mMenuName + "生成选中对象的角色控制器", false, 34)]
	public static void createGameObjectCollider()
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

			// 创建角色控制器并设置参数
			var controller = getOrAddComponent<CharacterController>(item);

			// 生成的高度不能小于半径的2倍,且不能大于4
			controller.height = bounds.size.y;
			controller.radius = clamp(bounds.size.x * 0.5f, 0.001f, controller.height * 0.5f);
			controller.center = new(0.0f, bounds.center.y - bounds.size.y * 0.5f + controller.height * 0.5f, 0.0f);
			controller.slopeLimit = 80.0f;
			controller.stepOffset = clampMax(0.3f, controller.height + controller.radius * 2.0f);
			controller.enabled = true;
			controller.skinWidth = 0.01f;
		}
	}
	[MenuItem(mMenuName + "打开PersistentDataPath", false, 37)]
	public static void openPersistentDataPath()
	{
		EditorUtility.RevealInFinder(F_PERSISTENT_DATA_PATH);
	}
	[MenuItem(mMenuName + "打开TemporaryCachePath", false, 38)]
	public static void openTemporaryCachePath()
	{
		EditorUtility.RevealInFinder(F_TEMPORARY_CACHE_PATH);
	}
	[MenuItem(mMenuName + "初始化版本号文件", false, 39)]
	public static void initVersionFile()
	{
		if (!isFileExist(F_ASSET_BUNDLE_PATH + VERSION))
		{
			writeTxtFile(F_ASSET_BUNDLE_PATH + VERSION, "0.0.0");
		}
	}
	[MenuItem(mMenuName + "初始化AOTGenericReferences", false, 40)]
	public static void initAOTGenericReference()
	{
		if (!isFileExist(F_ASSETS_PATH + "HybridCLRGenerate/AOTGenericReferences.cs"))
		{
			string str = "using System.Collections.Generic;\n";
			str += "\n";
			str += "public class AOTGenericReferences\n";
			str += "{\n";
			str += "\tpublic static List<string> PatchedAOTAssemblyList = new();\n";
			str += "}";
			writeTxtFile(F_ASSETS_PATH + "HybridCLRGenerate/AOTGenericReferences.cs", str);
			AssetDatabase.Refresh();
		}
	}
	[MenuItem(mMenuName + "dll解密", false, 41)]
	public static void decryptDllFile()
	{
		EditorWindow.GetWindow<DecryptDllWindow>(true, "解密dll", true).start();
	}
}