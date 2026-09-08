using System;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

public abstract class FastUIRenderElementEditorBase : Editor
{
	protected bool mDebug;

	protected void beginInspector()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);
	}

	protected void drawVisible()
	{
		FastGUIInspectorUtility.drawBool<FastUIRenderElement>(
			targets,
			"Visible",
			element => element.getVisible(),
			(element, value) => element.setVisible(value),
			"Controls FastGUI rendering only. It does not disable the GameObject or invoke OnEnable/OnDisable.");
	}

	protected void drawCommonGraphicFields()
	{
		FastGUIInspectorUtility.drawColor<FastUIRenderElement>(
			targets,
			"Color",
			element => element.getColor(),
			(element, value) => element.setColor(value));
		FastGUIInspectorUtility.drawObject<FastUIRenderElement, Material>(
			targets,
			"Material",
			element => element.getMaterial(),
			(element, value) => element.setMaterial(value));
	}

	protected void drawDebug(Action extra = null)
	{
		FastGUIInspectorUtility.drawCanvasWarning(target as Component);
		if (!FastGUIInspectorUtility.drawDebugFoldout(ref mDebug))
		{
			return;
		}
		EditorGUI.indentLevel++;
		FastGUIInspectorUtility.drawCanvasBinding(target as Component);
		if (targets.Length == 1 && target is FastUIRenderElement element)
		{
			FastGUIInspectorUtility.drawGraphicRuntimeStatus(element);
			extra?.Invoke();
			FastGUIInspectorUtility.drawAllSerializedPropertiesReadOnly(serializedObject);
		}
		else
		{
			EditorGUILayout.HelpBox("Runtime debug data is available when a single object is selected.", MessageType.Info);
		}
		EditorGUI.indentLevel--;
	}
}

[CustomEditor(typeof(FastCanvas))]
[CanEditMultipleObjects]
public class FastCanvasEditor : Editor
{
	private bool mDebug;
	private bool mShowPerformance;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);
		drawDefaultMaterial();
		drawSortingLayer();
		FastGUIInspectorUtility.drawInt<FastCanvas>(targets, "Order in Layer", canvas => canvas.getSortingOrder(), (canvas, value) => canvas.setSortingOrder(value));
		FastGUIInspectorUtility.drawBool<FastCanvas>(
			targets,
			"Visible",
			canvas => canvas.getVisible(),
			(canvas, value) => canvas.setVisible(value),
			"Controls FastGUI rendering only. The GameObject remains active.");

		if (!FastGUIInspectorUtility.drawDebugFoldout(ref mDebug))
		{
			return;
		}

		EditorGUI.indentLevel++;
		mShowPerformance = EditorGUILayout.Foldout(mShowPerformance, "Performance Settings", true);
		if (mShowPerformance)
		{
			EditorGUI.indentLevel++;
			serializedObject.Update();
			FastGUIInspectorUtility.drawProperty(serializedObject, "mVisibilityIndexThreshold", "Visibility Index Threshold");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mPositionDeltaPackedQueueMinCount", "Position Delta Batch Threshold");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mSimpleTextCanvasBatchMinCount", "Simple Text Batch Threshold");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mBulkPositionDirtyEnvelopeEnabled", "Merge Position Dirty Range");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mBulkPositionSlotCacheEnabled", "Cache Position Slots");
			SerializedProperty originShift = FastGUIInspectorUtility.drawProperty(serializedObject, "mRenderOriginShiftEnabled", "Render Origin Shift");
			if (originShift != null && !originShift.hasMultipleDifferentValues && originShift.boolValue)
			{
				FastGUIInspectorUtility.drawProperty(serializedObject, "mRenderOriginShiftMinSavedVertexCount", "Min Saved Vertex Count");
			}
			serializedObject.ApplyModifiedProperties();
			EditorGUI.indentLevel--;
		}

		FastGUIInspectorUtility.drawAllSerializedPropertiesReadOnly(serializedObject);

		if (targets.Length == 1 && target is FastCanvas canvas)
		{
			drawCanvasStatus(canvas);
			drawCanvasActions(canvas);
		}
		else
		{
			EditorGUILayout.HelpBox("Runtime debug data is available when a single FastCanvas is selected.", MessageType.Info);
		}
		EditorGUI.indentLevel--;
	}

	private void drawDefaultMaterial()
	{
		SerializedProperty property = serializedObject.FindProperty("mDefaultMaterial");
		if (property == null)
		{
			return;
		}
		EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
		EditorGUI.BeginChangeCheck();
		Material current = property.objectReferenceValue as Material;
		Material next = EditorGUILayout.ObjectField(new GUIContent("Default Material", "The default material used by FastGUI graphics that do not specify their own material."), current, typeof(Material), false) as Material;
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (!changed)
		{
			return;
		}
		Undo.RecordObjects(targets, "Set Default Material");
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is FastCanvas canvas)
			{
				canvas.setDefaultMaterial(next);
				EditorUtility.SetDirty(canvas);
				PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
			}
		}
		serializedObject.Update();
		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}

	private void drawSortingLayer()
	{
		if (targets.Length <= 0 || targets[0] is not FastCanvas first)
		{
			return;
		}
		string current = first.getSortingLayer();
		bool mixed = false;
		for (int i = 1; i < targets.Length; ++i)
		{
			if (targets[i] is FastCanvas canvas && canvas.getSortingLayer() != current)
			{
				mixed = true;
				break;
			}
		}

		SortingLayer[] layers = SortingLayer.layers;
		string[] names = new string[layers.Length];
		int currentIndex = 0;
		for (int i = 0; i < layers.Length; ++i)
		{
			names[i] = layers[i].name;
			if (names[i] == current)
			{
				currentIndex = i;
			}
		}

		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		int nextIndex = EditorGUILayout.Popup(new GUIContent("Sorting Layer", "The Sorting Layer used by this FastCanvas renderer."), currentIndex, names);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (!changed || nextIndex < 0 || nextIndex >= names.Length)
		{
			return;
		}

		Undo.RecordObjects(targets, "Set Sorting Layer");
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is FastCanvas canvas)
			{
				canvas.setSortingLayer(names[nextIndex]);
				EditorUtility.SetDirty(canvas);
				PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
			}
		}
		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}

	private static void drawCanvasStatus(FastCanvas canvas)
	{
		FastGUIInspectorUtility.drawReadOnlyInt("Render Elements", canvas.getRenderElementCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Text Elements", canvas.getTextElementCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Batches", canvas.getBatchCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Transform Nodes", canvas.getTransformNodeCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Tombstones", canvas.getTombstoneCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Hidden Roots", canvas.getHiddenRootCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Clip Roots", canvas.getClipRootCount());
		FastGUIInspectorUtility.drawReadOnlyInt("SOA Groups", canvas.getSOAGroupCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Adjacent SOA Clusters", canvas.getSOAAdjacentMergeClusterCount());
		FastGUIInspectorUtility.drawReadOnlyText("Transform Backend", canvas.getTransformNodeBackendName());
		FastUIMeshRenderer renderer = canvas.getMeshRenderer();
		if (renderer != null)
		{
			FastGUIInspectorUtility.drawReadOnlyText("Vertex Upload", renderer.getVertexUploadBackend().ToString());
			FastGUIInspectorUtility.drawReadOnlyText("Index Upload", "MeshCPUCopy");
			FastGUIInspectorUtility.drawReadOnlyText("GPU Index Format", renderer.getGPUIndexFormat() + " / " + renderer.getGPUIndexStride() + "B");
			FastGUIInspectorUtility.drawReadOnlyInt("GPU Vertex Span", renderer.getVertexSpan());
		}
	}

	private static void drawCanvasActions(FastCanvas canvas)
	{
		EditorGUILayout.Space(3.0f);
		using (new EditorGUILayout.HorizontalScope())
		{
			if (GUILayout.Button("Refresh Bindings"))
			{
				canvas.refreshDescendantBindings();
				SceneView.RepaintAll();
			}
			if (GUILayout.Button("Flush Now"))
			{
				canvas.flushFrameNow();
				SceneView.RepaintAll();
			}
		}
		using (new EditorGUI.DisabledScope(canvas.getTombstoneCount() <= 0))
		{
			if (GUILayout.Button("Compact Tombstones"))
			{
				canvas.compactRenderOrderTombstones();
			}
		}
	}
}

[CustomEditor(typeof(FastRawImage))]
[CanEditMultipleObjects]
public class FastRawImageEditor : FastUIRenderElementEditorBase
{
	public override void OnInspectorGUI()
	{
		beginInspector();
		FastGUIInspectorUtility.drawObject<FastRawImage, Texture>(targets, "Texture", image => image.getTexture(), (image, value) => image.setTexture(value));
		FastGUIInspectorUtility.drawColor<FastUIRenderElement>(targets, "Color", image => image.getColor(), (image, value) => image.setColor(value));
		FastGUIInspectorUtility.drawObject<FastUIRenderElement, Material>(targets, "Material", image => image.getMaterial(), (image, value) => image.setMaterial(value));
		FastGUIInspectorUtility.drawRect<FastRawImage>(targets, "UV Rect", image => image.getUVRect(), (image, value) => image.setUVRect(value));
		drawVisible();

		if (targets.Length == 1 && target is FastRawImage rawImage && rawImage.getTexture() == null)
		{
			EditorGUILayout.HelpBox("No Texture assigned. The element will use a white texture and can be used as a solid color block.", MessageType.Info);
		}
		drawDebug();
	}
}

[CustomEditor(typeof(FastImage))]
[CanEditMultipleObjects]
public class FastImageEditor : FastUIRenderElementEditorBase
{
	private static readonly string[] SLICED_FILL_METHOD = { "Horizontal", "Vertical" };
	private static readonly string[] FILL_ORIGIN_HORIZONTAL = { "Left", "Right" };
	private static readonly string[] FILL_ORIGIN_VERTICAL = { "Bottom", "Top" };
	private static readonly string[] FILL_ORIGIN_RADIAL90 = { "Bottom Left", "Top Left", "Top Right", "Bottom Right" };
	private static readonly string[] FILL_ORIGIN_RADIAL180 = { "Bottom", "Left", "Top", "Right" };
	private static readonly string[] FILL_ORIGIN_RADIAL360 = { "Bottom", "Right", "Top", "Left" };

	public override void OnInspectorGUI()
	{
		beginInspector();
		FastGUIInspectorUtility.drawObject<FastImage, Sprite>(targets, "Source Image", image => image.getSprite(), (image, value) => image.setSprite(value));
		FastGUIInspectorUtility.drawColor<FastUIRenderElement>(targets, "Color", image => image.getColor(), (image, value) => image.setColor(value));
		FastGUIInspectorUtility.drawObject<FastUIRenderElement, Material>(targets, "Material", image => image.getMaterial(), (image, value) => image.setMaterial(value));
		FastGUIInspectorUtility.drawEnum<FastImage, FastUIImageType>(targets, "Image Type", image => image.getType(), (image, value) => image.setType(value));

		serializedObject.Update();
		SerializedProperty imageTypeProperty = serializedObject.FindProperty("mType");
		if (imageTypeProperty != null && !imageTypeProperty.hasMultipleDifferentValues)
		{
			drawTypeProperties((FastUIImageType)imageTypeProperty.enumValueIndex);
		}

		drawWarnings();
		drawVisible();
		drawNativeSizeButton();
		drawDebug();
	}

	private void drawTypeProperties(FastUIImageType imageType)
	{
		switch (imageType)
		{
			case FastUIImageType.Simple:
				break;
			case FastUIImageType.Sliced:
				FastGUIInspectorUtility.drawBool<FastImage>(
					targets,
					"Fill Center",
					image => image.getFillCenter(),
					(image, value) => image.setFillCenter(value));
				FastGUIInspectorUtility.drawFloat<FastImage>(
					targets,
					"Pixels Per Unit Multiplier",
					image => image.getPixelsPerUnitMultiplier(),
					(image, value) => image.setPixelsPerUnitMultiplier(value));
				drawSlicedFillMethod();
				drawSlicedFillOrigin();
				FastGUIInspectorUtility.drawSlider<FastImage>(
					targets,
					"Fill Amount",
					image => image.getFillAmount(),
					(image, value) => image.setFillAmount(value),
					0.0f,
					1.0f,
					"Controls how much of the sliced image is visible. The active region is re-sliced so both end borders remain intact.");
				break;
			case FastUIImageType.Tiled:
				FastGUIInspectorUtility.drawBool<FastImage>(targets, "Fill Center", image => image.getFillCenter(), (image, value) => image.setFillCenter(value));
				FastGUIInspectorUtility.drawFloat<FastImage>(targets, "Pixels Per Unit Multiplier", image => image.getPixelsPerUnitMultiplier(), (image, value) => image.setPixelsPerUnitMultiplier(value));
				break;
			case FastUIImageType.Filled:
				FastGUIInspectorUtility.drawEnum<FastImage, FastUIImageFillMethod>(targets, "Fill Method", image => image.getFillMethod(), (image, value) => image.setFillMethod(value));
				drawFillOrigin();
				FastGUIInspectorUtility.drawSlider<FastImage>(
					targets,
					"Fill Amount",
					image => image.getFillAmount(),
					(image, value) => image.setFillAmount(value),
					0.0f,
					1.0f);
				FastGUIInspectorUtility.drawBool<FastImage>(targets, "Clockwise", image => image.getFillClockwise(), (image, value) => image.setFillClockwise(value));
				break;
		}
	}

	private void drawSlicedFillMethod()
	{
		FastGUIInspectorUtility.drawPopup<FastImage>(
			targets,
			"Fill Method",
			image => image.getFillMethod() == FastUIImageFillMethod.Vertical ? 1 : 0,
			(image, value) => image.setFillMethod(value == 1 ? FastUIImageFillMethod.Vertical : FastUIImageFillMethod.Horizontal),
			SLICED_FILL_METHOD,
			"Sliced Fill supports linear Horizontal or Vertical filling while preserving the sliced borders.");
	}

	private void drawSlicedFillOrigin()
	{
		bool vertical = false;
		bool hasValue = false;
		bool mixed = false;
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not FastImage image)
			{
				continue;
			}
			bool currentVertical = image.getFillMethod() == FastUIImageFillMethod.Vertical;
			if (!hasValue)
			{
				vertical = currentVertical;
				hasValue = true;
			}
			else if (vertical != currentVertical)
			{
				mixed = true;
			}
		}
		if (!hasValue || mixed)
		{
			return;
		}
		string[] names = vertical ? FILL_ORIGIN_VERTICAL : FILL_ORIGIN_HORIZONTAL;
		FastGUIInspectorUtility.drawPopup<FastImage>(
			targets,
			"Fill Origin",
			image => Mathf.Clamp(image.getFillOrigin(), 0, 1),
			(image, value) => image.setFillOrigin(value),
			names,
			vertical
				? "Chooses whether Sliced Fill grows from Bottom or Top."
				: "Chooses whether Sliced Fill grows from Left or Right.");
	}

	private void drawFillOrigin()
	{
		serializedObject.Update();
		SerializedProperty fillMethodProperty = serializedObject.FindProperty("mFillMethod");
		if (fillMethodProperty == null || fillMethodProperty.hasMultipleDifferentValues || target is not FastImage image)
		{
			return;
		}
		string[] names = getFillOriginNames(image.getFillMethod());
		FastGUIInspectorUtility.drawPopup<FastImage>(targets, "Fill Origin", item => item.getFillOrigin(), (item, value) => item.setFillOrigin(value), names);
	}

	private void drawNativeSizeButton()
	{
		bool disabled = true;
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is FastImage image && image.getSprite() != null)
			{
				disabled = false;
				break;
			}
		}
		using (new EditorGUI.DisabledScope(disabled))
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(new GUIContent("Set Native Size", "Sets the RectTransform size to the native size of the Source Image."), GUILayout.Width(120.0f)))
				{
					for (int i = 0; i < targets.Length; ++i)
					{
						if (targets[i] is FastImage image && image.getSprite() != null)
						{
							Undo.RecordObject(image.getRectTransform(), "Set Native Size");
							image.setNativeSize();
							EditorUtility.SetDirty(image.getRectTransform());
							PrefabUtility.RecordPrefabInstancePropertyModifications(image.getRectTransform());
						}
					}
					SceneView.RepaintAll();
				}
			}
		}
	}

	private void drawWarnings()
	{
		if (targets.Length == 1 && target is FastImage image)
		{
			Sprite sprite = image.getSprite();
			if (image.getType() == FastUIImageType.Sliced && sprite != null && !hasSpriteBorder(sprite))
			{
				EditorGUILayout.HelpBox("This image doesn't have a border.", MessageType.Warning);
			}
			return;
		}

		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is FastImage selectedImage &&
				selectedImage.getType() == FastUIImageType.Sliced &&
				selectedImage.getSprite() != null &&
				!hasSpriteBorder(selectedImage.getSprite()))
			{
				EditorGUILayout.HelpBox("One or more selected images don't have a border.", MessageType.Warning);
				return;
			}
		}
	}

	private static bool hasSpriteBorder(Sprite sprite)
	{
		if (sprite == null)
		{
			return false;
		}
		Vector4 border = sprite.border;
		return border.sqrMagnitude > 0.000001f;
	}

	private static string[] getFillOriginNames(FastUIImageFillMethod method)
	{
		switch (method)
		{
			case FastUIImageFillMethod.Horizontal: return FILL_ORIGIN_HORIZONTAL;
			case FastUIImageFillMethod.Vertical: return FILL_ORIGIN_VERTICAL;
			case FastUIImageFillMethod.Radial90: return FILL_ORIGIN_RADIAL90;
			case FastUIImageFillMethod.Radial180: return FILL_ORIGIN_RADIAL180;
			default: return FILL_ORIGIN_RADIAL360;
		}
	}
}

[CustomEditor(typeof(FastText))]
[CanEditMultipleObjects]
public class FastTextEditor : FastUIRenderElementEditorBase
{
	private bool mShowSpacingOptions;
	private bool mShowExtraSettings;

	public override void OnInspectorGUI()
	{
		beginInspector();

		FastGUIInspectorUtility.drawTextArea<FastText>(
			targets,
			"Text",
			text => text.getText(),
			(text, value) => text.setText(value),
			44.0f,
			"The text displayed by this component.");

		EditorGUILayout.Space(2.0f);

		FastGUIInspectorUtility.drawObject<FastText, TMP_FontAsset>(
			targets,
			"Font Asset",
			text => text.getFont(),
			(text, value) => text.setFont(value),
			"The TMP Font Asset used to generate and render the text.");

		drawMaterialPreset();

		FastGUIInspectorUtility.drawFloat<FastText>(
			targets,
			"Font Size",
			text => text.getFontSize(),
			(text, value) => text.setFontSize(value),
			"The font size used to render the text.");

		FastGUIInspectorUtility.drawColor<FastUIRenderElement>(
			targets,
			"Vertex Color",
			text => text.getColor(),
			(text, value) => text.setColor(value),
			"The color applied to the generated text vertices.");

		EditorGUILayout.Space(3.0f);

		mShowSpacingOptions = EditorGUILayout.Foldout(
			mShowSpacingOptions,
			new GUIContent("Spacing Options", "Character and line spacing controls."),
			true);
		if (mShowSpacingOptions)
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawFloat<FastText>(
				targets,
				"Character Spacing",
				text => text.getCharacterSpacing(),
				(text, value) => text.setCharacterSpacing(value),
				"Additional horizontal spacing between characters.");
			FastGUIInspectorUtility.drawFloat<FastText>(
				targets,
				"Line Spacing",
				text => text.getLineSpacing(),
				(text, value) => text.setLineSpacing(value),
				"Additional vertical spacing between lines.");
			EditorGUI.indentLevel--;
		}

		drawAlignment();

		FastGUIInspectorUtility.drawEnum<FastText, FastUITextWrappingMode>(
			targets,
			"Text Wrapping Mode",
			text => text.getTextWrappingMode(),
			(text, value) => text.setTextWrappingMode(value),
			"No Wrap keeps text on the same line. Normal wraps text when the current line exceeds the RectTransform width.");

		FastGUIInspectorUtility.drawEnum<FastText, FastUITextOverflowMode>(
			targets,
			"Overflow",
			text => text.getOverflowMode(),
			(text, value) => text.setOverflowMode(value),
			"Overflow renders all laid-out text. Truncate stops rendering at the first logical text overflow, matching TMP-style container truncation without treating SDF material padding as overflow.");

		mShowExtraSettings = EditorGUILayout.Foldout(
			mShowExtraSettings,
			new GUIContent("Extra Settings", "Additional text rendering settings."),
			true);
		if (mShowExtraSettings)
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawBool<FastText>(
				targets,
				"Rich Text",
				text => text.getRichText(),
				(text, value) => text.setRichText(value),
				"Enables supported rich-text tags.");
			drawVisible();
			EditorGUI.indentLevel--;
		}

		if (targets.Length == 1 && target is FastText singleText && singleText.getFont() == null)
		{
			EditorGUILayout.HelpBox("A Font Asset is required to render text.", MessageType.Error);
		}

		drawDebug(drawTextDebug);
	}

	private void drawAlignment()
	{
		FastUITextHorizontalAlignment horizontal = FastUITextHorizontalAlignment.Left;
		FastUITextVerticalAlignment vertical = FastUITextVerticalAlignment.Top;
		bool hasValue = false;
		bool mixedHorizontal = false;
		bool mixedVertical = false;

		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not FastText text)
			{
				continue;
			}

			FastUITextHorizontalAlignment nextHorizontal = text.getHorizontalAlignment();
			FastUITextVerticalAlignment nextVertical = text.getVerticalAlignment();
			if (!hasValue)
			{
				horizontal = nextHorizontal;
				vertical = nextVertical;
				hasValue = true;
			}
			else
			{
				mixedHorizontal |= horizontal != nextHorizontal;
				mixedVertical |= vertical != nextVertical;
			}
		}

		if (!hasValue)
		{
			return;
		}

		const float ROW_GAP = 2.0f;
		float lineHeight = EditorGUIUtility.singleLineHeight;
		Rect fullRect = EditorGUILayout.GetControlRect(false, lineHeight * 2.0f + ROW_GAP);
		Rect controls = EditorGUI.PrefixLabel(
			fullRect,
			new GUIContent(
				"Alignment",
				"Horizontal alignment is shown on the first row and vertical alignment on the second row. Hover an individual button for its exact layout behavior."));

		float availableWidth = controls.width;
		float buttonWidth = Mathf.Floor(Mathf.Min(30.0f, availableWidth / 6.0f));
		float toolbarWidth = buttonWidth * 6.0f;

		Rect horizontalRect = new(controls.x, controls.y, toolbarWidth, lineHeight);
		Rect verticalRect = new(controls.x, controls.y + lineHeight + ROW_GAP, toolbarWidth, lineHeight);

		drawHorizontalAlignmentToolbar(horizontalRect, horizontal, mixedHorizontal);
		drawVerticalAlignmentToolbar(verticalRect, vertical, mixedVertical);
	}

	private void drawHorizontalAlignmentToolbar(
		Rect rect,
		FastUITextHorizontalAlignment current,
		bool mixed)
	{
		drawTMPAlignmentButton(
			rect, 0, 6,
			!mixed && current == FastUITextHorizontalAlignment.Left,
			TMP_UIStyleManager.alignLeft,
			"Left\nAligns each line to the left edge of the RectTransform. Line width does not change.",
			() => applyHorizontalAlignment(FastUITextHorizontalAlignment.Left));

		drawTMPAlignmentButton(
			rect, 1, 6,
			!mixed && current == FastUITextHorizontalAlignment.Center,
			TMP_UIStyleManager.alignCenter,
			"Center\nCenters each line using its text advance width.",
			() => applyHorizontalAlignment(FastUITextHorizontalAlignment.Center));

		drawTMPAlignmentButton(
			rect, 2, 6,
			!mixed && current == FastUITextHorizontalAlignment.Right,
			TMP_UIStyleManager.alignRight,
			"Right\nAligns each line to the right edge of the RectTransform.",
			() => applyHorizontalAlignment(FastUITextHorizontalAlignment.Right));

		drawTMPAlignmentButton(
			rect, 3, 6,
			!mixed && current == FastUITextHorizontalAlignment.Justified,
			TMP_UIStyleManager.alignJustified,
			"Justified\nExpands spacing so wrapped lines fill the RectTransform width. Word gaps are used first; lines without spaces fall back to character gaps. The final line of a paragraph is not stretched.",
			() => applyHorizontalAlignment(FastUITextHorizontalAlignment.Justified));

		drawTMPAlignmentButton(
			rect, 4, 6,
			!mixed && current == FastUITextHorizontalAlignment.Flush,
			TMP_UIStyleManager.alignFlush,
			"Flush\nExpands spacing so every line fills the RectTransform width, including the final line. Word gaps are used first; lines without spaces fall back to character gaps.",
			() => applyHorizontalAlignment(FastUITextHorizontalAlignment.Flush));

		drawTMPAlignmentButton(
			rect, 5, 6,
			!mixed && current == FastUITextHorizontalAlignment.Geometry,
			TMP_UIStyleManager.alignGeoCenter,
			"Geometry\nCenters the actual visible glyph geometry instead of the text advance width. Useful when side bearings make ordinary Center look visually off.",
			() => applyHorizontalAlignment(FastUITextHorizontalAlignment.Geometry));
	}

	private void drawVerticalAlignmentToolbar(
		Rect rect,
		FastUITextVerticalAlignment current,
		bool mixed)
	{
		drawTMPAlignmentButton(
			rect, 0, 6,
			!mixed && current == FastUITextVerticalAlignment.Top,
			TMP_UIStyleManager.alignTop,
			"Top\nPlaces the text block against the top edge of the RectTransform.",
			() => applyVerticalAlignment(FastUITextVerticalAlignment.Top));

		drawTMPAlignmentButton(
			rect, 1, 6,
			!mixed && current == FastUITextVerticalAlignment.Middle,
			TMP_UIStyleManager.alignMiddle,
			"Middle\nCenters the complete text block vertically inside the RectTransform.",
			() => applyVerticalAlignment(FastUITextVerticalAlignment.Middle));

		drawTMPAlignmentButton(
			rect, 2, 6,
			!mixed && current == FastUITextVerticalAlignment.Bottom,
			TMP_UIStyleManager.alignBottom,
			"Bottom\nPlaces the text block against the bottom edge of the RectTransform.",
			() => applyVerticalAlignment(FastUITextVerticalAlignment.Bottom));

		drawTMPAlignmentButton(
			rect, 3, 6,
			!mixed && current == FastUITextVerticalAlignment.Baseline,
			TMP_UIStyleManager.alignBaseline,
			"Baseline\nAligns the font baseline to the vertical center of the RectTransform. Useful when matching text that shares a common writing baseline.",
			() => applyVerticalAlignment(FastUITextVerticalAlignment.Baseline));

		drawTMPAlignmentButton(
			rect, 4, 6,
			!mixed && current == FastUITextVerticalAlignment.Midline,
			TMP_UIStyleManager.alignMidline,
			"Midline\nAligns the font mean line to the vertical center of the RectTransform using the TMP Font Asset metrics.",
			() => applyVerticalAlignment(FastUITextVerticalAlignment.Midline));

		drawTMPAlignmentButton(
			rect, 5, 6,
			!mixed && current == FastUITextVerticalAlignment.Capline,
			TMP_UIStyleManager.alignCapline,
			"Capline\nAligns the font cap line to the vertical center of the RectTransform using the TMP Font Asset metrics.",
			() => applyVerticalAlignment(FastUITextVerticalAlignment.Capline));
	}

	private static void drawTMPAlignmentButton(
		Rect groupRect,
		int index,
		int count,
		bool selected,
		Texture2D icon,
		string tooltip,
		Action onClick)
	{
		float buttonWidth = groupRect.width / count;
		Rect rect = new(
			groupRect.x + buttonWidth * index,
			groupRect.y,
			buttonWidth,
			groupRect.height);

		GUIStyle style =
			index == 0
				? TMP_UIStyleManager.alignmentButtonLeft
				: index == count - 1
					? TMP_UIStyleManager.alignmentButtonRight
					: TMP_UIStyleManager.alignmentButtonMid;

		GUIContent content = new(icon, tooltip);

		bool next = GUI.Toggle(
			rect,
			selected,
			content,
			style);

		if (next && !selected)
		{
			onClick?.Invoke();
		}
	}

	private void applyHorizontalAlignment(FastUITextHorizontalAlignment alignment)
	{
		Undo.RecordObjects(targets, "Set Horizontal Alignment");
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not FastText text)
			{
				continue;
			}
			text.setHorizontalAlignment(alignment);
			EditorUtility.SetDirty(text);
			PrefabUtility.RecordPrefabInstancePropertyModifications(text);
		}
		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}

	private void applyVerticalAlignment(FastUITextVerticalAlignment alignment)
	{
		Undo.RecordObjects(targets, "Set Vertical Alignment");
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not FastText text)
			{
				continue;
			}
			text.setVerticalAlignment(alignment);
			EditorUtility.SetDirty(text);
			PrefabUtility.RecordPrefabInstancePropertyModifications(text);
		}
		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}

	private void drawMaterialPreset()
	{
		FastText firstText = null;
		TMP_FontAsset commonFont = null;
		Material currentMaterial = null;
		bool mixedFont = false;
		bool mixedMaterial = false;

		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not FastText text)
			{
				continue;
			}

			TMP_FontAsset font = text.getFont();
			Material material = text.getMaterial();

			if (firstText == null)
			{
				firstText = text;
				commonFont = font;
				currentMaterial = material;
			}
			else
			{
				if (font != commonFont)
				{
					mixedFont = true;
				}
				if (material != currentMaterial)
				{
					mixedMaterial = true;
				}
			}
		}

		GUIContent label = new(
			"Material Preset",
			"Selects a TextMesh Pro material preset associated with the current Font Asset.");

		if (firstText == null)
		{
			return;
		}

		if (mixedFont || commonFont == null)
		{
			using (new EditorGUI.DisabledScope(true))
			{
				EditorGUI.showMixedValue = mixedFont || mixedMaterial;
				EditorGUILayout.ObjectField(label, currentMaterial, typeof(Material), false);
				EditorGUI.showMixedValue = false;
			}
			return;
		}

		List<Material> presets = findMaterialPresets(commonFont);
		if (currentMaterial != null && !presets.Contains(currentMaterial))
		{
			presets.Add(currentMaterial);
		}

		if (presets.Count == 0)
		{
			using (new EditorGUI.DisabledScope(true))
			{
				EditorGUILayout.ObjectField(label, null, typeof(Material), false);
			}
			return;
		}

		string[] names = new string[presets.Count];
		int selectedIndex = 0;
		for (int i = 0; i < presets.Count; ++i)
		{
			Material material = presets[i];
			names[i] = material == commonFont.material
				? material.name + " (Default)"
				: material.name;
			if (material == currentMaterial)
			{
				selectedIndex = i;
			}
		}

		EditorGUI.showMixedValue = mixedMaterial;
		EditorGUI.BeginChangeCheck();
		int nextIndex = EditorGUILayout.Popup(label, selectedIndex, names);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;

		if (!changed || nextIndex < 0 || nextIndex >= presets.Count)
		{
			return;
		}

		Material nextMaterial = presets[nextIndex];
		Undo.RecordObjects(targets, "Set Material Preset");
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not FastText text)
			{
				continue;
			}
			text.setMaterial(nextMaterial);
			EditorUtility.SetDirty(text);
			PrefabUtility.RecordPrefabInstancePropertyModifications(text);
		}
		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}

	private static List<Material> findMaterialPresets(TMP_FontAsset font)
	{
		List<Material> result = new();
		if (font == null)
		{
			return result;
		}

		Material defaultMaterial = font.material;
		if (defaultMaterial != null)
		{
			result.Add(defaultMaterial);
		}

		Texture atlasTexture = null;
		if (defaultMaterial != null && defaultMaterial.HasProperty("_MainTex"))
		{
			atlasTexture = defaultMaterial.GetTexture("_MainTex");
		}
		if (atlasTexture == null)
		{
			atlasTexture = font.atlasTexture;
		}
		if (atlasTexture == null)
		{
			return result;
		}

		string[] nameParts = font.name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		string searchName = nameParts.Length > 0 ? nameParts[0] : font.name;
		string[] materialGuids = AssetDatabase.FindAssets("t:Material " + searchName);
		for (int i = 0; i < materialGuids.Length; ++i)
		{
			string path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
			Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
			if (material == null || result.Contains(material) || !material.HasProperty("_MainTex"))
			{
				continue;
			}

			Texture materialAtlas = material.GetTexture("_MainTex");
			if (materialAtlas == atlasTexture)
			{
				result.Add(material);
			}
		}

		result.Sort((a, b) =>
		{
			if (a == defaultMaterial)
			{
				return -1;
			}
			if (b == defaultMaterial)
			{
				return 1;
			}
			return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
		});
		return result;
	}

	private void drawTextDebug()
	{
		if (target is not FastText text)
		{
			return;
		}
		drawMissingCharacter(text);
		FastGUIInspectorUtility.drawReadOnlyInt("Line Count", text.getLineCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Visible Characters", text.getVisibleGlyphCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Missing Characters", text.getMissingGlyphCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Fallback Characters", text.getFallbackGlyphCount());
		FastGUIInspectorUtility.drawReadOnlyInt("Atlas Runs", text.getRenderRunCount());
		FastGUIInspectorUtility.drawReadOnlyVector2("Preferred Size", new Vector2(text.getPreferredWidth(), text.getPreferredHeight()));
		if (text.getMissingGlyphCount() > 0)
		{
			EditorGUILayout.HelpBox("Some characters are missing from the primary font and its fallback fonts.", MessageType.Warning);
		}
	}

	private void drawMissingCharacter(FastText text)
	{
		serializedObject.Update();
		SerializedProperty property = serializedObject.FindProperty("mMissingCharacter");
		if (property == null)
		{
			return;
		}
		EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
		EditorGUI.BeginChangeCheck();
		long current = property.longValue;
		long next = EditorGUILayout.LongField(new GUIContent("Missing Character", "Unicode code point used when a requested character cannot be resolved by the configured font and fallback fonts."), current);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			uint unicode = (uint)Mathf.Clamp((float)next, 0.0f, 0x10FFFF);
			Undo.RecordObjects(targets, "Set Missing Character");
			for (int i = 0; i < targets.Length; ++i)
			{
				if (targets[i] is FastText changedText)
				{
					changedText.setMissingCharacter(unicode);
					EditorUtility.SetDirty(changedText);
					PrefabUtility.RecordPrefabInstancePropertyModifications(changedText);
				}
			}
			serializedObject.Update();
		}
		EditorGUILayout.LabelField("Unicode", "U+" + ((uint)Mathf.Clamp((float)next, 0.0f, 0x10FFFF)).ToString("X4"), EditorStyles.miniLabel);
	}
}

[CustomEditor(typeof(FastMask))]
[CanEditMultipleObjects]
public class FastMaskEditor : Editor
{
	private bool mDebug;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);
		FastGUIInspectorUtility.drawBool<FastMask>(targets, "Show Mask Graphic", mask => mask.getShowMaskGraphic(), (mask, value) => mask.setShowMaskGraphic(value));

		if (targets.Length == 1 && target is FastMask mask)
		{
			FastUIRenderElement graphic = mask.GetComponent<FastUIRenderElement>();
			if (graphic == null)
			{
				EditorGUILayout.HelpBox("FastMask must be on the same GameObject as a FastImage, FastRawImage, FastText, or another FastUIRenderElement.", MessageType.Error);
			}
			if (mask.getDepth() >= FastMaskUtility.MAX_MASK_DEPTH)
			{
				EditorGUILayout.HelpBox("Mask nesting has reached the maximum supported depth: " + FastMaskUtility.MAX_MASK_DEPTH + ".", MessageType.Error);
			}
		}

		if (FastGUIInspectorUtility.drawDebugFoldout(ref mDebug))
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawCanvasBinding(target as Component);
			FastGUIInspectorUtility.drawAllSerializedPropertiesReadOnly(serializedObject);
			if (targets.Length == 1 && target is FastMask debugMask)
			{
				FastGUIInspectorUtility.drawReadOnlyInt("Mask Depth", debugMask.getDepth());
			}
			EditorGUI.indentLevel--;
		}
	}
}

[CustomEditor(typeof(FastRectMask2D))]
[CanEditMultipleObjects]
public class FastRectMask2DEditor : Editor
{
	private bool mDebug;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);
		FastGUIInspectorUtility.drawVector4<FastRectMask2D>(
			targets,
			"Padding",
			mask => mask.getPadding(),
			(mask, value) => mask.setPadding(value),
			"Left / Bottom / Right / Top");

		if (targets.Length == 1 && target is FastRectMask2D mask && mask.getDepth() >= FastMaskUtility.MAX_MASK_DEPTH)
		{
			EditorGUILayout.HelpBox("Mask nesting has reached the maximum supported depth: " + FastMaskUtility.MAX_MASK_DEPTH + ".", MessageType.Error);
		}

		if (FastGUIInspectorUtility.drawDebugFoldout(ref mDebug))
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawCanvasBinding(target as Component);
			FastGUIInspectorUtility.drawAllSerializedPropertiesReadOnly(serializedObject);
			if (targets.Length == 1 && target is FastRectMask2D debugMask)
			{
				FastGUIInspectorUtility.drawReadOnlyInt("Mask Depth", debugMask.getDepth());
				FastGUIInspectorUtility.drawReadOnlyRect("Clip Rect", debugMask.getClipRect());
			}
			EditorGUI.indentLevel--;
		}
	}
}

[CustomEditor(typeof(FastUIVisibility))]
[CanEditMultipleObjects]
public class FastUIVisibilityEditor : Editor
{
	private bool mDebug;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);
		FastGUIInspectorUtility.drawBool<FastUIVisibility>(
			targets,
			"Visible",
			visibility => visibility.getVisible(),
			(visibility, value) => visibility.setVisible(value),
			"Controls FastGUI rendering for this subtree without changing GameObject active state.");

		if (FastGUIInspectorUtility.drawDebugFoldout(ref mDebug))
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawCanvasBinding(target as Component);
			FastGUIInspectorUtility.drawAllSerializedPropertiesReadOnly(serializedObject);
			EditorGUI.indentLevel--;
		}
	}
}

[CustomEditor(typeof(FastSOARenderGroup))]
[CanEditMultipleObjects]
public class FastSOARenderGroupEditor : Editor
{
	private bool mDebug;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);
		FastGUIInspectorUtility.drawEnum<FastSOARenderGroup, FastSOAStructureMode>(
			targets,
			"Structure Mode",
			group => group.getStructureMode(),
			(group, value) => group.setStructureMode(value));
		FastGUIInspectorUtility.drawBool<FastSOARenderGroup>(
			targets,
			"Allow Adjacent Group Merge",
			group => group.getAllowAdjacentGroupMerge(),
			(group, value) => group.setAllowAdjacentGroupMerge(value));

		if (!FastGUIInspectorUtility.drawDebugFoldout(ref mDebug))
		{
			return;
		}
		EditorGUI.indentLevel++;
		FastGUIInspectorUtility.drawCanvasBinding(target as Component);
		FastGUIInspectorUtility.drawAllSerializedPropertiesReadOnly(serializedObject);
		if (targets.Length == 1 && target is FastSOARenderGroup group)
		{
			FastGUIInspectorUtility.drawReadOnlyInt("Direct Children", group.transform.childCount);
			FastGUIInspectorUtility.drawReadOnlyBool("Last Build Valid", group.getLastBuildValid());
			if (!group.getLastBuildValid() && !string.IsNullOrEmpty(group.getLastBuildError()))
			{
				EditorGUILayout.HelpBox(group.getLastBuildError(), MessageType.Warning);
			}
			if (GUILayout.Button("Scan Scene for SOA Candidates"))
			{
				FastSOAEditorAdvisor.scanFromMenu();
			}
		}
		EditorGUI.indentLevel--;
	}
}

[CustomEditor(typeof(FastInputField))]
[CanEditMultipleObjects]
public class FastInputFieldEditor : Editor
{
	private bool mShowText = true;
	private bool mShowSettings = true;
	private bool mShowValidation = true;
	private bool mDebug;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);

		mShowText = EditorGUILayout.Foldout(mShowText, "Text", true);
		if (mShowText)
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawProperty(serializedObject, "mFont", "Font Asset");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mText", "Text");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mPlaceholder", "Placeholder");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mFontSize", "Font Size");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mTextColor", "Text Color");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mPlaceholderColor", "Placeholder Color");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mTextPadding", "Text Padding");
			EditorGUI.indentLevel--;
		}

		mShowSettings = EditorGUILayout.Foldout(mShowSettings, "Settings", true);
		if (mShowSettings)
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawProperty(serializedObject, "mInteractable", "Interactable");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mReadOnly", "Read Only");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mOnFocusSelectAll", "On Focus - Select All");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mLineType", "Line Type");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mCharacterLimit", "Character Limit");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mLineLimit", "Line Limit");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mCaretBlinkRate", "Caret Blink Rate");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mCaretWidth", "Caret Width");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mCaretColor", "Caret Color");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mSelectionColor", "Selection Color");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mRestoreOriginalTextOnEscape", "Restore On Escape");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mEnableTouchScreenKeyboard", "Touch Screen Keyboard");
			EditorGUI.indentLevel--;
		}

		mShowValidation = EditorGUILayout.Foldout(mShowValidation, "Content Type", true);
		if (mShowValidation)
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawProperty(serializedObject, "mContentType", "Content Type");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mInputType", "Input Type");
			SerializedProperty validation = FastGUIInspectorUtility.drawProperty(serializedObject, "mCharacterValidation", "Character Validation");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mKeyboardType", "Keyboard Type");
			FastGUIInspectorUtility.drawProperty(serializedObject, "mAsteriskChar", "Asterisk Char");
			if (validation != null && !validation.hasMultipleDifferentValues)
			{
				FastInputFieldCharacterValidation validationType = (FastInputFieldCharacterValidation)validation.enumValueIndex;
				if (validationType == FastInputFieldCharacterValidation.Regex)
				{
					FastGUIInspectorUtility.drawProperty(serializedObject, "mRegexValue", "Regex");
				}
				else if (validationType == FastInputFieldCharacterValidation.CustomValidator)
				{
					FastGUIInspectorUtility.drawProperty(serializedObject, "mInputValidator", "Input Validator");
				}
			}
			EditorGUI.indentLevel--;
		}

		// Property foldouts (for example the Vector4 Text Padding drawer) can set GUI.changed
		// even when no serialized value changed. ApplyModifiedProperties() only reports real data edits.
		bool serializedChanged = serializedObject.ApplyModifiedProperties();
		if (serializedChanged)
		{
			syncExistingInputFieldVisuals();
		}

		if (FastGUIInspectorUtility.drawDebugFoldout(ref mDebug))
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawCanvasBinding(target as Component);
			serializedObject.Update();
			using (new EditorGUI.DisabledScope(true))
			{
				FastGUIInspectorUtility.drawProperty(serializedObject, "mEventCamera", "Event Camera");
				FastGUIInspectorUtility.drawProperty(serializedObject, "mViewportClip", "Text Viewport");
				FastGUIInspectorUtility.drawProperty(serializedObject, "mTextComponent", "Text Component");
				FastGUIInspectorUtility.drawProperty(serializedObject, "mPlaceholderComponent", "Placeholder Component");
				FastGUIInspectorUtility.drawProperty(serializedObject, "mSelectionGraphic", "Selection Graphic");
				FastGUIInspectorUtility.drawProperty(serializedObject, "mCaretGraphic", "Caret Graphic");
			}
			if (targets.Length == 1 && target is FastInputField input && !hasCompleteHierarchy())
			{
				EditorGUILayout.HelpBox("The generated visual hierarchy is incomplete.", MessageType.Warning);
			}
			if (targets.Length == 1 && target is FastInputField singleInput && GUILayout.Button("Build / Repair Visual Hierarchy"))
			{
				Undo.RegisterFullObjectHierarchyUndo(singleInput.gameObject, "Build FastInputField Hierarchy");
				singleInput.getTextComponent();
				singleInput.getPlaceholderComponent();
				singleInput.getViewportClip();
				singleInput.getSelectionGraphic();
				singleInput.getCaretGraphic();
				EditorUtility.SetDirty(singleInput);
				serializedObject.Update();
			}
			EditorGUI.indentLevel--;
		}
	}

	private bool hasCompleteHierarchy()
	{
		SerializedProperty viewport = serializedObject.FindProperty("mViewportClip");
		SerializedProperty text = serializedObject.FindProperty("mTextComponent");
		SerializedProperty placeholder = serializedObject.FindProperty("mPlaceholderComponent");
		SerializedProperty selection = serializedObject.FindProperty("mSelectionGraphic");
		SerializedProperty caret = serializedObject.FindProperty("mCaretGraphic");
		return viewport != null && viewport.objectReferenceValue != null &&
			text != null && text.objectReferenceValue != null &&
			placeholder != null && placeholder.objectReferenceValue != null &&
			selection != null && selection.objectReferenceValue != null &&
			caret != null && caret.objectReferenceValue != null;
	}

	private void syncExistingInputFieldVisuals()
	{
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not FastInputField input)
			{
				continue;
			}

			input.refreshEditorVisuals();
			EditorUtility.SetDirty(input);
			PrefabUtility.RecordPrefabInstancePropertyModifications(input);
		}

		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}
}

public abstract class FastGeneratedComponentEditorBase : Editor
{
	private bool mDebug;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		FastGUIInspectorUtility.drawScript(serializedObject);
		EditorGUILayout.HelpBox("This component is generated and managed by FastGUI. Configure the owning FastMask, FastRectMask2D, FastText, or FastInputField instead.", MessageType.Info);
		if (FastGUIInspectorUtility.drawDebugFoldout(ref mDebug) && target is FastUIRenderElement graphic)
		{
			EditorGUI.indentLevel++;
			FastGUIInspectorUtility.drawCanvasBinding(graphic);
			FastGUIInspectorUtility.drawGraphicRuntimeStatus(graphic);
			FastGUIInspectorUtility.drawAllSerializedPropertiesReadOnly(serializedObject);
			EditorGUI.indentLevel--;
		}
	}
}

[CustomEditor(typeof(FastClipStencilGraphic))]
public class FastClipStencilGraphicEditor : FastGeneratedComponentEditorBase
{
}

[CustomEditor(typeof(FastMaskPopGraphic))]
public class FastMaskPopGraphicEditor : FastGeneratedComponentEditorBase
{
}

[CustomEditor(typeof(FastTextAtlasRun))]
public class FastTextAtlasRunEditor : FastGeneratedComponentEditorBase
{
}

[CustomEditor(typeof(FastInputFieldSelectionGraphic))]
public class FastInputFieldSelectionGraphicEditor : FastGeneratedComponentEditorBase
{
}
