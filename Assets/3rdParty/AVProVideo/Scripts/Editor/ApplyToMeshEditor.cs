using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the ApplyToMesh component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ApplyToMesh))]
	public class ApplyToMeshEditor : UnityEditor.Editor
	{
		private static readonly GUIContent _guiTextTextureProperty = new GUIContent("Texture Property");

		private SerializedProperty _propTextureOffset;
		private SerializedProperty _propTextureScale;
		private SerializedProperty _propMediaPlayer;
		private SerializedProperty _propRenderer;
		private SerializedProperty _propTexturePropertyName;
		private SerializedProperty _propDefaultTexture;
		private GUIContent[] _materialTextureProperties = new GUIContent[0];

		void OnEnable()
		{
			_propTextureOffset = serializedObject.FindProperty("_offset");
			_propTextureScale = serializedObject.FindProperty("_scale");
			_propMediaPlayer = serializedObject.FindProperty("_media");
			_propRenderer = serializedObject.FindProperty("_mesh");
			_propTexturePropertyName = serializedObject.FindProperty("_texturePropertyName");
			_propDefaultTexture = serializedObject.FindProperty("_defaultTexture");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (_propRenderer == null)
			{
				return;
			}

			EditorGUILayout.PropertyField(_propMediaPlayer);
			EditorGUILayout.PropertyField(_propDefaultTexture);
			EditorGUILayout.PropertyField(_propRenderer);

			bool hasKeywords = false;
			int texturePropertyIndex = 0;
			if (_propRenderer.objectReferenceValue != null)
			{
				Renderer r = (Renderer)(_propRenderer.objectReferenceValue);

				Material[] materials = r.sharedMaterials;

				MaterialProperty[] matProps = MaterialEditor.GetMaterialProperties(materials);

				foreach (Material mat in materials)
				{
					if (mat.shaderKeywords.Length > 0)
					{
						hasKeywords = true;
						break;
					}
				}

				List<GUIContent> items = new List<GUIContent>(16);
				foreach (MaterialProperty matProp in matProps)
				{
					if (matProp.type == MaterialProperty.PropType.Texture)
					{
						if (matProp.name == _propTexturePropertyName.stringValue)
						{
							texturePropertyIndex = items.Count;
						}
						items.Add(new GUIContent(matProp.name));
					}
				}
				_materialTextureProperties = items.ToArray();
			}

			int newTexturePropertyIndex = EditorGUILayout.Popup(_guiTextTextureProperty, texturePropertyIndex, _materialTextureProperties);
			if (newTexturePropertyIndex != texturePropertyIndex)
			{
				_propTexturePropertyName.stringValue = _materialTextureProperties[newTexturePropertyIndex].text;
			}

			if (hasKeywords && _propTexturePropertyName.stringValue != "_MainTex")
			{
				EditorGUILayout.HelpBox("When using an uber shader you may need to enable the keywords on a material for certain texture slots to take effect.  You can sometimes achieve this (eg with Standard shader) by putting a dummy texture into the texture slot.", MessageType.Info);
			}

			EditorGUILayout.PropertyField(_propTextureOffset);
			EditorGUILayout.PropertyField(_propTextureScale);

			serializedObject.ApplyModifiedProperties();
		}
	}
}