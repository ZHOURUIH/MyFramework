using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;
using static FrameBase;
using static FrameUtility;

// 全部都是对MovableObject的操作,部分Transformable的通用操作在ToolFrame中
public class OT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 摄像机视角
	#region 摄像机视角
	public static void FOV(GameCamera obj, float fov)
	{
		if (obj == null)
		{
			return;
		}
		CmdGameCameraFOV.execute(obj, 0.0f, 0.0f, fov, fov);
	}
	public static void FOV(GameCamera obj, float start, float target, float onceLength)
	{
		FOV_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV(GameCamera obj, int keyframe, float start, float target, float onceLength)
	{
		FOV_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV_EX(GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		FOV_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void FOV_EX(GameCamera obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CmdGameCameraFOV.execute(obj, onceLength, offset, start, target, keyframe, loop, doingCallBack, doneCallback);
	}
	public static void ORTHO_SIZE(GameCamera obj, float start, float target, float onceLength)
	{
		ORTHO_SIZE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ORTHO_SIZE_EX(GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ORTHO_SIZE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ORTHO_SIZE_EX(GameCamera obj, int keyframe, float startFOV, float targetFOV, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdGameCameraOrthoSize cmd, LOG_LEVEL.LOW);
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
	//------------------------------------------------------------------------------------------------------------------------------
	// 时间缩放
	#region 时间缩放
	public static void TIME(float scale)
	{
		CMD(out CmdTimeManagerScaleTime cmd, LOG_LEVEL.FORCE);
		cmd.mOnceLength = 0.0f;
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		pushCommand(cmd, mTimeManager);
	}
	public static void TIME(float start, float target, float onceLength)
	{
		TIME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void TIME(int keyframe, float start, float target, float onceLength)
	{
		TIME_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void TIME(int keyframe, float start, float target, float onceLength, bool loop)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void TIME(int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void TIME_EX(float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, float offsetTime, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, doingCallBack, doneCallback);
	}
	public static void TIME_EX(int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(keyframe, start, target, onceLength, false, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(int keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		CMD(out CmdTimeManagerScaleTime cmd, LOG_LEVEL.LOW);
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
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 透明度
	#region 透明度
	public static void ALPHA(MovableObject obj, float alpha = 1.0f)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdMovableObjectAlpha cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartAlpha = alpha;
		cmd.mTargetAlpha = alpha;
		pushCommand(cmd, obj);
	}
	public static void ALPHA(MovableObject obj, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, int keyframe, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, int keyframe, float start, float target, float onceLength, bool loop)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ALPHA_EX(MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(MovableObject obj, float alpha)");
			return;
		}
		CMD(out CmdMovableObjectAlpha cmd, LOG_LEVEL.LOW);
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
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线设置物体透明度
	#region 以指定点列表以及时间点的路线设置物体透明度
	public static void ALPHA_PATH(MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		pushCommand<CmdMovableObjectAlphaPath>(obj, LOG_LEVEL.LOW);
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
		CMD(out CmdMovableObjectAlphaPath cmd, LOG_LEVEL.LOW);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 基础数据类型的渐变,Tweener的操作由于暂时没有合适的地方放,所以放在这里
	public static MyTweenerFloat TWEEN_FLOAT(float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return TWEEN_FLOAT_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static MyTweenerFloat TWEEN_FLOAT_EX(int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(MovableObject obj, float alpha)");
			return null;
		}
		MyTweenerFloat tweenerFloat = mTweenerManager.createTweenerFloat();
		CMD(out CmdMyTweenerFloat cmd, LOG_LEVEL.LOW);
		cmd.mKeyframeID = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStart = start;
		cmd.mTarget = target;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, tweenerFloat);
		return tweenerFloat;
	}
}