#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_5 || UNITY_5_4_OR_NEWER
	#define UNITY_FEATURE_UGUI
#endif

using UnityEngine;
using UnityEditor;
#if UNITY_FEATURE_UGUI
using UnityEditor.UI;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the DisplayUGUI component
	/// </summary>
	[CustomEditor(typeof(DisplayUGUI), true)]
	[CanEditMultipleObjects]
	public class DisplayUGUIEditor : GraphicEditor
	{
		// Note we have precedence for calling rectangle for just rect, even in the Inspector.
		// For example in the Camera component's Viewport Rect.
		// Hence sticking with Rect here to be consistent with corresponding property in the API.
		private static readonly GUIContent m_guiTextUVRectContent = new GUIContent("UV Rect");

		private SerializedProperty m_Movie;
		private SerializedProperty m_UVRect;
		private SerializedProperty m_DefaultTexture;
		private SerializedProperty m_NoDefaultDisplay;
		private SerializedProperty m_DisplayInEditor;
		private SerializedProperty m_SetNativeSize;
		private SerializedProperty m_ScaleMode;

		[MenuItem("GameObject/UI/AVPro Video uGUI", false, 0)]
		public static void CreateGameObject()
		{
			GameObject parent = Selection.activeGameObject;
			RectTransform parentCanvasRenderer = ( parent != null ) ? parent.GetComponent<RectTransform>() : null;
			if( parentCanvasRenderer )
			{
				GameObject go = new GameObject("AVPro Video");
				go.transform.SetParent(parent.transform, false);
				go.AddComponent<RectTransform>();
				go.AddComponent<CanvasRenderer>();
				go.AddComponent<DisplayUGUI>();
				Selection.activeGameObject = go;
			}
			else
			{
				EditorUtility.DisplayDialog("AVPro Video", "You must make the AVPro Video uGUI object as a child of a Canvas.", "Ok");
			}
		}

		public override bool RequiresConstantRepaint()
		{
			DisplayUGUI displayComponent = target as DisplayUGUI;
			return (displayComponent != null && displayComponent.HasValidTexture());
		}

	    protected override void OnEnable()
	    {
	        base.OnEnable();

	        m_Movie = serializedObject.FindProperty("_mediaPlayer");
	        m_UVRect = serializedObject.FindProperty("m_UVRect");
			m_SetNativeSize = serializedObject.FindProperty("_setNativeSize");
			m_ScaleMode = serializedObject.FindProperty("_scaleMode");

			m_NoDefaultDisplay = serializedObject.FindProperty("_noDefaultDisplay");
			m_DisplayInEditor = serializedObject.FindProperty("_displayInEditor");
			m_DefaultTexture = serializedObject.FindProperty("_defaultTexture");

			SetShowNativeSize(true);
	    }

	    public override void OnInspectorGUI()
	    {
	        serializedObject.Update();

	        EditorGUILayout.PropertyField(m_Movie);
			EditorGUILayout.PropertyField(m_DefaultTexture);
			EditorGUILayout.PropertyField(m_NoDefaultDisplay);
			EditorGUILayout.PropertyField(m_DisplayInEditor);
			AppearanceControlsGUI();
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			RaycastControlsGUI();
#endif
			EditorGUILayout.PropertyField(m_UVRect, m_guiTextUVRectContent);

			EditorGUILayout.PropertyField(m_SetNativeSize);
			EditorGUILayout.PropertyField(m_ScaleMode);

	        SetShowNativeSize(false);
	        NativeSizeButtonGUI();

	        serializedObject.ApplyModifiedProperties();
	    }

	    private void SetShowNativeSize(bool instant)
	    {
	        base.SetShowNativeSize(m_Movie.objectReferenceValue != null, instant);
	    }

	    /// <summary>
	    /// Allow the texture to be previewed.
	    /// </summary>
	    public override bool HasPreviewGUI()
	    {
			DisplayUGUI rawImage = target as DisplayUGUI;
	        return rawImage != null;
	    }

	    /// <summary>
	    /// Draw the Image preview.
	    /// </summary>
		public override void OnPreviewGUI(Rect drawArea, GUIStyle background)
	    {
			DisplayUGUI rawImage = target as DisplayUGUI;
	        Texture tex = rawImage.mainTexture;

	        if (tex == null)
	            return;

			// Create the texture rectangle that is centered inside rect.
			Rect outerRect = drawArea;

			Matrix4x4 m = GUI.matrix;
			// Flip the image vertically
			if (rawImage.HasValidTexture())
			{
				if (rawImage._mediaPlayer.TextureProducer.RequiresVerticalFlip())
				{
					GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0, outerRect.y + (outerRect.height / 2f)));
				}
			}

			EditorGUI.DrawTextureTransparent(outerRect, tex, ScaleMode.ScaleToFit);//, outer.width / outer.height);
	        //SpriteDrawUtility.DrawSprite(tex, rect, outer, rawImage.uvRect, rawImage.canvasRenderer.GetColor());

			GUI.matrix = m;
	    }

	    /// <summary>
	    /// Info String drawn at the bottom of the Preview
	    /// </summary>
	    public override string GetInfoString()
	    {
			DisplayUGUI rawImage = target as DisplayUGUI;

			string text = string.Empty;
			if (rawImage.HasValidTexture())
			{
				text += string.Format("Video Size: {0}x{1}\n",
				                        Mathf.RoundToInt(Mathf.Abs(rawImage.mainTexture.width)),
				                        Mathf.RoundToInt(Mathf.Abs(rawImage.mainTexture.height)));
			}

	        // Image size Text
			text += string.Format("Display Size: {0}x{1}",
	                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.width)),
	                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.height)));

	        return text;
	    }
	}
}

#endif