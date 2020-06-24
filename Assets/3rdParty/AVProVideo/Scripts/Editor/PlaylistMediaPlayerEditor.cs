using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2015-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the MediaPlaylist.MediaItem class
	/// </summary>
	[CustomPropertyDrawer(typeof(MediaPlaylist))]
	public class MediaPlaylistDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			EditorGUILayout.LabelField("Items");

			SerializedProperty propItems = property.FindPropertyRelative("_items");

			if (propItems.arraySize == 0)
			{
				if (GUILayout.Button("Insert Item"))
				{
					propItems.InsertArrayElementAtIndex(0);
				}
			}

			for (int i = 0; i < propItems.arraySize; i++)
			{
				SerializedProperty propItem = propItems.GetArrayElementAtIndex(i);

				GUILayout.BeginVertical(GUI.skin.box);
				propItem.isExpanded = EditorGUILayout.ToggleLeft("Item " + i, propItem.isExpanded);
				if (propItem.isExpanded)
				{
					EditorGUILayout.PropertyField(propItem);
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Insert"))
					{
						propItems.InsertArrayElementAtIndex(i);
					}
					if (GUILayout.Button("Delete"))
					{
						propItems.DeleteArrayElementAtIndex(i);
					}
					EditorGUI.BeginDisabledGroup((i - 1) < 0);
					if (GUILayout.Button("Up"))
					{
						propItems.MoveArrayElement(i, i - 1);
					}
					EditorGUI.EndDisabledGroup();
					EditorGUI.BeginDisabledGroup((i + 1) >= propItems.arraySize);
					if (GUILayout.Button("Down"))
					{
						propItems.MoveArrayElement(i, i + 1);
					}
					EditorGUI.EndDisabledGroup();
					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();
			}		

			EditorGUI.EndProperty();
		}
	}

	/// <summary>
	/// Editor for the MediaPlaylist.MediaItem class
	/// </summary>
	[CustomPropertyDrawer(typeof(MediaPlaylist.MediaItem))]
	public class MediaPlaylistItemDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//SerializedProperty propSourceType = property.FindPropertyRelative("sourceType");
			SerializedProperty propIsOverrideTransition = property.FindPropertyRelative("isOverrideTransition");

			//EditorGUILayout.PropertyField(propSourceType);
			//if (propSourceType.enumValueIndex == 0)
			{
				EditorGUILayout.PropertyField(property.FindPropertyRelative("fileLocation"));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("filePath"));
			}
			/*else
			{
				EditorGUILayout.PropertyField(property.FindPropertyRelative("texture"));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("textureDuration"));
			}*/

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(property.FindPropertyRelative("stereoPacking"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("alphaPacking"));

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(property.FindPropertyRelative("loop"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("autoPlay"));
			EditorGUILayout.PropertyField(property.FindPropertyRelative("startMode"));
			SerializedProperty propProgressMode = property.FindPropertyRelative("progressMode");
			EditorGUILayout.PropertyField(propProgressMode);
			if (propProgressMode.enumValueIndex == (int)PlaylistMediaPlayer.ProgressMode.BeforeFinish)
			{
				EditorGUILayout.PropertyField(property.FindPropertyRelative("progressTimeSeconds"));
			}

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(propIsOverrideTransition, new GUIContent("Override Transition"));
			if (propIsOverrideTransition.boolValue)
			{
				EditorGUI.indentLevel++;
				SerializedProperty propTransitionMode = property.FindPropertyRelative("overrideTransition");
				EditorGUILayout.PropertyField(propTransitionMode, new GUIContent("Transition"));
				if (propTransitionMode.enumValueIndex != (int)PlaylistMediaPlayer.Transition.None)
				{
					EditorGUILayout.PropertyField(property.FindPropertyRelative("overrideTransitionDuration"), new GUIContent("Duration"));
					EditorGUILayout.PropertyField(property.FindPropertyRelative("overrideTransitionEasing.preset"), new GUIContent("Easing"));
				}
				EditorGUI.indentLevel--;
			}
		}
	}

	/// <summary>
	/// Editor for the PlaylistMediaPlayer component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(PlaylistMediaPlayer))]
	public class PlaylistMediaPlayerEditor : UnityEditor.Editor
	{
		private SerializedProperty _propPlayerA;
		private SerializedProperty _propPlayerB;
		private SerializedProperty _propNextTransition;
		private SerializedProperty _propPlaylist;
		private SerializedProperty _propPlaylistAutoProgress;
		private SerializedProperty _propPlaylistLoopMode;
		private SerializedProperty _propPausePreviousOnTransition;
		private SerializedProperty _propTransitionDuration;
		private SerializedProperty _propTransitionEasing;

		private void OnEnable()
		{
			_propPlayerA = serializedObject.FindProperty("_playerA");
			_propPlayerB = serializedObject.FindProperty("_playerB");
			_propNextTransition = serializedObject.FindProperty("_nextTransition");
			_propTransitionDuration = serializedObject.FindProperty("_transitionDuration");
			_propTransitionEasing = serializedObject.FindProperty("_transitionEasing.preset");
			_propPausePreviousOnTransition = serializedObject.FindProperty("_pausePreviousOnTransition");
			_propPlaylist = serializedObject.FindProperty("_playlist");
			_propPlaylistAutoProgress = serializedObject.FindProperty("_playlistAutoProgress");
			_propPlaylistLoopMode = serializedObject.FindProperty("_playlistLoopMode");
		}

		public override bool RequiresConstantRepaint()
		{
			PlaylistMediaPlayer media = (this.target) as PlaylistMediaPlayer;
			return (media.Control != null && media.isActiveAndEnabled);
		}

		public override void OnInspectorGUI()
		{
			PlaylistMediaPlayer media = (this.target) as PlaylistMediaPlayer;

			serializedObject.Update();

			if (media == null || _propPlayerA == null)
			{
				return;
			}

			EditorGUILayout.PropertyField(_propPlayerA);
			EditorGUILayout.PropertyField(_propPlayerB);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUILayout.Label("Playlist", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_propPlaylistAutoProgress, new GUIContent("Auto Progress"));
			EditorGUILayout.PropertyField(_propPlaylistLoopMode, new GUIContent("Loop Mode"));
			EditorGUILayout.PropertyField(_propPlaylist);
			EditorGUILayout.Space(); 
			EditorGUILayout.Space();
			GUILayout.Label("Transition", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(_propNextTransition, new GUIContent("Next"));
			EditorGUILayout.PropertyField(_propTransitionEasing, new GUIContent("Easing"));
			EditorGUILayout.PropertyField(_propTransitionDuration, new GUIContent("Duration"));
			EditorGUILayout.PropertyField(_propPausePreviousOnTransition, new GUIContent("Pause Previous"));
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			if (Application.isPlaying)
			{
				IMediaProducer textureSource = media.TextureProducer;

				Texture texture = null;
				if (textureSource != null)
				{
					texture = textureSource.GetTexture();
				}
				if (texture == null)
				{
					texture = EditorGUIUtility.whiteTexture;
				}

				float ratio = 1f;// (float)texture.width / (float)texture.height;

				// Reserve rectangle for texture
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				Rect textureRect;
				Rect alphaRect = new Rect(0f, 0f, 1f, 1f);
				if (texture != EditorGUIUtility.whiteTexture)
				{
					textureRect = GUILayoutUtility.GetRect(Screen.width / 2, Screen.width / 2, (Screen.width / 2) / ratio, (Screen.width / 2) / ratio);
				}
				else
				{
					textureRect = GUILayoutUtility.GetRect(1920f / 40f, 1080f / 40f);
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				string rateText = "0";
				string playerText = string.Empty;
				if (media.Info != null)
				{
					rateText = media.Info.GetVideoDisplayRate().ToString("F2");
					playerText = media.Info.GetPlayerDescription();
				}

				EditorGUILayout.LabelField("Display Rate", rateText);
				EditorGUILayout.LabelField("Using", playerText);
								
				// Draw the texture
				Matrix4x4 prevMatrix = GUI.matrix;
				if (textureSource != null && textureSource.RequiresVerticalFlip())
				{
					GUIUtility.ScaleAroundPivot(new Vector2(1f, -1f), new Vector2(0, textureRect.y + (textureRect.height / 2)));
				}

				if (!GUI.enabled)
				{
					GUI.color = Color.grey;
					GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
					GUI.color = Color.white;
				}
				else
				{
					{
						GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit, false);
						EditorGUI.DrawTextureAlpha(alphaRect, texture, ScaleMode.ScaleToFit);
					}
				}
				GUI.matrix = prevMatrix;
			}

			EditorGUI.BeginDisabledGroup(!(media.Control != null && media.Control.CanPlay() && media.isActiveAndEnabled && !EditorApplication.isPaused));
			OnInspectorGUI_PlayControls(media.Control, media.Info);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);

			GUILayout.Label("Current Item: " + media.PlaylistIndex + " / " + Mathf.Max(0, media.Playlist.Items.Count - 1) );

			GUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!media.CanJumpToItem(media.PlaylistIndex - 1));
			if (GUILayout.Button("Prev"))
			{
				media.PrevItem();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(!media.CanJumpToItem(media.PlaylistIndex + 1));
			if (GUILayout.Button("Next"))
			{
				media.NextItem();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();

			serializedObject.ApplyModifiedProperties();
		}


		private void OnInspectorGUI_PlayControls(IMediaControl control, IMediaInfo info)
		{
			GUILayout.Space(8.0f);

			// Slider
			EditorGUILayout.BeginHorizontal();
			bool isPlaying = false;
			if (control != null)
			{
				isPlaying = control.IsPlaying();
			}
			float currentTime = 0f;
			if (control != null)
			{
				currentTime = control.GetCurrentTimeMs();
			}

			float durationTime = 0f;
			if (info != null)
			{
				durationTime = info.GetDurationMs();
				if (float.IsNaN(durationTime))
				{
					durationTime = 0f;
				}
			}
			string timeUsed = Helper.GetTimeString(currentTime / 1000f, true);
			GUILayout.Label(timeUsed, GUILayout.ExpandWidth(false));

			float newTime = GUILayout.HorizontalSlider(currentTime, 0f, durationTime, GUILayout.ExpandWidth(true));
			if (newTime != currentTime)
			{
				control.Seek(newTime);
			}

			string timeTotal = "Infinity";
			if (!float.IsInfinity(durationTime))
			{
				timeTotal = Helper.GetTimeString(durationTime / 1000f, true);
			}

			GUILayout.Label(timeTotal, GUILayout.ExpandWidth(false));

			EditorGUILayout.EndHorizontal();

			// Buttons
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Rewind", GUILayout.ExpandWidth(false)))
			{
				control.Rewind();
			}

			if (!isPlaying)
			{
				GUI.color = Color.green;
				if (GUILayout.Button("Play", GUILayout.ExpandWidth(true)))
				{
					control.Play();
				}
			}
			else
			{
				GUI.color = Color.yellow;
				if (GUILayout.Button("Pause", GUILayout.ExpandWidth(true)))
				{
					control.Pause();
				}
			}
			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal();
		}		
	}
}