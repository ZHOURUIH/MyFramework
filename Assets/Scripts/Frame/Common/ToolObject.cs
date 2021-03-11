using UnityEngine;
using System.Collections.Generic;

// ObjectTools
public class OT : FrameBase
{
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 摄像机视角
	#region 摄像机视角
	public static void FOV(GameCamera obj, float fov)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CommandCameraFOV cmd, false);
		cmd.mStartFOV = fov;
		cmd.mTargetFOV = fov;
		cmd.mOnceLength = 0.0f;
		pushCommand(cmd, obj);
	}
	public static void FOV(GameCamera obj, float start, float target, float onceLength)
	{
		FOV_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV(GameCamera obj, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		FOV_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV_EX(GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		FOV_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void FOV_EX(GameCamera obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CommandCameraFOV cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartFOV = start;
		cmd.mTargetFOV = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallBack;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	public static void ORTHO_SIZE(GameCamera obj, float start, float target, float onceLength)
	{
		ORTHO_SIZE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ORTHO_SIZE_EX(GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ORTHO_SIZE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ORTHO_SIZE_EX(GameCamera obj, KEY_FRAME keyframe, float startFOV, float targetFOV, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CommandCameraOrthoSize cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartOrthoSize = startFOV;
		cmd.mTargetOrthoSize = targetFOV;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallBack;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 显示
	#region 物体的显示和隐藏
	public static void ACTIVE(MovableObject obj, bool active = true)
	{
		obj?.setActive(active);
	}
	public static CommandMovableObjectActive ACTIVE_DELAY(IDelayCmdWatcher watcher, MovableObject obj, bool active, float delayTime)
	{
		return ACTIVE_DELAY_EX(watcher, obj, active, delayTime, null);
	}
	public static CommandMovableObjectActive ACTIVE_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, bool active, float dealyTime, CommandCallback startCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CMD(out CommandMovableObjectActive cmd, false, true);
		cmd.mActive = active;
		cmd.addStartCommandCallback(startCallback);
		pushDelayCommand(cmd, obj, dealyTime, watcher);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 时间缩放
	#region 时间缩放
	public static void TIME(float scale)
	{
		CMD(out CommandTimeManagerScaleTime cmd, true, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		pushCommand(cmd, mTimeManager);
	}
	public static void TIME(float start, float target, float onceLength)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void TIME(KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		TIME_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void TIME(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void TIME(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void TIME_EX(float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, float offsetTime, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, doingCallBack, doneCallback);
	}
	public static void TIME_EX(KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(keyframe, start, target, onceLength, false, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		CMD(out CommandTimeManagerScaleTime cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartScale = start;
		cmd.mTargetScale = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallBack;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, mTimeManager);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY(IDelayCmdWatcher watcher, float delayTime, float scale)
	{
		CMD(out CommandTimeManagerScaleTime cmd, false, true);
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		cmd.mOnceLength = 0.0f;
		pushDelayCommand(cmd, mTimeManager, delayTime, watcher);
		cmd.setIgnoreTimeScale(true);
		return cmd;
	}
	public static CommandTimeManagerScaleTime TIME_DELAY(IDelayCmdWatcher watcher, float delayTime, float start, float target, float onceLength)
	{
		return TIME_DELAY_EX(watcher, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(IDelayCmdWatcher watcher, float delayTime, float start, float target, float onceLength, KeyFrameCallback moveDoneCallback)
	{
		return TIME_DELAY_EX(watcher, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, moveDoneCallback);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(IDelayCmdWatcher watcher, float delayTime, float start, float target, float onceLength, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		return TIME_DELAY_EX(watcher, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, movingCallback, moveDoneCallback);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(IDelayCmdWatcher watcher, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		return TIME_DELAY_EX(watcher, delayTime, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(IDelayCmdWatcher watcher, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		return TIME_DELAY_EX(watcher, delayTime, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(IDelayCmdWatcher watcher, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		return TIME_DELAY_EX(watcher, delayTime, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(IDelayCmdWatcher watcher, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		CMD(out CommandTimeManagerScaleTime cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mStartScale = start;
		cmd.mTargetScale = target;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = movingCallback;
		cmd.mDoneCallBack = moveDoneCallback;
		pushDelayCommand(cmd, mTimeManager, delayTime, watcher);
		cmd.setIgnoreTimeScale(true);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 物体音效
	#region 播放物体音效
	public static void AUDIO(MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandMovableObjectPlayAudio>(obj, false);
	}
	public static void AUDIO(MovableObject obj, string sound, bool loop, float volume)
	{
		if (obj == null)
		{
			return;
		}
		if (isEmpty(sound))
		{
			logError("sound name must be valid, use void AUDIO(MovableObject obj) to stop sound");
			return;
		}
		CMD(out CommandMovableObjectPlayAudio cmd, false);
		cmd.mSoundFileName = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		pushCommand(cmd, obj);
	}
	public static void AUDIO(MovableObject obj, SOUND_DEFINE sound, bool loop)
	{
		AUDIO(obj, sound, loop, 1.0f, true);
	}
	public static void AUDIO(MovableObject obj, SOUND_DEFINE sound, bool loop, float volume)
	{
		AUDIO(obj, sound, loop, volume, true);
	}
	public static void AUDIO(MovableObject obj, SOUND_DEFINE sound, bool loop, float volume, bool useVolumeCoe)
	{
		if (obj == null)
		{
			return;
		}
		string name = (sound != SOUND_DEFINE.MIN) ? mAudioManager.getAudioName(sound) : null;
		if (isEmpty(name))
		{
			logError("sound name must be valid, use void AUDIO(MovableObject obj) to stop sound");
			return;
		}
		CMD(out CommandMovableObjectPlayAudio cmd, false);
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		cmd.mUseVolumeCoe = useVolumeCoe;
		pushCommand(cmd, obj);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandMovableObjectPlayAudio>(watcher, obj, delayTime, false);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, SOUND_DEFINE sound)
	{
		return AUDIO_DELAY(watcher, obj, delayTime, sound, false, 1.0f, true);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, SOUND_DEFINE sound, float volume)
	{
		return AUDIO_DELAY(watcher, obj, delayTime, sound, false, volume, true);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, SOUND_DEFINE sound, bool loop)
	{
		return AUDIO_DELAY(watcher, obj, delayTime, sound, loop, 1.0f, true);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, SOUND_DEFINE sound, bool loop, float volume, bool useVolumeCoe)
	{
		if (obj == null)
		{
			return null;
		}
		string name = (sound != SOUND_DEFINE.MIN) ? mAudioManager.getAudioName(sound) : null;
		if (isEmpty(name))
		{
			logError("sound name must be valid, use CommandMovableObjectPlayAudio AUDIO_DELAY(MovableObject obj, float delayTime) to stop sound");
			return null;
		}
		CMD(out CommandMovableObjectPlayAudio cmd, false, true);
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		cmd.mUseVolumeCoe = useVolumeCoe;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 透明度
	#region 透明度
	public static void ALPHA(MovableObject obj, float alpha = 1.0f)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CommandMovableObjectAlpha cmd, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartAlpha = alpha;
		cmd.mTargetAlpha = alpha;
		pushCommand(cmd, obj);
	}
	public static void ALPHA(MovableObject obj, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ALPHA_EX(MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(MovableObject obj, float alpha)");
			return;
		}
		CMD(out CommandMovableObjectAlpha cmd, false);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartAlpha = start;
		cmd.mTargetAlpha = target;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, float alpha)
	{
		if (obj == null)
		{
			return null;
		}
		CMD(out CommandMovableObjectAlpha cmd, false, true);
		cmd.mOnceLength = 0.0f;
		cmd.mStartAlpha = alpha;
		cmd.mTargetAlpha = alpha;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, float start, float target, float onceLength)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		if (keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,CommandMovableObjectAlpha ALPHA_DELAY(MovableObject obj, float delayTime, float alpha)");
			return null;
		}
		CMD(out CommandMovableObjectAlpha cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartAlpha = start;
		cmd.mTargetAlpha = target;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线设置物体透明度
	#region 以指定点列表以及时间点的路线设置物体透明度
	public static void ALPHA_PATH(MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandMovableObjectAlphaPath>(obj, false);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ALPHA_PATH_EX(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, KeyFrameCallback doneCallback)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CommandMovableObjectAlphaPath cmd, false);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandMovableObjectAlphaPath>(watcher, obj, delayTime, false);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, loop, offset, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY_EX(IDelayCmdWatcher watcher, MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CMD(out CommandMovableObjectAlphaPath cmd, false, true);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
	#endregion
}