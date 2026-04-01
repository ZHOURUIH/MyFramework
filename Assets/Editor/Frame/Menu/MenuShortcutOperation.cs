using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using static MathUtility;
using static WidgetUtility;
using static FrameBaseUtility;
using static EditorCommonUtility;
using static FileUtility;
using static FrameDefine;
using static StringUtility;
using static FrameBaseDefine;
using UObject = UnityEngine.Object;

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
	[MenuItem(mMenuName + "将Text替换成TMPro", false, 42)]
	public static void textToTMPro()
	{
		if (Selection.gameObjects.isEmpty())
		{
			Debug.LogError("需要在Hierarchy中至少选中一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			var list0 = go.GetComponentsInChildren<Text>(true);
			foreach (Text item in list0)
			{
				doTextReplaceToTMPro(item);
			}
			var list1 = go.GetComponentsInChildren<InputField>(true);
			foreach (InputField item in list1)
			{
				doInputFieldReplaceToInputFieldTMP(item);
			}
			if (list0.count() > 0 || list1.count() > 0)
			{
				EditorUtility.SetDirty(go);
			}
		}
	}
	[MenuItem(mMenuName + "将TMPro替换成Text", false, 42)]
	public static void TMProToText()
	{
		if (Selection.gameObjects.isEmpty())
		{
			Debug.LogError("需要在Hierarchy中至少选中一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			var list0 = go.GetComponentsInChildren<TextMeshProUGUI>(true);
			foreach (TextMeshProUGUI item in list0)
			{
				doTMProReplaceToText(item);
			}
			var list1 = go.GetComponentsInChildren<TMP_InputField>(true);
			foreach (TMP_InputField item in list1)
			{
				doInputFieldTMPReplaceToInputField(item);
			}
			if (list0.count() > 0 || list1.count() > 0)
			{
				EditorUtility.SetDirty(go);
			}
		}
	}
	[MenuItem(mMenuName + "将Image替换为SpriteRenderer", false, 42)]
	public static void menuImageToSpriteRenderer()
	{
		if (Selection.gameObjects.isEmpty())
		{
			Debug.LogError("需要在Hierarchy中至少选中一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			var list0 = go.GetComponentsInChildren<Image>(true);
			foreach (Image item in list0)
			{
				imageToSpriteRenderer(item.gameObject);
			}
			if (list0.count() > 0)
			{
				EditorUtility.SetDirty(go);
			}
		}
	}
	[MenuItem(mMenuName + "将SpriteRenderer替换为Image", false, 42)]
	public static void menuSpriteRendererToImage()
	{
		if (Selection.gameObjects.isEmpty())
		{
			Debug.LogError("需要在Hierarchy中至少选中一个节点");
			return;
		}
		foreach (GameObject go in Selection.gameObjects)
		{
			var list0 = go.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer spriteRenderer in list0)
			{
				spriteRendererToImage(spriteRenderer.gameObject);
			}
			if (list0.count() > 0)
			{
				EditorUtility.SetDirty(go);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void doTextReplaceToTMPro(Text comText)
	{
		GameObject go = comText.gameObject;
		int fontSize = comText.fontSize;
		string text = comText.text;
		Color color = comText.color;
		TextAnchor alignment = comText.alignment;
		HorizontalWrapMode horiWrapMode = comText.horizontalOverflow;
		VerticalWrapMode vertWrapMode = comText.verticalOverflow;
		string fontPath = AssetDatabase.GetAssetPath(comText.font);
		UObject.DestroyImmediate(comText);
		if (go.TryGetComponent(out Outline comOutline))
		{
			Color outlineColor = comOutline.effectColor;
			Debug.LogWarning("需要添加字体描边,颜色:" + colorToRGBAString(outlineColor) + ", 宽度:" + comOutline.effectDistance + ", 文本:" + text, go);
			UObject.DestroyImmediate(comOutline);
		}
		var comTMP = getOrAddComponent<TextMeshProUGUI>(go);
		comTMP.text = text;
		comTMP.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(replaceSuffix(fontPath, ".asset"));
		comTMP.fontSize = fontSize;
		comTMP.color = color;
		if (horiWrapMode == HorizontalWrapMode.Wrap)
		{
#if UNITY_6000_0_OR_NEWER
			comTMP.textWrappingMode = TextWrappingModes.Normal;
#else
			comTMP.enableWordWrapping = true;
#endif
		}
		else if (horiWrapMode == HorizontalWrapMode.Overflow)
		{
#if UNITY_6000_0_OR_NEWER
			comTMP.textWrappingMode = TextWrappingModes.NoWrap;
#else
			comTMP.enableWordWrapping = false;
#endif
		}
		if (vertWrapMode == VerticalWrapMode.Overflow)
		{
			comTMP.overflowMode = TextOverflowModes.Overflow;
		}
		else if (vertWrapMode == VerticalWrapMode.Truncate)
		{
			comTMP.overflowMode = TextOverflowModes.Truncate;
		}
		switch (alignment)
		{
			case TextAnchor.UpperLeft: comTMP.alignment = TextAlignmentOptions.TopLeft; break;
			case TextAnchor.UpperCenter: comTMP.alignment = TextAlignmentOptions.Top; break;
			case TextAnchor.UpperRight: comTMP.alignment = TextAlignmentOptions.TopRight; break;
			case TextAnchor.MiddleLeft: comTMP.alignment = TextAlignmentOptions.Left; break;
			case TextAnchor.MiddleCenter: comTMP.alignment = TextAlignmentOptions.Center; break;
			case TextAnchor.MiddleRight: comTMP.alignment = TextAlignmentOptions.Right; break;
			case TextAnchor.LowerLeft: comTMP.alignment = TextAlignmentOptions.BottomLeft; break;
			case TextAnchor.LowerCenter: comTMP.alignment = TextAlignmentOptions.Bottom; break;
			case TextAnchor.LowerRight: comTMP.alignment = TextAlignmentOptions.BottomRight; break;
		}
	}
	protected static void doInputFieldReplaceToInputFieldTMP(InputField comInput)
	{
		string text = comInput.text;
		int characterLimit = comInput.characterLimit;
		InputField.ContentType contentType = comInput.contentType;
		InputField.LineType lineType = comInput.lineType;
		GameObject go = comInput.gameObject;
		UObject.DestroyImmediate(comInput);

		var comTMP = getOrAddComponent<TMP_InputField>(go);
		comTMP.text = text;
		comTMP.characterLimit = characterLimit;
		switch (contentType)
		{
			case InputField.ContentType.Standard: comTMP.contentType = TMP_InputField.ContentType.Standard; break;
			case InputField.ContentType.Autocorrected: comTMP.contentType = TMP_InputField.ContentType.Autocorrected; break;
			case InputField.ContentType.IntegerNumber: comTMP.contentType = TMP_InputField.ContentType.IntegerNumber; break;
			case InputField.ContentType.DecimalNumber: comTMP.contentType = TMP_InputField.ContentType.DecimalNumber; break;
			case InputField.ContentType.Alphanumeric: comTMP.contentType = TMP_InputField.ContentType.Alphanumeric; break;
			case InputField.ContentType.Name: comTMP.contentType = TMP_InputField.ContentType.Name; break;
			case InputField.ContentType.EmailAddress: comTMP.contentType = TMP_InputField.ContentType.EmailAddress; break;
			case InputField.ContentType.Password: comTMP.contentType = TMP_InputField.ContentType.Password; break;
			case InputField.ContentType.Pin: comTMP.contentType = TMP_InputField.ContentType.Pin; break;
			case InputField.ContentType.Custom: comTMP.contentType = TMP_InputField.ContentType.Custom; break;
		}
		switch (lineType)
		{
			case InputField.LineType.SingleLine: comTMP.lineType = TMP_InputField.LineType.SingleLine; break;
			case InputField.LineType.MultiLineSubmit: comTMP.lineType = TMP_InputField.LineType.MultiLineSubmit; break;
			case InputField.LineType.MultiLineNewline: comTMP.lineType = TMP_InputField.LineType.MultiLineNewline; break;
		}
		comTMP.textComponent = go.GetComponentInChildren<TextMeshProUGUI>();
		GameObject textArea = new("TextArea");
		textArea.transform.SetParent(go.transform);
		textArea.AddComponent<RectMask2D>();
		textArea.transform.localPosition = Vector3.zero;
		comTMP.textComponent.gameObject.transform.SetParent(textArea.transform);
		comTMP.textComponent.gameObject.transform.localPosition = Vector3.zero;
		comTMP.textViewport = textArea.transform as RectTransform;
		setRectSize(textArea.transform as RectTransform, (comTMP.transform as RectTransform).rect.size);
	}
	protected static void doTMProReplaceToText(TextMeshProUGUI comTextTMP)
	{
		GameObject go = comTextTMP.gameObject;
		float fontSize = comTextTMP.fontSize;
		string text = comTextTMP.text;
		Color color = comTextTMP.color;
		TextAlignmentOptions alignment = comTextTMP.alignment;
#if UNITY_6000_0_OR_NEWER
		TextWrappingModes horiWrapMode = comTextTMP.textWrappingMode;
#else
		bool enableWrapping = comTextTMP.enableWordWrapping;
#endif
		TextOverflowModes vertWrapMode = comTextTMP.overflowMode;
		string fontPath = AssetDatabase.GetAssetPath(comTextTMP.font);
		UObject.DestroyImmediate(comTextTMP);

		var comText = getOrAddComponent<Text>(go);
		comText.text = text;
		string fontFile = null;
		if (isFileExist(replaceSuffix(fontPath, ".ttc")))
		{
			fontFile = replaceSuffix(fontPath, ".ttc");
		}
		else if (isFileExist(projectPathToFullPath(replaceSuffix(fontPath, ".ttf"))))
		{
			fontFile = replaceSuffix(fontPath, ".ttf");
		}
		comText.font = AssetDatabase.LoadAssetAtPath<Font>(fontFile);

		comText.fontSize = (int)fontSize;
		comText.color = color;
#if UNITY_6000_0_OR_NEWER
		if (horiWrapMode == TextWrappingModes.Normal)
		{
			comText.horizontalOverflow = HorizontalWrapMode.Wrap;
		}
		else if (horiWrapMode == TextWrappingModes.NoWrap)
		{
			comText.horizontalOverflow = HorizontalWrapMode.Overflow;
		}
#else
		if (enableWrapping)
		{
			comText.horizontalOverflow = HorizontalWrapMode.Wrap;
		}
		else
		{
			comText.horizontalOverflow = HorizontalWrapMode.Overflow;
		}
#endif
		if (vertWrapMode == TextOverflowModes.Overflow)
		{
			comText.verticalOverflow = VerticalWrapMode.Overflow;
		}
		else if (vertWrapMode == TextOverflowModes.Truncate)
		{
			comText.verticalOverflow = VerticalWrapMode.Truncate;
		}
		switch (alignment)
		{
			case TextAlignmentOptions.TopLeft: comText.alignment = TextAnchor.UpperLeft; break;
			case TextAlignmentOptions.Top: comText.alignment = TextAnchor.UpperCenter; break;
			case TextAlignmentOptions.TopRight: comText.alignment = TextAnchor.UpperRight; break;
			case TextAlignmentOptions.Left: comText.alignment = TextAnchor.MiddleLeft; break;
			case TextAlignmentOptions.Center: comText.alignment = TextAnchor.MiddleCenter; break;
			case TextAlignmentOptions.Right: comText.alignment = TextAnchor.MiddleRight; break;
			case TextAlignmentOptions.BottomLeft: comText.alignment = TextAnchor.LowerLeft; break;
			case TextAlignmentOptions.Bottom: comText.alignment = TextAnchor.LowerCenter; break;
			case TextAlignmentOptions.BottomRight: comText.alignment = TextAnchor.LowerRight; break;
		}
	}
	protected static void doInputFieldTMPReplaceToInputField(TMP_InputField comInputTMP)
	{
		string text = comInputTMP.text;
		int characterLimit = comInputTMP.characterLimit;
		TMP_InputField.ContentType contentType = comInputTMP.contentType;
		TMP_InputField.LineType lineType = comInputTMP.lineType;
		GameObject go = comInputTMP.gameObject;
		UObject.DestroyImmediate(comInputTMP);

		var comText = getOrAddComponent<InputField>(go);
		comText.text = text;
		comText.characterLimit = characterLimit;
		switch (contentType)
		{
			case TMP_InputField.ContentType.Standard: comText.contentType = InputField.ContentType.Standard; break;
			case TMP_InputField.ContentType.Autocorrected: comText.contentType = InputField.ContentType.Autocorrected; break;
			case TMP_InputField.ContentType.IntegerNumber: comText.contentType = InputField.ContentType.IntegerNumber; break;
			case TMP_InputField.ContentType.DecimalNumber: comText.contentType = InputField.ContentType.DecimalNumber; break;
			case TMP_InputField.ContentType.Alphanumeric: comText.contentType = InputField.ContentType.Alphanumeric; break;
			case TMP_InputField.ContentType.Name: comText.contentType = InputField.ContentType.Name; break;
			case TMP_InputField.ContentType.EmailAddress: comText.contentType = InputField.ContentType.EmailAddress; break;
			case TMP_InputField.ContentType.Password: comText.contentType = InputField.ContentType.Password; break;
			case TMP_InputField.ContentType.Pin: comText.contentType = InputField.ContentType.Pin; break;
			case TMP_InputField.ContentType.Custom: comText.contentType = InputField.ContentType.Custom; break;
		}
		switch (lineType)
		{
			case TMP_InputField.LineType.SingleLine: comText.lineType = InputField.LineType.SingleLine; break;
			case TMP_InputField.LineType.MultiLineSubmit: comText.lineType = InputField.LineType.MultiLineSubmit; break;
			case TMP_InputField.LineType.MultiLineNewline: comText.lineType = InputField.LineType.MultiLineNewline; break;
		}
		comText.textComponent = go.GetComponentInChildren<Text>();
		UObject.DestroyImmediate(go.GetComponentInChildren<RectMask2D>().gameObject);
		comText.textComponent.transform.SetParent(comText.transform);
	}
}