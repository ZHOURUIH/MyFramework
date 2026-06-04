using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static StringUtility;

[CustomEditor(typeof(TweenSequence))]
public class TweenSequenceAuthoringEditor : GameEditorBase
{
	private TweenSequence mSequence;
	private bool mPlaying;
	private float mPreviewTime;
	private double mStartTime;
	public override void OnInspectorGUI()
	{
		EditorCurveFactory.reload();
		mSequence = (TweenSequence)target;
		DrawGroups();
		space(10);
		using (new GUILayout.HorizontalScope())
		{
			if (button("Add Group"))
			{
				Undo.RecordObject(mSequence, "Add Group");
				mSequence.mGroupList.Add(new TweenGroup());
				EditorUtility.SetDirty(mSequence);
			}
			if (button("Clear Groups"))
			{
				Undo.RecordObject(mSequence, "Clear Groups");
				mSequence.mGroupList.Clear();
				EditorUtility.SetDirty(mSequence);
			}
		}
		space(10);
		DrawPreview();
		if (GUI.changed)
		{
			EditorUtility.SetDirty(mSequence);
		}
	}
	private void OnDisable()
	{
		StopPlay();
	}
	private void DrawGroups()
	{
		List<TweenGroup> groups = mSequence.mGroupList;
		for (int groupIndex = 0; groupIndex < groups.Count; ++groupIndex)
		{
			using (new GUILayout.VerticalScope("box"))
			{
				TweenGroup group = groups[groupIndex];
				using (new GUILayout.HorizontalScope())
				{
					label("Group" + groupIndex);
					if (button("Add Track"))
					{
						Undo.RecordObject(mSequence, "Add Track");
						group.mTrackList.Add(new TweenTrack());
					}
					if (button("Delete" + "Group" + groupIndex))
					{
						Undo.RecordObject(mSequence, "Delete Group");
						groups.RemoveAt(groupIndex);
						return;
					}
				}
				// 一个Group中所有的Track
				List<TweenTrack> tracks = group.mTrackList;
				for (int i = 0; i < tracks.Count; ++i)
				{
					if (i > 0)
					{
						space(7);
						EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 2), new Color(0.35f, 0.35f, 0.35f));
						space(7);
					}
					DrawTrack(group, i);
				}
			}
			space(5);
		}
		toggle("NeedReset", ref mSequence.mResetWhenStop);
		EditorGUI.BeginDisabledGroup(mSequence.hasSelfValueType());
		toggle("Loop", ref mSequence.mLoop);
		EditorGUI.EndDisabledGroup();
		if (mSequence.hasSelfValueType())
		{
			mSequence.mLoop = false;
			EditorGUILayout.HelpBox("存在SELF模式轨道时不允许循环播放", MessageType.Warning);
		}
	}
	private void DrawTrack(TweenGroup group, int trackIndex)
	{
		TweenTrack track = group.mTrackList[trackIndex];
		using (new GUILayout.HorizontalScope())
		{
			label("Track" + trackIndex);
			if (button("Delete" + " Track" + trackIndex))
			{
				Undo.RecordObject(mSequence, "Delete Track");
				group.mTrackList.RemoveAt(trackIndex);
				return;
			}
		}
		toggle("Enable", ref track.mEnable);

		displayEnum("Type", ref track.mType);

		int[] ids = EditorCurveFactory.getIDs();
		if (ids.Length == 0)
		{
			return;
		}
		if (track.mCurveID == 0)
		{
			track.setCurveID(ids[0]);
		}
		ids.find(track.mCurveID, out int curIndex);
		int newIndex = EditorGUILayout.Popup("Curve", curIndex, EditorCurveFactory.getNames());
		track.setCurveID(ids[newIndex]);
		EditorGUILayout.CurveField("Preview", EditorCurveFactory.getPreviewCurve(track.mCurveID), GUILayout.Height(20));

		displayFloat("Duration", ref track.mDuration);
		displayFloat("StartDelay", ref track.mStartDelay);
		if (track.mType == TWEEN_TYPE.MOVE)
		{
			displayEnum("Start Mode", ref track.mStartMode);
			if (track.mStartMode == START_MODE.VALUE)
			{
				using (new GUILayout.HorizontalScope())
				{
					displayVector3("Start", ref track.mStartValue);
					if (button("Set To Current"))
					{
						track.mStartValue = mSequence.transform.localPosition;
						EditorUtility.SetDirty(mSequence);
					}
				}
			}

			displayEnum("Target Mode", ref track.mTargetMode);
			if (track.mTargetMode == TARGET_MODE.VALUE)
			{
				using (new GUILayout.HorizontalScope())
				{
					displayVector3("Target", ref track.mTargetValue);
					if (button("Set To Current"))
					{
						track.mTargetValue = mSequence.transform.localPosition;
						EditorUtility.SetDirty(mSequence);
					}
				}
			}
			else
			{
				if (track.mTargetMode == TARGET_MODE.TRANSFORM_SNAPSHOT || track.mTargetMode == TARGET_MODE.TRANSFORM_REALTIME)
				{
					objectField("Target Transform", ref track.mTargetTransform);
				}
				displayVector3("Target Offset", ref track.mTargetOffset);
			}
		}
		else if (track.mType == TWEEN_TYPE.SCALE)
		{
			using (new GUILayout.HorizontalScope())
			{
				displayVector3("Start", ref track.mStartValue);
				if (button("Set To Current"))
				{
					track.mStartValue = mSequence.transform.localScale;
					EditorUtility.SetDirty(mSequence);
				}
			}
			using (new GUILayout.HorizontalScope())
			{
				displayVector3("Target", ref track.mTargetValue);
				if (button("Set To Current"))
				{
					track.mTargetValue = mSequence.transform.localScale;
					EditorUtility.SetDirty(mSequence);
				}
			}
		}
		else if (track.mType == TWEEN_TYPE.ROTATE)
		{
			using (new GUILayout.HorizontalScope())
			{
				displayVector3("Start", ref track.mStartValue);
				if (button("Set To Current"))
				{
					track.mStartValue = mSequence.transform.localEulerAngles;
					EditorUtility.SetDirty(mSequence);
				}
			}
			using (new GUILayout.HorizontalScope())
			{
				displayVector3("Target", ref track.mTargetValue);
				if (button("Set To Current"))
				{
					track.mTargetValue = mSequence.transform.localEulerAngles;
					EditorUtility.SetDirty(mSequence);
				}
			}
		}
	}
	private void DrawPreview()
	{
		using (new GUILayout.HorizontalScope())
		{
			if (!mPlaying)
			{
				if (button("Play"))
				{
					StartPlay();
				}
			}
			else
			{
				if (button("Stop"))
				{
					StopPlay();
				}
			}
			if (button("Reset"))
			{
				StopPlay(true);
			}
		}

		label("time:" + FToS(mPreviewTime, 2) + "/" + mSequence.getTotalLength());
		EditorGUILayout.Slider("Preview", mPreviewTime, 0.0f, mSequence.getTotalLength());
	}
	private void StartPlay()
	{
		mPlaying = true;
		// 播放之前先确认所有轨道都是在停止状态的
		mSequence.stop(true);
		mSequence.play();
		mStartTime = EditorApplication.timeSinceStartup;
		EditorApplication.update -= UpdatePreview;
		EditorApplication.update += UpdatePreview;
	}
	private void StopPlay(bool forceReset = false)
	{
		mPlaying = false;
		if (mSequence != null)
		{
			mSequence.stop(forceReset);
		}
		if (forceReset || mSequence.mResetWhenStop)
		{
			mPreviewTime = 0.0f;
		}
		EditorApplication.update -= UpdatePreview;
	}
	private void UpdatePreview()
	{
		if (!mPlaying || mSequence == null)
		{
			StopPlay();
			return;
		}

		float totalLength = mSequence.getTotalLength();
		float currentTime = (float)(EditorApplication.timeSinceStartup - mStartTime);
		if (currentTime >= totalLength)
		{
			if (mSequence.mLoop)
			{
				currentTime -= totalLength;
				mStartTime = EditorApplication.timeSinceStartup - currentTime;
			}
			else
			{
				currentTime = totalLength;
			}
		}

		mPreviewTime = currentTime;
		mSequence.evaluateSequence(currentTime, out Vector3 pos, out Vector3 scale, out Vector3 rot);
		Transform transform = mSequence.transform;
		transform.localPosition = pos;
		transform.localScale = scale;
		transform.localEulerAngles = rot;
		if (currentTime >= totalLength && !mSequence.mLoop)
		{
			StopPlay();
		}
		if (SceneView.lastActiveSceneView != null)
		{
			SceneView.lastActiveSceneView.Repaint();
		}
		Repaint();
	}
}