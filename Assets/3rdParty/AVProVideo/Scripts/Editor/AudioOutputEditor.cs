using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the AudioOutput component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(AudioOutput))]
	public class AudioOutputEditor : UnityEditor.Editor
	{
		private static readonly GUIContent _guiTextChannel = new GUIContent("Channel");
		private static readonly GUIContent _guiTextChannels = new GUIContent("Channels");
		private static readonly string[] _channelMaskOptions = { "1", "2", "3", "4", "5", "6", "7", "8" };

		private AudioOutput _target;
		private SerializedProperty _channelMaskProperty;

		void OnEnable()
		{
			_target = (this.target) as AudioOutput;
			_channelMaskProperty = serializedObject.FindProperty("_channelMask");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawDefaultInspector();

			if(_target._audioOutputMode == AudioOutput.AudioOutputMode.Multiple)
			{
				_channelMaskProperty.intValue = EditorGUILayout.MaskField(_guiTextChannels, _channelMaskProperty.intValue, _channelMaskOptions);
			}
			else
			{
				int prevVal = 0;
				for(int i = 0; i < 8; ++i)
				{
					if((_channelMaskProperty.intValue & (1 << i)) > 0)
					{
						prevVal = i;
						break;
					}
				}
				
				int newVal = Mathf.Clamp(EditorGUILayout.IntSlider(_guiTextChannel, prevVal, 0, 7), 0, 7);
				_channelMaskProperty.intValue = 1 << newVal;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
