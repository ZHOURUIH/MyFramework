using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FastGUIInspectorUtility
{
	private static readonly FastDictionary<string, string> TOOLTIP_MAP = new FastDictionary<string, string>(StringComparer.Ordinal)
	{
		{ "Source Image", "The Sprite used to render this image." },
		{ "Texture", "The Texture used to render this Raw Image." },
		{ "Color", "The tint color applied to the rendered graphic." },
		{ "Material", "Optional material used to render this graphic. Leave None to use the FastCanvas default material." },
		{ "Image Type", "How the Source Image is rendered: Simple, Sliced, Tiled, or Filled." },
		{ "Preserve Aspect", "Preserves the Sprite aspect ratio when fitting it inside the RectTransform." },
		{ "Fill Center", "Controls whether the center of a Sliced or Tiled Sprite is rendered." },
		{ "Pixels Per Unit Multiplier", "Multiplies the Sprite pixels-per-unit value used by Sliced and Tiled rendering." },
		{ "Fill Method", "The method used to determine the visible portion of a Filled image." },
		{ "Fill Origin", "The edge or corner from which the Filled image starts." },
		{ "Fill Amount", "The visible amount of the image, from 0 to 1." },
		{ "Clockwise", "Controls the direction used by radial Filled image types." },
		{ "UV Rect", "The normalized UV rectangle used to sample the Texture." },
		{ "Visible", "Controls FastGUI rendering without changing the GameObject active state." },
		{ "Text", "The text displayed by this component." },
		{ "Font Asset", "The TMP Font Asset used to generate and render the text." },
		{ "Font Size", "The font size used to render the text." },
		{ "Character Spacing", "Additional horizontal spacing applied between characters." },
		{ "Line Spacing", "Additional vertical spacing applied between text lines." },
		{ "Wrapping", "Controls whether text wraps to a new line when it reaches the available width." },
		{ "Rich Text", "Enables supported rich-text tags in the text content." },
		{ "Horizontal Alignment", "Controls the horizontal alignment of text inside the RectTransform." },
		{ "Vertical Alignment", "Controls the vertical alignment of text inside the RectTransform." },
		{ "Show Mask Graphic", "Controls whether the graphic that defines the mask is also visible." },
		{ "Padding", "Padding applied to the Rect Mask 2D clip rectangle: Left, Bottom, Right, Top." },
		{ "Structure Mode", "Selects how this SOA Render Group describes and optimizes its repeated render structure." },
		{ "Allow Adjacent Group Merge", "Allows compatible adjacent SOA groups to be merged during render-order construction." },
		{ "Default Material", "The default material used by FastGUI graphics that do not specify their own material." },
		{ "Sorting Layer", "The Sorting Layer used by this FastCanvas renderer." },
		{ "Order in Layer", "The rendering order of this FastCanvas inside its Sorting Layer." },
		{ "Visibility Index Threshold", "Minimum visibility-change workload before the indexed visibility update path is used." },
		{ "Position Delta Batch Threshold", "Minimum workload before position-delta updates are processed as a packed batch." },
		{ "Simple Text Batch Threshold", "Minimum simple-text workload before the canvas batch text path is used." },
		{ "Merge Position Dirty Range", "Allows compatible position dirty slots to be merged into larger upload ranges." },
		{ "Cache Position Slots", "Caches position-slot information used by bulk position updates." },
		{ "Render Origin Shift", "Enables render-origin shifting for large coordinate ranges when beneficial." },
		{ "Min Saved Vertex Count", "Minimum number of saved vertex updates required before render-origin shifting is used." },
		{ "Placeholder", "Text displayed when the input field has no user-entered content." },
		{ "Text Color", "The color used by the input field text." },
		{ "Placeholder Color", "The color used by the placeholder text." },
		{ "Text Padding", "Padding between the text viewport and the input field bounds." },
		{ "Interactable", "Controls whether the input field can receive user interaction." },
		{ "Read Only", "Prevents the user from changing the text while still allowing selection and navigation." },
		{ "On Focus - Select All", "Selects all text when the input field receives focus." },
		{ "Line Type", "Controls whether the input field accepts single-line or multi-line text." },
		{ "Character Limit", "Maximum number of characters allowed. A value of 0 means no character limit." },
		{ "Line Limit", "Maximum number of text lines allowed. A value of 0 means no line limit." },
		{ "Caret Blink Rate", "The blink rate of the input caret." },
		{ "Caret Width", "The width of the input caret in pixels." },
		{ "Caret Color", "The color of the input caret." },
		{ "Selection Color", "The color used to highlight selected text." },
		{ "Restore On Escape", "Restores the text that existed before editing when editing is canceled with Escape." },
		{ "Touch Screen Keyboard", "Allows the platform touch-screen keyboard to be opened when editing." },
		{ "Content Type", "Preset that configures the expected input content and validation behavior." },
		{ "Input Type", "Controls how entered characters are displayed, such as standard text or password masking." },
		{ "Character Validation", "Selects the validation rule applied to each entered character." },
		{ "Keyboard Type", "Selects the requested touch-screen keyboard layout." },
		{ "Asterisk Char", "Character used to hide text when password-style input is enabled." },
		{ "Regex", "Regular expression used when Character Validation is set to Regex." },
		{ "Input Validator", "Custom validator used when Character Validation is set to Custom Validator." },
		{ "On Value Changed (String)", "Invoked whenever the input field text changes." },
		{ "On Submit (String)", "Invoked when the current input is submitted." },
		{ "On End Edit (String)", "Invoked when editing ends." },
		{ "On Select", "Invoked when the input field becomes selected." },
		{ "On Deselect", "Invoked when the input field is deselected." },
		{ "Event Camera", "Camera used to resolve pointer positions for this input field." },
		{ "Text Viewport", "Rect Mask 2D used to clip the editable text area." },
		{ "Text Component", "FastText component used to render the current input text." },
		{ "Placeholder Component", "FastText component used to render placeholder text." },
		{ "Selection Graphic", "Generated graphic used to render text selection." },
		{ "Caret Graphic", "Generated graphic used to render the input caret." },
		{ "Canvas", "FastCanvas currently responsible for this component." },
		{ "Cull", "Runtime culling state. True means the element is currently excluded from rendering by FastGUI." },
		{ "Render Active", "Runtime render-active state after GameObject and component activation are evaluated." },
		{ "Clip Active", "Runtime state indicating whether this element is affected by an active clip." },
		{ "Vertex Slot", "Runtime geometry slot assigned to this render element." },
		{ "Render Order", "Runtime render-order index assigned by FastCanvas." },
		{ "Vertex Start", "Start offset of this element inside the current canvas vertex stream." },
		{ "Vertex Count", "Number of vertices currently used by this render element." },
		{ "SOA Compatibility", "Runtime compatibility identifier used by SOA render-order optimization." },
		{ "Render Elements", "Number of render elements currently registered with this FastCanvas." },
		{ "Text Elements", "Number of FastText elements currently registered with this FastCanvas." },
		{ "Batches", "Number of logical FastGUI batches in the current canvas render state." },
		{ "Transform Nodes", "Number of transform nodes tracked by this FastCanvas." },
		{ "Tombstones", "Number of inactive render-order slots waiting for compaction." },
		{ "Hidden Roots", "Number of hidden hierarchy roots tracked by the visibility system." },
		{ "Clip Roots", "Number of active clip roots tracked by the clip system." },
		{ "SOA Groups", "Number of SOA render groups currently registered with this FastCanvas." },
		{ "Adjacent SOA Clusters", "Number of adjacent SOA clusters merged by the current render structure." },
		{ "Transform Backend", "Runtime backend currently used for transform-range processing." },
		{ "Vertex Upload", "Runtime backend currently used for vertex-stream uploads." },
		{ "Index Upload", "Runtime backend currently used for index-buffer uploads." },
		{ "GPU Index Format", "Current GPU index format and index stride." },
		{ "GPU Vertex Span", "Current vertex span allocated by the FastGUI renderer." },
		{ "Mask Depth", "Runtime stencil-mask nesting depth." },
		{ "Clip Rect", "Runtime clip rectangle produced by this Rect Mask 2D." },
		{ "Direct Children", "Number of direct Transform children below this SOA Render Group." },
		{ "Last Build Valid", "Whether the most recent SOA structure build completed successfully." },
		{ "Line Count", "Number of text lines generated by the current text layout." },
		{ "Visible Characters", "Number of visible glyphs generated by the current text layout." },
		{ "Missing Characters", "Number of characters that could not be resolved by the configured font and fallbacks." },
		{ "Fallback Characters", "Number of characters resolved from fallback font assets." },
		{ "Atlas Runs", "Number of text render runs generated across font atlases/materials." },
		{ "Preferred Size", "Preferred width and height calculated for the current text." },
		{ "Missing Character", "Unicode code point used when a requested character cannot be resolved." },
	};

	// Kept so existing editors do not need special cases. FastGUI inspectors intentionally hide the Script field.
	public static void drawScript(SerializedObject serializedObject)
	{
	}

	public static SerializedProperty drawProperty(SerializedObject serializedObject, string propertyName, string label = null, string tooltip = null, bool includeChildren = true)
	{
		SerializedProperty property = serializedObject.FindProperty(propertyName);
		if (property == null)
		{
			return null;
		}
		string displayName = string.IsNullOrEmpty(label) ? property.displayName : label;
		string propertyTooltip = !string.IsNullOrEmpty(tooltip) ? tooltip : property.tooltip;
		EditorGUILayout.PropertyField(property, createContent(displayName, propertyTooltip), includeChildren);
		return property;
	}

	public static SerializedProperty drawEnumPopup(SerializedObject serializedObject, string propertyName, string label, string[] displayNames, string tooltip = null)
	{
		SerializedProperty property = serializedObject.FindProperty(propertyName);
		if (property == null)
		{
			return null;
		}
		EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
		EditorGUI.BeginChangeCheck();
		int value = Mathf.Clamp(property.enumValueIndex, 0, displayNames.Length - 1);
		int next = EditorGUILayout.Popup(createContent(label, tooltip), value, displayNames);
		if (EditorGUI.EndChangeCheck())
		{
			property.enumValueIndex = next;
		}
		EditorGUI.showMixedValue = false;
		return property;
	}

	public static bool drawDebugFoldout(ref bool value)
	{
		EditorGUILayout.Space(3.0f);
		value = EditorGUILayout.Foldout(value, "Debug", true);
		return value;
	}

	public static void drawCanvasWarning(Component component)
	{
		if (component == null)
		{
			return;
		}
		FastCanvas canvas = component.GetComponentInParent<FastCanvas>(true);
		if (canvas == null)
		{
			EditorGUILayout.HelpBox("This component is not under a FastCanvas and will not be rendered by FastGUI.", MessageType.Warning);
		}
	}

	public static void drawCanvasBinding(Component component)
	{
		if (component == null)
		{
			return;
		}
		FastCanvas canvas = component.GetComponentInParent<FastCanvas>(true);
		using (new EditorGUI.DisabledScope(true))
		{
			Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
			EditorGUI.ObjectField(rect, createContent("Canvas"), canvas, typeof(FastCanvas), true);
		}
	}

	public static void drawGraphicRuntimeStatus(FastUIRenderElement element)
	{
		if (element == null)
		{
			return;
		}
		drawReadOnlyBool("Cull", element.isCulled());
		drawReadOnlyBool("Render Active", element.isRenderActive());
		drawReadOnlyBool("Clip Active", element.hasActiveClip());
		drawReadOnlyInt("Vertex Slot", element.getVertexSlot());
		drawReadOnlyInt("Render Order", element.getRenderOrderIndex());
		drawReadOnlyInt("Vertex Start", element.getCanvasRenderStart());
		drawReadOnlyInt("Vertex Count", element.getCanvasRenderCount());
		drawReadOnlyInt("SOA Compatibility", element.getSOACompatibilityID());
	}

	public static void drawReadOnlyText(string label, string value, string tooltip = null)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.TextField(createContent(label, tooltip), value ?? string.Empty);
		}
	}

	public static void drawReadOnlyInt(string label, int value, string tooltip = null)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.IntField(createContent(label, tooltip), value);
		}
	}

	public static void drawReadOnlyBool(string label, bool value, string tooltip = null)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.Toggle(createContent(label, tooltip), value);
		}
	}

	public static void drawReadOnlyRect(string label, Rect value, string tooltip = null)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.RectField(createContent(label, tooltip), value);
		}
	}

	public static void drawReadOnlyVector2(string label, Vector2 value, string tooltip = null)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
			EditorGUI.Vector2Field(rect, createContent(label, tooltip), value);
		}
	}

	public static void drawAllSerializedPropertiesReadOnly(SerializedObject serializedObject)
	{
		if (serializedObject == null)
		{
			return;
		}
		serializedObject.Update();
		EditorGUILayout.Space(3.0f);
		EditorGUILayout.LabelField(new GUIContent("Serialized Data", "Shows the complete serialized state for debugging."), EditorStyles.boldLabel);
		SerializedProperty iterator = serializedObject.GetIterator();
		bool enterChildren = true;
		using (new EditorGUI.DisabledScope(true))
		{
			while (iterator.NextVisible(enterChildren))
			{
				enterChildren = false;
				if (iterator.propertyPath == "m_Script")
				{
					continue;
				}
				EditorGUILayout.PropertyField(iterator, createContent(iterator.displayName, iterator.tooltip), true);
			}
		}
	}

	public static void notifyGraphicInspectorChanged(UnityEngine.Object[] targets)
	{
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is FastUIRenderElement element)
			{
				element.SendMessage("OnDidApplyAnimationProperties", SendMessageOptions.DontRequireReceiver);
				EditorUtility.SetDirty(element);
				PrefabUtility.RecordPrefabInstancePropertyModifications(element);
			}
		}
		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}

	public static bool drawBool<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, bool> getter, Action<TTarget, bool> setter, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out bool value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		bool next = EditorGUILayout.Toggle(createContent(label, tooltip), value);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawFloat<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, float> getter, Action<TTarget, float> setter, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out float value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		float next = EditorGUILayout.FloatField(createContent(label, tooltip), value);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	// Matches Unity's [Range] property presentation: slider plus editable numeric field.
	public static bool drawSlider<TTarget>(
		UnityEngine.Object[] targets,
		string label,
		Func<TTarget, float> getter,
		Action<TTarget, float> setter,
		float min,
		float max,
		string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out float value, out bool mixed))
		{
			return false;
		}
		value = Mathf.Clamp(value, min, max);
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		float next = EditorGUILayout.Slider(createContent(label, tooltip), value, min, max);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, Mathf.Clamp(next, min, max));
		}
		return changed;
	}

	public static bool drawInt<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, int> getter, Action<TTarget, int> setter, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out int value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		int next = EditorGUILayout.IntField(createContent(label, tooltip), value);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawString<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, string> getter, Action<TTarget, string> setter, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out string value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		string next = EditorGUILayout.TextField(createContent(label, tooltip), value ?? string.Empty);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawTextArea<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, string> getter, Action<TTarget, string> setter, float minHeight = 44.0f, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out string value, out bool mixed))
		{
			return false;
		}
		EditorGUILayout.LabelField(createContent(label, tooltip));
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		string next = EditorGUILayout.TextArea(value ?? string.Empty, GUILayout.MinHeight(minHeight));
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawColor<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, Color> getter, Action<TTarget, Color> setter, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out Color value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		Color next = EditorGUILayout.ColorField(createContent(label, tooltip), value);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawRect<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, Rect> getter, Action<TTarget, Rect> setter, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out Rect value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		Rect next = EditorGUILayout.RectField(createContent(label, tooltip), value);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawVector4<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, Vector4> getter, Action<TTarget, Vector4> setter, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out Vector4 value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
		Vector4 next = EditorGUI.Vector4Field(rect, createContent(label, tooltip), value);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawObject<TTarget, TObject>(UnityEngine.Object[] targets, string label, Func<TTarget, TObject> getter, Action<TTarget, TObject> setter, string tooltip = null, bool allowSceneObjects = false)
		where TTarget : UnityEngine.Object where TObject : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out TObject value, out bool mixed))
		{
			return false;
		}

		// Sprite fields intentionally keep a thumbnail while also showing the object name.
		// This matches the information density expected from Unity's Image inspector better
		// than either a thumbnail-only field or a compact name-only field.
		if (typeof(TObject) == typeof(Sprite))
		{
			return drawSpriteObject(targets, label, getter, setter, tooltip);
		}

		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
		TObject next = EditorGUI.ObjectField(rect, createContent(label, tooltip), value, typeof(TObject), allowSceneObjects) as TObject;
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	private static bool drawSpriteObject<TTarget, TObject>(UnityEngine.Object[] targets, string label, Func<TTarget, TObject> getter, Action<TTarget, TObject> setter, string tooltip)
		where TTarget : UnityEngine.Object where TObject : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out TObject rawValue, out bool mixed))
		{
			return false;
		}

		Sprite value = rawValue as Sprite;
		const float thumbnailSize = 64.0f;
		const float spacing = 4.0f;
		GUIContent content = createContent(label, tooltip);

		Rect row = EditorGUILayout.GetControlRect(false, thumbnailSize);
		Rect labelRect = new Rect(row.x, row.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
		EditorGUI.LabelField(labelRect, content);

		float valueX = row.x + EditorGUIUtility.labelWidth;
		float valueWidth = Mathf.Max(0.0f, row.xMax - valueX);
		float previewSize = Mathf.Min(thumbnailSize, valueWidth);
		Rect previewRect = new Rect(valueX, row.y, previewSize, thumbnailSize);
		Rect fieldRect = new Rect(previewRect.xMax + spacing, row.y, Mathf.Max(0.0f, valueWidth - previewSize - spacing), EditorGUIUtility.singleLineHeight);

		// Draw a UGUI-style Sprite preview. Border guide lines are overlaid using
		// Sprite.border so a 9-sliced Sprite is visibly different from a plain Sprite.
		GUI.Box(previewRect, GUIContent.none, EditorStyles.helpBox);
		if (!mixed && value != null)
		{
			drawSpritePreview(value, previewRect);
		}

		// Match Unity ObjectField behavior: clicking the thumbnail only pings the Sprite
		// in the Project window. Do not change Selection.activeObject, otherwise the
		// Inspector would switch away from the FastGUI component to the Sprite asset.
		if (!mixed && value != null && GUI.Button(previewRect, new GUIContent(string.Empty, content.tooltip), GUIStyle.none))
		{
			EditorGUIUtility.PingObject(value);
		}

		Sprite next = value;
		bool changed = false;
		// A narrow Inspector can leave no room for the object field. Keep the preview
		// visible and avoid sending an invalid rectangle to IMGUI.
		if (fieldRect.width > 1.0f)
		{
			EditorGUI.showMixedValue = mixed;
			EditorGUI.BeginChangeCheck();
			next = EditorGUI.ObjectField(fieldRect, GUIContent.none, value, typeof(Sprite), false) as Sprite;
			changed = EditorGUI.EndChangeCheck();
			EditorGUI.showMixedValue = false;
		}

		if (changed)
		{
			apply(targets, label, setter, (TObject)(UnityEngine.Object)next);
		}
		return changed;
	}

	private static void drawSpritePreview(Sprite sprite, Rect previewRect)
	{
		if (sprite == null)
		{
			return;
		}
		Rect area = new Rect(
			previewRect.x + 2.0f,
			previewRect.y + 2.0f,
			Mathf.Max(0.0f, previewRect.width - 4.0f),
			Mathf.Max(0.0f, previewRect.height - 4.0f));
		if (area.width <= 0.0f || area.height <= 0.0f)
		{
			return;
		}

		Rect spriteRect = sprite.rect;
		if (spriteRect.width <= 0.0f || spriteRect.height <= 0.0f)
		{
			return;
		}

		// Compute the exact fitted rect first so Sprite.border pixel distances can
		// be projected onto the same area as the preview image.
		float scale = Mathf.Min(area.width / spriteRect.width, area.height / spriteRect.height);
		Vector2 drawSize = new Vector2(spriteRect.width * scale, spriteRect.height * scale);
		Rect drawRect = new Rect(
			area.center.x - drawSize.x * 0.5f,
			area.center.y - drawSize.y * 0.5f,
			drawSize.x,
			drawSize.y);

		Texture preview = AssetPreview.GetAssetPreview(sprite);
		if (preview == null)
		{
			preview = AssetPreview.GetMiniThumbnail(sprite);
		}
		if (preview != null)
		{
			GUI.DrawTexture(drawRect, preview, ScaleMode.StretchToFill, true);
		}

		Vector4 border = sprite.border;
		if (border.sqrMagnitude <= 0.0f)
		{
			return;
		}

		float left = drawRect.xMin + drawRect.width * Mathf.Clamp01(border.x / spriteRect.width);
		float right = drawRect.xMax - drawRect.width * Mathf.Clamp01(border.z / spriteRect.width);
		// GUI coordinates grow downward while Sprite border Y values are bottom/top.
		float bottom = drawRect.yMax - drawRect.height * Mathf.Clamp01(border.y / spriteRect.height);
		float top = drawRect.yMin + drawRect.height * Mathf.Clamp01(border.w / spriteRect.height);

		drawSpriteBorderGuide(new Rect(left, drawRect.yMin, 1.0f, drawRect.height), true);
		drawSpriteBorderGuide(new Rect(right, drawRect.yMin, 1.0f, drawRect.height), true);
		drawSpriteBorderGuide(new Rect(drawRect.xMin, bottom, drawRect.width, 1.0f), false);
		drawSpriteBorderGuide(new Rect(drawRect.xMin, top, drawRect.width, 1.0f), false);
	}

	private static float snapGUIToPhysicalPixel(float value, float pixelsPerPoint)
	{
		return Mathf.Round(value * pixelsPerPoint) / pixelsPerPoint;
	}

	private static void drawSpriteBorderGuide(Rect rect, bool vertical)
	{
		// IMGUI coordinates are points, not guaranteed physical pixels. At non-100%
		// Editor/DPI scaling a 1.0f-wide rect can rasterize differently in X and Y.
		// Convert all geometry to the physical-pixel grid so both horizontal and
		// vertical guides are exactly one device pixel thick.
		float pixelsPerPoint = Mathf.Max(EditorGUIUtility.pixelsPerPoint, 1.0f);
		float physicalPixel = 1.0f / pixelsPerPoint;
		float dashLength = 4.0f / pixelsPerPoint;
		float gapLength = 3.0f / pixelsPerPoint;
		float endInset = 1.0f / pixelsPerPoint;
		float step = dashLength + gapLength;

		Color guideColor = new Color(0.92f, 0.92f, 0.92f, 1.0f);
		if (vertical)
		{
			float x = snapGUIToPhysicalPixel(rect.xMin, pixelsPerPoint);
			float start = snapGUIToPhysicalPixel(rect.yMin + endInset, pixelsPerPoint);
			float end = snapGUIToPhysicalPixel(rect.yMax - endInset, pixelsPerPoint);
			if (end <= start)
			{
				return;
			}
			for (float y = start; y < end; y += step)
			{
				float snappedY = snapGUIToPhysicalPixel(y, pixelsPerPoint);
				float length = Mathf.Min(dashLength, end - snappedY);
				if (length <= 0.0f)
				{
					break;
				}
				length = snapGUIToPhysicalPixel(length, pixelsPerPoint);
				EditorGUI.DrawRect(new Rect(x, snappedY, physicalPixel, length), guideColor);
			}
		}
		else
		{
			float y = snapGUIToPhysicalPixel(rect.yMin, pixelsPerPoint);
			float start = snapGUIToPhysicalPixel(rect.xMin + endInset, pixelsPerPoint);
			float end = snapGUIToPhysicalPixel(rect.xMax - endInset, pixelsPerPoint);
			if (end <= start)
			{
				return;
			}
			for (float x = start; x < end; x += step)
			{
				float snappedX = snapGUIToPhysicalPixel(x, pixelsPerPoint);
				float length = Mathf.Min(dashLength, end - snappedX);
				if (length <= 0.0f)
				{
					break;
				}
				length = snapGUIToPhysicalPixel(length, pixelsPerPoint);
				EditorGUI.DrawRect(new Rect(snappedX, y, length, physicalPixel), guideColor);
			}
		}
	}

	public static bool drawEnum<TTarget, TEnum>(UnityEngine.Object[] targets, string label, Func<TTarget, TEnum> getter, Action<TTarget, TEnum> setter, string tooltip = null)
		where TTarget : UnityEngine.Object where TEnum : Enum
	{
		if (!tryGetValue(targets, getter, out TEnum value, out bool mixed))
		{
			return false;
		}
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		TEnum next = (TEnum)(object)EditorGUILayout.EnumPopup(createContent(label, tooltip), value);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	public static bool drawPopup<TTarget>(UnityEngine.Object[] targets, string label, Func<TTarget, int> getter, Action<TTarget, int> setter, string[] displayNames, string tooltip = null)
		where TTarget : UnityEngine.Object
	{
		if (!tryGetValue(targets, getter, out int value, out bool mixed))
		{
			return false;
		}
		value = Mathf.Clamp(value, 0, Mathf.Max(0, displayNames.Length - 1));
		EditorGUI.showMixedValue = mixed;
		EditorGUI.BeginChangeCheck();
		int next = EditorGUILayout.Popup(createContent(label, tooltip), value, displayNames);
		bool changed = EditorGUI.EndChangeCheck();
		EditorGUI.showMixedValue = false;
		if (changed)
		{
			apply(targets, label, setter, next);
		}
		return changed;
	}

	private static GUIContent createContent(string label, string tooltip = null)
	{
		return new GUIContent(label, resolveTooltip(label, tooltip));
	}

	private static string resolveTooltip(string label, string explicitTooltip)
	{
		if (!string.IsNullOrEmpty(explicitTooltip))
		{
			return explicitTooltip;
		}
		if (!string.IsNullOrEmpty(label) && TOOLTIP_MAP.TryGetValue(label, out string mapped))
		{
			return mapped;
		}
		if (string.IsNullOrEmpty(label))
		{
			return string.Empty;
		}
		return "FastGUI property: " + label + ".";
	}

	private static bool tryGetValue<TTarget, TValue>(UnityEngine.Object[] targets, Func<TTarget, TValue> getter, out TValue value, out bool mixed)
		where TTarget : UnityEngine.Object
	{
		value = default;
		mixed = false;
		bool found = false;
		EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not TTarget item)
			{
				continue;
			}
			TValue current = getter(item);
			if (!found)
			{
				value = current;
				found = true;
			}
			else if (!comparer.Equals(value, current))
			{
				mixed = true;
			}
		}
		return found;
	}

	private static void apply<TTarget, TValue>(UnityEngine.Object[] targets, string label, Action<TTarget, TValue> setter, TValue value)
		where TTarget : UnityEngine.Object
	{
		Undo.RecordObjects(targets, "Set " + label);
		for (int i = 0; i < targets.Length; ++i)
		{
			if (targets[i] is not TTarget item)
			{
				continue;
			}
			setter(item, value);
			EditorUtility.SetDirty(item);
			PrefabUtility.RecordPrefabInstancePropertyModifications(item);
		}
		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}
}
