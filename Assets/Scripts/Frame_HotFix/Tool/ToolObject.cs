using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;
using static FrameBaseHotFix;

// 全部都是对MovableObject的操作,部分Transformable的通用操作在ToolFrame中
public static class OT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 摄像机视角
	#region 摄像机视角
	public static void FOV(this GameCamera obj, float fov)
	{
		CmdGameCameraFOV.execute(obj, 0.0f, 0.0f, fov, fov);
	}
	public static void FOV(this GameCamera obj, float start, float target, float onceLength)
	{
		obj.FOV_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV(this GameCamera obj, int keyframe, float start, float target, float onceLength)
	{
		obj.FOV_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV_EX(this GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.FOV_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void FOV_EX(this GameCamera obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		CmdGameCameraFOV.execute(obj, onceLength, offset, start, target, keyframe, loop, doingCallBack, doneCallback);
	}
	public static void ORTHO_SIZE(this GameCamera obj, float start, float target, float onceLength)
	{
		obj.ORTHO_SIZE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ORTHO_SIZE_EX(this GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.ORTHO_SIZE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ORTHO_SIZE_EX(this GameCamera obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		CmdGameCameraOrthoSize.execute(obj, start, target, onceLength, offset, keyframe, loop, doingCallBack, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 时间缩放
	#region 时间缩放
	public static void TIME(float scale)
	{
		CmdTimeManagerScaleTime.execute(scale);
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
		CmdTimeManagerScaleTime.execute(start, target, onceLength, offset, keyframe, loop, doingCallBack, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 透明度
	#region 透明度
	public static void ALPHA(this MovableObject obj, float alpha = 1.0f)
	{
		CmdMovableObjectAlpha.execute(obj, alpha);
	}
	public static void ALPHA(this MovableObject obj, float start, float target, float onceLength)
	{
		obj.ALPHA_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(this MovableObject obj, int keyframe, float start, float target, float onceLength)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(this MovableObject obj, int keyframe, float start, float target, float onceLength, bool loop)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void ALPHA(this MovableObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ALPHA_EX(this MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(this MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(this MovableObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(this MovableObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(this MovableObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(MovableObject obj, float alpha)");
			return;
		}
		CmdMovableObjectAlpha.execute(obj, start, target, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线设置物体透明度
	#region 以指定点列表以及时间点的路线设置物体透明度
	public static void ALPHA_PATH(this MovableObject obj)
	{
		CmdMovableObjectAlphaPath.execute(obj);
	}
	public static void ALPHA_PATH(this MovableObject obj, Dictionary<float, float> valueKeyFrame)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(this MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(this MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(this MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ALPHA_PATH_EX(this MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(this MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(this MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdMovableObjectAlphaPath.execute(obj, valueKeyFrame, valueOffset, offset, speed, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 基础数据类型的渐变,Tweener的操作由于暂时没有合适的地方放,所以放在这里
	public static void TWEEN_FLOAT(ref MyTweenerFloat tweener)
	{
		mTweenerManager?.destroyTweener(tweener);
		tweener = null;
	}
	public static MyTweenerFloat TWEEN_FLOAT(float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return TWEEN_FLOAT_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static MyTweenerFloat TWEEN_FLOAT_EX(int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void TWEEN_FLOAT(MyTweenerFloat tweener)");
			return null;
		}
		MyTweenerFloat tweenerFloat = mTweenerManager.createTweenerFloat();
		CmdMyTweenerFloat.execute(tweenerFloat, start, target, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
		return tweenerFloat;
	}
}