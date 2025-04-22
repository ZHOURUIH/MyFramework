using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static MathUtility;

// 全部都是对于UI布局或窗口的操作,部分Transformable的通用操作在ToolFrame中
public class LT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 布局
	#region 布局
	public static LayoutScript LOAD(Type type, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene, bool isAsync, GameLayoutCallback callback)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, visible, isScene, isAsync, callback);
	}
	#region UI_SCENE
	// UI作为场景时深度应该为固定值
	public static T LOAD_SCENE_HIDE<T>(int renderOrder) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, false, true, false, null) as T;
	}
	public static void LOAD_SCENE_ASYNC_HIDE<T>(int renderOrder, GameLayoutCallback callback) where T : LayoutScript
	{
		CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, false, true, true, callback);
	}
	public static void LOAD_SCENE_ASYNC_HIDE(Type type, int renderOrder, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, false, true, true, callback);
	}
	public static LayoutScript LOAD_SCENE_HIDE(Type type, int renderOrder)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, false, true, false, null);
	}
	public static T LOAD_SCENE_SHOW<T>(int renderOrder) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, true, true, false, null) as T;
	}
	public static LayoutScript LOAD_SCENE_SHOW(Type type, int renderOrder)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, true, true, false, null);
	}
	public static void LOAD_SCENE_ASYNC_SHOW<T>(int renderOrder, GameLayoutCallback callback) where T : LayoutScript
	{
		CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, true, true, true, callback);
	}
	public static void LOAD_SCENE_ASYNC_SHOW(Type type, int renderOrder, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, true, true, true, callback);
	}
	public static LayoutScript LOAD_SCENE(Type type, int renderOrder, bool visible)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, visible, true, false, null);
	}
	public static void LOAD_SCENE_ASYNC(Type type, int renderOrder, bool visible, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, visible, true, true, callback);
	}
	#endregion
	#region UGUI
	public static LayoutScript LOAD(Type type, bool visible)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.AUTO, visible, false, false, null);
	}
	public static LayoutScript LOAD(Type type, int renderOrder, LAYOUT_ORDER orderType, bool visible)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, visible, false, false, null);
	}
	public static void LOAD_ASYNC_SHOW(Type type, int renderOrder, LAYOUT_ORDER orderType, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.execute(type, renderOrder, orderType, true, false, true, callback);
	}
	public static void LOAD_ASYNC_SHOW<T>(GameLayoutCallback callback) where T : LayoutScript
	{
		CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.AUTO, true, false, true, callback);
	}
	public static void LOAD_ASYNC_SHOW<T>() where T : LayoutScript
	{
		CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.AUTO, true, false, true, null);
	}
	public static void LOAD_ASYNC_HIDE(Type type, int renderOrder, LAYOUT_ORDER orderType, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.execute(type, renderOrder, orderType, false, false, true, callback);
	}
	public static LayoutScript LOAD_TOP_HIDE(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, false, false, false, null);
	}
	public static LayoutScript LOAD_HIDE(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.AUTO, false, false, false, null);
	}
	public static LayoutScript LOAD_HIDE(Type type, int renderOrder, LAYOUT_ORDER orderType)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, false, false, false, null);
	}
	public static T LOAD_HIDE<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.AUTO, false, false, false, null) as T;
	}
	public static T LOAD_HIDE<T>(int renderOrder, LAYOUT_ORDER orderType) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, orderType, false, false, false, null) as T;
	}
	public static T LOAD_TOP_SHOW<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, true, false, false, null) as T;
	}
	public static T LOAD_TOP_SHOW<T>(int renderOrder) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.ALWAYS_TOP, true, false, false, null) as T;
	}
	public static LayoutScript LOAD_TOP_SHOW(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, true, false, false, null);
	}
	public static T LOAD_SHOW<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.AUTO, true, false, false, null) as T;
	}
	public static LayoutScript LOAD_SHOW(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.AUTO, true, false, false, null);
	}
	public static T LOAD_SHOW<T>(int renderOrder, LAYOUT_ORDER orderType) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, orderType, true, false, false, null) as T;
	}
	public static LayoutScript LOAD_SHOW(Type type, int renderOrder, LAYOUT_ORDER orderType)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, true, false, false, null);
	}
	#endregion
	#region LAYOUT_VISIBLE
	public static void HIDE<T>() where T : LayoutScript
	{
		CmdLayoutManagerVisible.execute(typeof(T), false, false);
	}
	public static void HIDE(Type type)
	{
		CmdLayoutManagerVisible.execute(type, false, false);
	}
	public static void HIDE_FORCE(Type type)
	{
		CmdLayoutManagerVisible.execute(type, false, true);
	}
	public static void HIDE_FORCE<T>() where T : LayoutScript
	{
		CmdLayoutManagerVisible.execute(typeof(T), false, true);
	}
	public static void SHOW(Type type)
	{
		CmdLayoutManagerVisible.execute(type, true, false);
	}
	public static T SHOW<T>() where T : LayoutScript
	{
		return CmdLayoutManagerVisible.execute(typeof(T), true, false) as T;
	}
	public static void SHOW_FORCE(Type type)
	{
		CmdLayoutManagerVisible.execute(type, true, true);
	}
	public static T SHOW_FORCE<T>() where T : LayoutScript
	{
		return CmdLayoutManagerVisible.execute(typeof(T), true, true) as T;
	}
	public static T VISIBLE<T>(bool visible) where T : LayoutScript
	{
		return CmdLayoutManagerVisible.execute(typeof(T), visible, false) as T;
	}
	public static void VISIBLE(Type type, bool visible)
	{
		CmdLayoutManagerVisible.execute(type, visible, false);
	}
	public static void VISIBLE_FORCE(Type type, bool visible)
	{
		CmdLayoutManagerVisible.execute(type, visible, true);
	}
	#endregion
	#region UNLOAD
	public static void UNLOAD<T>() where T : LayoutScript
	{
		Type type = typeof(T);
		// 需要首先隐藏布局
		CmdLayoutManagerVisible.execute(type, false, false);
		mLayoutManager.destroyLayout(type);
	}
	public static void UNLOAD(Type type)
	{
		// 需要首先隐藏布局
		CmdLayoutManagerVisible.execute(type, false, false);
		mLayoutManager.destroyLayout(type);
	}
	#endregion
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 进度条
	#region 进度条
	public static void SLIDER(ComponentOwner slider, float value)
	{
		CMD(out CmdWindowSlider cmd, LOG_LEVEL.LOW);
		cmd.mStartValue = value;
		cmd.mTargetValue = value;
		cmd.mOnceLength = 0.0f;
		pushCommand(cmd, slider);
	}
	public static void SLIDER(ComponentOwner slider, float start, float target, float time)
	{
		CMD(out CmdWindowSlider cmd, LOG_LEVEL.LOW);
		cmd.mStartValue = start;
		cmd.mTargetValue = target;
		cmd.mOnceLength = time;
		cmd.mKeyframe = KEY_CURVE.ZERO_ONE;
		pushCommand(cmd, slider);
	}
	#endregion
	// 窗口填充
	#region 窗口填充
	public static void FILL(myUIObject obj, float value = 1.0f)
	{
		CMD(out CmdWindowFill cmd, LOG_LEVEL.LOW);
		cmd.mStartValue = value;
		cmd.mTargetValue = value;
		cmd.mOnceLength = 0.0f;
		pushCommand(cmd, obj);
	}
	public static void FILL(myUIObject obj, float start, float target, float time)
	{
		FILL_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, null, null);
	}
	public static void FILL(myUIObject obj, int keyframe, float start, float target, float time)
	{
		FILL_EX(obj, keyframe, start, target, time, null, null);
	}
	public static void FILL_EX(myUIObject obj, float start, float target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		FILL_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, doingCallback, doneCallback);
	}
	public static void FILL_EX(myUIObject obj, int keyframe, float start, float target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CMD(out CmdWindowFill cmd, LOG_LEVEL.LOW);
		cmd.mStartValue = start;
		cmd.mTargetValue = target;
		cmd.mOnceLength = time;
		cmd.mKeyframe = keyframe;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 透明度
	#region 透明度
	public static void ALPHA(myUIObject obj, float alpha = 1.0f)
	{
		CMD(out CmdWindowAlpha cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartAlpha = alpha;
		cmd.mTargetAlpha = alpha;
		pushCommand(cmd, obj);
	}
	public static void ALPHA(myUIObject obj, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(myUIObject obj, int keyframe, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(myUIObject obj, int keyframe, float start, float target, float onceLength, bool loop)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void ALPHA(myUIObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ALPHA_EX(myUIObject obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(myUIObject obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(myUIObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(myUIObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(myUIObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(myUIObject obj, float alpha)");
			return;
		}
		CMD(out CmdWindowAlpha cmd, LOG_LEVEL.LOW);
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
	// 颜色,也包含透明度
	#region 颜色
	public static void COLOR(myUIObject obj)
	{
		COLOR(obj, Color.white);
	}
	public static void COLOR(myUIObject obj, Color color)
	{
		CMD(out CmdWindowColor cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartColor = color;
		cmd.mTargetColor = color;
		pushCommand(cmd, obj);
	}
	public static void COLOR(myUIObject obj, Color start, Color target, float onceLength)
	{
		COLOR_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void COLOR(myUIObject obj, int keyframe, Color start, Color target, float onceLength)
	{
		COLOR_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void COLOR(myUIObject obj, int keyframe, Color start, Color target, float onceLength, bool loop)
	{
		COLOR_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void COLOR(myUIObject obj, int keyframe, Color start, Color target, float onceLength, bool loop, float offset)
	{
		COLOR_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void COLOR_EX(myUIObject obj, Color start, Color target, float onceLength, KeyFrameCallback doneCallback)
	{
		COLOR_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void COLOR_EX(myUIObject obj, Color start, Color target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		COLOR_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void COLOR_EX(myUIObject obj, int keyframe, Color start, Color target, float onceLength, KeyFrameCallback doneCallback)
	{
		COLOR_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void COLOR_EX(myUIObject obj, int keyframe, Color start, Color target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		COLOR_EX(obj, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void COLOR_EX(myUIObject obj, int keyframe, Color start, Color target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(myUIObject obj, float alpha)");
			return;
		}
		CMD(out CmdWindowColor cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartColor = start;
		cmd.mTargetColor = target;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线设置物体透明度
	#region 以指定点列表以及时间点的路线设置物体透明度
	public static void ALPHA_PATH(myUIObject obj)
	{
		if (obj == null)
		{
			return;
		}
		CmdWindowAlphaPath.execute(obj);
	}
	public static void ALPHA_PATH(myUIObject obj, Dictionary<float, float> valueKeyFrame)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(myUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(myUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(myUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ALPHA_PATH_EX(myUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, KeyFrameCallback doneCallback)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(myUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(myUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float timeOffset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CmdWindowAlphaPath.execute(obj, valueKeyFrame, doingCallback, doneCallback, valueOffset, timeOffset, speed, loop);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// HSL
	#region HSL
	public static void HSL(myUIObject obj, Vector3 hsl)
	{
		CMD(out CmdWindowHSL cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartHSL = hsl;
		cmd.mTargetHSL = hsl;
		pushCommand(cmd, obj);
	}
	public static void HSL(myUIObject obj, Vector3 start, Vector3 target, float onceLength)
	{
		HSL(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f);
	}
	public static void HSL(myUIObject obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		HSL(obj, keyframe, start, target, onceLength, false, 0.0f);
	}
	public static void HSL(myUIObject obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		CMD(out CmdWindowHSL cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartHSL = start;
		cmd.mTargetHSL = target;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 亮度
	#region 亮度
	public static void LUM(myUIObject obj, float lum)
	{
		CMD(out CmdWindowLum cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartLum = lum;
		cmd.mTargetLum = lum;
		pushCommand(cmd, obj);
	}
	public static void LUM(myUIObject obj, float start, float target, float onceLength)
	{
		LUM(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f);
	}
	public static void LUM(myUIObject obj, int keyframe, float start, float target, float onceLength)
	{
		LUM(obj, keyframe, start, target, onceLength, false, 0.0f);
	}
	public static void LUM(myUIObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		CMD(out CmdWindowLum cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartLum = start;
		cmd.mTargetLum = target;
		pushCommand(cmd, obj);
	}
	#endregion
}