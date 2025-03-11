#if USE_TMP
using TMPro;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UObject = UnityEngine.Object;
using static WidgetUtility;
using static StringUtility;
using static MathUtility;
using static EditorCommonUtility;
using static FileUtility;
using static FrameDefine;
using static FrameDefineBase;

// 添加InitializeOnLoad使每次重新编译后都执行MenuShortcutOperation构造
[InitializeOnLoad]
public class MenuShortcutOperation
{
	private const string PENDING_RESTART_KEY = "Replay_PendingRestart";
	private static bool mIsEventRegistered;
	static MenuShortcutOperation()
	{
		// 每次脚本重载后检查未完成的重启操作
		AssemblyReloadEvents.afterAssemblyReload += CheckPendingRestart;
		CheckPendingRestart();
	}
	[MenuItem("快捷操作/启动游戏 _F5", false, 0)]
	public static void startGame()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(START_SCENE, OpenSceneMode.Single);
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
	[MenuItem("快捷操作/重新运行 _F8", false, 2)]
	public static void replayGame()
	{
		if (EditorApplication.isPlaying)
		{
			// 标记需要重启并退出播放模式
			EditorPrefs.SetBool(PENDING_RESTART_KEY, true);
			EditorApplication.isPlaying = false;
			if (!mIsEventRegistered)
			{
				EditorApplication.playModeStateChanged += OnPlayModeChanged;
				mIsEventRegistered = true;
			}
		}
		else
		{
			EditorApplication.isPlaying = true;
		}
	}
	[MenuItem("快捷操作/打开初始场景 _F9", false, 3)]
	public static void jumpGameSceme()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(START_SCENE);
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
		go.TryGetComponent<Transform>(out var trans);
		roundTransformToInt(trans);
	}
	[MenuItem("快捷操作/调整父节点使其完全包含所有子节点的范围 %E", false, 31)]
	public static void adjustParentRect()
	{
		var selection = Selection.activeGameObject.transform as RectTransform;
		if (selection == null)
		{
			Debug.LogError("节点必须有RectTransform组件");
			return;
		}
		if (selection.childCount == 0)
		{
			Debug.LogError("选择的节点没有子节点");
			return;
		}
		adjustRectTransformToContainsAllChildRect(selection);
	}
	[MenuItem("快捷操作/删除所有空文件夹", false, 32)]
	public static void deleteAllEmptyFolder()
	{
		deleteEmptyFolder(F_GAME_RESOURCES_PATH);
		AssetDatabase.Refresh();
	}
	[MenuItem("快捷操作/修复MeshCollider的模型引用", false, 33)]
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
			modified |= fixMeshOfMeshCollider(item);
		}
		if (modified)
		{
			EditorUtility.SetDirty(selectObj[0]);
		}
	}
	[MenuItem("快捷操作/生成选中对象的角色控制器", false, 34)]
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
			var renderers = item.GetComponentsInChildren<Renderer>();
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
			if (!item.TryGetComponent<CharacterController>(out var controller))
			{
				controller = item.AddComponent<CharacterController>();
			}

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
	[MenuItem("快捷操作/将缩放转换为位置和大小", false, 35)]
	public static void applyScaleToTransform()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		Vector3 scale = go.transform.localScale;
		var transforms = go.transform.GetComponentsInChildren<RectTransform>(true);
		foreach (RectTransform item in transforms)
		{
			if (item != go.transform)
			{
				item.localPosition = multiVector3(item.localPosition, scale);
			}
			setRectSize(item, multiVector2(item.rect.size, scale));
		}
		go.transform.localScale = Vector3.one;
	}
	[MenuItem("快捷操作/将Image替换为SpriteRenderer &Q", false, 36)]
	public static void imageToSpriteRenderer()
	{
		if (Selection.gameObjects == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			Sprite sprite = null;
			Material material = null;
			if (go.TryGetComponent<Image>(out var image))
			{
				sprite = image.sprite;
				material = image.material;
				UObject.DestroyImmediate(image);
			}
			if (go.TryGetComponent<CanvasRenderer>(out var canvasRenderer))
			{
				UObject.DestroyImmediate(canvasRenderer);
			}
			if (!go.TryGetComponent<SpriteRenderer>(out _))
			{
				var spriteRenderer = go.AddComponent<SpriteRenderer>();
				spriteRenderer.sprite = sprite;
				spriteRenderer.material = material;
			}
		}
	}
	[MenuItem("快捷操作/将字体设置为宋体 &W", false, 36)]
	public static void setFontToSimsun()
	{
		if (Selection.gameObjects == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			if (!go.TryGetComponent<Text>(out var text))
			{
				continue;
			}
			text.font = AssetDatabase.LoadAssetAtPath<Font>(P_GAME_RESOURCES_PATH + "Font/simsunFull.ttf");
			EditorUtility.SetDirty(go);
		}
	}
	[MenuItem("快捷操作/打开PersistentDataPath", false, 37)]
	public static void openPersistentDataPath()
	{
		EditorUtility.RevealInFinder(F_PERSISTENT_DATA_PATH);
	}
	[MenuItem("快捷操作/打开TemporaryCachePath", false, 38)]
	public static void openTemporaryCachePath()
	{
		EditorUtility.RevealInFinder(F_TEMPORARY_CACHE_PATH);
	}
	[MenuItem("快捷操作/初始化版本号文件", false, 39)]
	public static void initVersionFile()
	{
		if (!isFileExist(F_ASSET_BUNDLE_PATH + VERSION))
		{
			writeTxtFile(F_ASSET_BUNDLE_PATH + VERSION, "0.0.0");
		}
	}
	[MenuItem("快捷操作/初始化AOTGenericReferences", false, 40)]
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
	[MenuItem("快捷操作/dll解密", false, 41)]
	public static void decryptDllFile()
	{
		EditorWindow.GetWindow<DecryptDllWindow>(true, "解密dll", true).start();
	}
#if USE_TMP
	[MenuItem("快捷操作/将Text替换成TMPro", false, 42)]
	public static void textToTMPro()
	{
		GameObject go = Selection.gameObjects.get(0);
		if (go == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		var list = go.GetComponentsInChildren<Text>();
		bool dirty = list.count() > 0;
		foreach (Text item in list)
		{
			doTextReplaceToTMPro(item);
		}
		if (dirty)
		{
			EditorUtility.SetDirty(go);
		}
	}
#endif
//------------------------------------------------------------------------------------------------------------------------------
#if USE_TMP
	protected static void doTextReplaceToTMPro(Text comText)
	{
		GameObject go = comText.gameObject;
		int fontSize = comText.fontSize;
		string text = comText.text;
		Color color = comText.color;
		TextAnchor alignment = comText.alignment;
		string fontPath = AssetDatabase.GetAssetPath(comText.font);
		UObject.DestroyImmediate(comText);
		Color outlineColor = Color.white;
		if (go.TryGetComponent(out Outline comOutline))
		{
			outlineColor = comOutline.effectColor;
			Debug.LogWarning("需要添加字体描边,颜色:" + colorToRGBAString(outlineColor) + ", 宽度:" + comOutline.effectDistance + ", 文本:" + text, go);
			UObject.DestroyImmediate(comOutline);
		}
		if (!go.TryGetComponent(out TextMeshProUGUI comTMP))
		{
			comTMP = go.AddComponent<TextMeshProUGUI>();
		}
		comTMP.text = text;
		comTMP.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(replaceSuffix(fontPath, ".asset"));
		comTMP.fontSize = fontSize;
		comTMP.color = color;
		switch (alignment)
		{
			case TextAnchor.UpperLeft:		comTMP.alignment = TextAlignmentOptions.TopLeft; break;
			case TextAnchor.UpperCenter:	comTMP.alignment = TextAlignmentOptions.Top; break;
			case TextAnchor.UpperRight:		comTMP.alignment = TextAlignmentOptions.TopRight; break;
			case TextAnchor.MiddleLeft:		comTMP.alignment = TextAlignmentOptions.Left; break;
			case TextAnchor.MiddleCenter:	comTMP.alignment = TextAlignmentOptions.Center; break;
			case TextAnchor.MiddleRight:	comTMP.alignment = TextAlignmentOptions.Right; break;
			case TextAnchor.LowerLeft:		comTMP.alignment = TextAlignmentOptions.BottomLeft; break;
			case TextAnchor.LowerCenter:	comTMP.alignment = TextAlignmentOptions.Bottom; break;
			case TextAnchor.LowerRight:		comTMP.alignment = TextAlignmentOptions.BottomRight; break;
		}
	}
#endif
	private static void CheckPendingRestart()
	{
		if (EditorPrefs.GetBool(PENDING_RESTART_KEY, false))
		{
			EditorPrefs.DeleteKey(PENDING_RESTART_KEY);
			if (!EditorApplication.isPlaying)
			{
				Debug.Log("检测到未完成的重启请求，正在尝试重新触发...");
				replayGame();
			}
		}
	}
	private static void OnPlayModeChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingPlayMode)
		{
			// 当退出播放模式时检查编译状态
			if (EditorApplication.isCompiling)
			{
				// 如果正在编译，注册编译完成监听
				AssemblyReloadEvents.afterAssemblyReload += OnPostCompileRestart;
				Debug.Log("检测到代码修改，等待编译完成后重启...");
			}
		}
		else if (state == PlayModeStateChange.EnteredEditMode)
		{
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			mIsEventRegistered = false;
			if (!EditorApplication.isCompiling)
			{
				EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
			}
		}
	}
	private static void OnPostCompileRestart()
	{
		AssemblyReloadEvents.afterAssemblyReload -= OnPostCompileRestart;
		// 代码编译完成，自动重启播放
		EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
	}
}