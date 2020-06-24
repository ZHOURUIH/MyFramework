using UnityEngine;
using UnityEditor;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for UpdateStereoMaterial component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UpdateStereoMaterial))]
	public class UpdateStereoMaterialEditor : UnityEditor.Editor
	{
		private SerializedProperty _propCamera;
		private SerializedProperty _propRenderer;
		private SerializedProperty _propGraphic;
		private SerializedProperty _propMaterial;
		private SerializedProperty _propForceEyeMode;

		void OnEnable()
		{
			_propCamera = serializedObject.FindProperty("_camera");
			_propRenderer = serializedObject.FindProperty("_renderer");
			_propGraphic = serializedObject.FindProperty("_uGuiComponent");
			_propForceEyeMode = serializedObject.FindProperty("_forceEyeMode");
			_propMaterial = serializedObject.FindProperty("_material");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (_propCamera == null)
			{
				return;
			}

			EditorGUILayout.PropertyField(_propCamera);
			if (_propCamera.objectReferenceValue == null)
			{
				if (Camera.main == null)
				{
					ShowNoticeBox(MessageType.Error, "No 'main' camera found in scene and no camera assigned.");
				}
				else
				{
					ShowNoticeBox(MessageType.Warning, "No camera assigned.  Using 'main' camera: " + Camera.main.name);
				}
			}
			if (DetectMultipleMainCameras())
			{
				ShowNoticeBox(MessageType.Warning, "Multiple 'main' cameras found in scene. Make sure the correct camera is assigned.");
			}

			EditorGUILayout.PropertyField(_propRenderer);
			EditorGUILayout.PropertyField(_propGraphic);
			EditorGUILayout.PropertyField(_propMaterial);
			EditorGUILayout.PropertyField(_propForceEyeMode);
			if (_propRenderer.objectReferenceValue == null && _propGraphic.objectReferenceValue == null && _propMaterial.objectReferenceValue == null)
			{
				ShowNoticeBox(MessageType.Error, "At least one of the renderers (MeshRenderer, uGUI Graphic or Material) need to be assigned.");
			}

			serializedObject.ApplyModifiedProperties();
		}


		private static void ShowNoticeBox(MessageType messageType, string message)
		{
			switch (messageType)
			{
				case MessageType.Error:
					GUI.color = Color.red;
					message = "Error: " + message;
					break;
				case MessageType.Warning:
					GUI.color = Color.yellow;
					message = "Warning: " + message;
					break;
			}

			GUILayout.TextArea(message);
			GUI.color = Color.white;
		}

		private static bool DetectMultipleMainCameras()
		{
			bool result = false;
			if (Camera.main != null)
			{
				Camera[] cameras = Camera.allCameras;
				foreach (Camera cam in cameras)
				{
					if (cam != Camera.main && cam.CompareTag("MainCamera"))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}
	}
}