using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FastSpriteRenderer))]
[CanEditMultipleObjects]
public sealed class FastSpriteRendererEditor : Editor
{
	private SerializedProperty mSprite;
	private SerializedProperty mColor;
	private SerializedProperty mFlipX;
	private SerializedProperty mFlipY;
	private SerializedProperty mMaterial;
	private SerializedProperty mDrawMode;
	private SerializedProperty mSize;
	private SerializedProperty mTileMode;
	private SerializedProperty mAdaptiveModeThreshold;
	private SerializedProperty mMaskInteraction;
	private SerializedProperty mSpriteSortPoint;
	private SerializedProperty mSortingLayerID;
	private SerializedProperty mSortingOrder;

	private void OnEnable()
	{
		mSprite = serializedObject.FindProperty("mSprite");
		mColor = serializedObject.FindProperty("mColor");
		mFlipX = serializedObject.FindProperty("mFlipX");
		mFlipY = serializedObject.FindProperty("mFlipY");
		mMaterial = serializedObject.FindProperty("mMaterial");
		mDrawMode = serializedObject.FindProperty("mDrawMode");
		mSize = serializedObject.FindProperty("mSize");
		mTileMode = serializedObject.FindProperty("mTileMode");
		mAdaptiveModeThreshold = serializedObject.FindProperty("mAdaptiveModeThreshold");
		mMaskInteraction = serializedObject.FindProperty("mMaskInteraction");
		mSpriteSortPoint = serializedObject.FindProperty("mSpriteSortPoint");
		mSortingLayerID = serializedObject.FindProperty("mSortingLayerID");
		mSortingOrder = serializedObject.FindProperty("mSortingOrder");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(mSprite, new GUIContent("Sprite", "The Sprite to render."));
		EditorGUILayout.PropertyField(mColor, new GUIContent("Color", "Vertex tint applied to the Sprite."));
		drawFlip();
		EditorGUILayout.PropertyField(mMaterial, new GUIContent("Material", "Optional shared material. None uses the FastSpriteRenderSystem default material."));
		EditorGUILayout.PropertyField(mDrawMode, new GUIContent("Draw Mode", "Simple, Sliced, or Tiled rendering, matching SpriteRenderer terminology."));

		if (!mDrawMode.hasMultipleDifferentValues)
		{
			SpriteDrawMode mode = (SpriteDrawMode)mDrawMode.enumValueIndex;
			if (mode == SpriteDrawMode.Sliced || mode == SpriteDrawMode.Tiled)
			{
				EditorGUILayout.PropertyField(mSize, new GUIContent("Size", "Local size used by Sliced and Tiled rendering."));
			}
			if (mode == SpriteDrawMode.Tiled)
			{
				EditorGUILayout.PropertyField(mTileMode, new GUIContent("Tile Mode", "Continuous or Adaptive tiled rendering."));
				if (!mTileMode.hasMultipleDifferentValues && (SpriteTileMode)mTileMode.enumValueIndex == SpriteTileMode.Adaptive)
				{
					EditorGUILayout.PropertyField(mAdaptiveModeThreshold, new GUIContent("Adaptive Mode Threshold", "Threshold before Adaptive mode switches from stretching to tiling."));
				}
			}
			if (mode == SpriteDrawMode.Simple)
			{
				EditorGUILayout.PropertyField(mSpriteSortPoint, new GUIContent("Sprite Sort Point", "Center or Pivot position used by dynamic FastSprite sorting modes."));
			}
		}

		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.PropertyField(mMaskInteraction, new GUIContent("Mask Interaction", "Serialized for migration/API parity. SpriteMask rendering is not implemented in FastSpriteRenderer V1."));
		}
		if (!mMaskInteraction.hasMultipleDifferentValues &&
			(SpriteMaskInteraction)mMaskInteraction.enumValueIndex != SpriteMaskInteraction.None)
		{
			EditorGUILayout.HelpBox("SpriteMask interaction is not implemented in FastSpriteRenderer V1. Rendering behaves as None.", MessageType.Warning);
		}

		EditorGUILayout.Space(4.0f);
		EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);
		drawSortingLayer();
		EditorGUILayout.PropertyField(mSortingOrder, new GUIContent("Order in Layer", "Render priority within the selected Sorting Layer."));

		if (UnityEngine.Object.FindFirstObjectByType<FastSpriteRenderSystem>() == null)
		{
			EditorGUILayout.HelpBox("No FastSpriteRenderSystem exists in the Scene. Add one to render FastSpriteRenderer components.", MessageType.Warning);
			if (GUILayout.Button("Create FastSpriteRenderSystem"))
			{
				GameObject go = new("FastSpriteRenderSystem");
				Undo.RegisterCreatedObjectUndo(go, "Create FastSpriteRenderSystem");
				go.AddComponent<FastSpriteRenderSystem>();
				Selection.activeGameObject = go;
			}
		}

		if (serializedObject.ApplyModifiedProperties())
		{
			for (int i = 0; i < targets.Length; ++i)
			{
				if (targets[i] is FastSpriteRenderer renderer)
				{
					renderer.refreshAll();
					EditorUtility.SetDirty(renderer);
				}
			}
			SceneView.RepaintAll();
		}
	}

	private void drawFlip()
	{
		Rect rect = EditorGUILayout.GetControlRect();
		Rect valueRect = EditorGUI.PrefixLabel(rect, new GUIContent("Flip", "Flip Sprite UVs without changing the Transform."));
		float half = valueRect.width * 0.5f;
		Rect xRect = new(valueRect.x, valueRect.y, half - 2.0f, valueRect.height);
		Rect yRect = new(valueRect.x + half + 2.0f, valueRect.y, half - 2.0f, valueRect.height);
		EditorGUI.PropertyField(xRect, mFlipX, new GUIContent("X"));
		EditorGUI.PropertyField(yRect, mFlipY, new GUIContent("Y"));
	}

	private void drawSortingLayer()
	{
		SortingLayer[] layers = SortingLayer.layers;
		string[] names = new string[layers.Length];
		int selected = 0;
		for (int i = 0; i < layers.Length; ++i)
		{
			names[i] = layers[i].name;
			if (!mSortingLayerID.hasMultipleDifferentValues && layers[i].id == mSortingLayerID.intValue)
			{
				selected = i;
			}
		}

		EditorGUI.showMixedValue = mSortingLayerID.hasMultipleDifferentValues;
		EditorGUI.BeginChangeCheck();
		int next = EditorGUILayout.Popup(new GUIContent("Sorting Layer", "Sorting Layer used by the generated batch renderer."), selected, names);
		if (EditorGUI.EndChangeCheck() && next >= 0 && next < layers.Length)
		{
			mSortingLayerID.intValue = layers[next].id;
		}
		EditorGUI.showMixedValue = false;
	}
}
