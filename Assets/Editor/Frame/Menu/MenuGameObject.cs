#if USE_TMP
using TMPro;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using static EditorCommonUtility;
using static StringUtility;
using static FileUtility;
using static MathUtility;
using static WidgetUtility;
using static FrameBaseUtility;
using UObject = UnityEngine.Object;

public class MenuGameObject
{
	public const string mMenuName = "GameObject/";
	[MenuItem(mMenuName + "将缩放转换为位置和大小 &R", false, 35)]
	public static void applyScaleToTransform()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		Vector3 scale = go.transform.localScale;
		foreach (RectTransform item in go.transform.GetComponentsInChildren<RectTransform>(true))
		{
			if (item != go.transform)
			{
				item.localPosition = round(multiVector3(item.localPosition, scale));
			}
			setRectSize(item, round(multiVector2(item.rect.size, scale)));
		}
		foreach (Text item in go.transform.GetComponentsInChildren<Text>(true))
		{
			item.fontSize = (int)(item.fontSize * getMin(scale.x, scale.y));
		}
#if USE_TMP
		foreach (TextMeshProUGUI item in go.transform.GetComponentsInChildren<TextMeshProUGUI>(true))
		{
			item.fontSize = (int)(item.fontSize * getMin(scale.x, scale.y));
		}
#endif
		go.transform.localScale = Vector3.one;
		EditorUtility.SetDirty(go);
	}
	[MenuItem(mMenuName + "修改UI位置大小与父节点一致 &T", false, 35)]
	public static void setPosSizeSameAsParent()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			Debug.LogError("需要在Hierarchy中选中一个节点");
			return;
		}
		var thisTrans = go.transform as RectTransform;
		var parent = go.transform.parent as RectTransform;
		if (thisTrans == null || parent == null)
		{
			Debug.LogError("当前节点和父节点都需要是RectTransform");
			return;
		}
		thisTrans.localPosition = Vector3.zero;
		setRectSize(thisTrans, parent.rect.size);
		EditorUtility.SetDirty(go);
	}
	[MenuItem(mMenuName + "将Image替换为SpriteRenderer &Q", false, 36)]
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
			destroyComponent<CanvasRenderer>(go);
			if (getOrAddComponent(go, out SpriteRenderer spriteRenderer))
			{
				spriteRenderer.sprite = sprite;
				spriteRenderer.material = material;
			}
			EditorUtility.SetDirty(go);
		}
	}
	[MenuItem(mMenuName + "调整物体坐标为整数 _F1", false, 30)]
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
	[MenuItem(mMenuName + "调整节点位置和大小使其完全包含所有子节点的范围 %E", false, 31)]
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
#if USE_TMP
	[MenuItem("快捷操作/将Text替换成TMPro", false, 42)]
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
	[MenuItem("快捷操作/将TMPro替换成Text", false, 42)]
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
#endif
		}