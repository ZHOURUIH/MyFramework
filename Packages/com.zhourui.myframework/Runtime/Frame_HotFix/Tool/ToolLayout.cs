using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static MathUtility;

// 全部都是对于UI布局或窗口的操作,部分Transformable的通用操作在ToolFrame中
public static class LT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 布局
	#region 布局
	public static LayoutScript LOAD(Type type, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, visible, isScene);
	}
	public static void LOAD_ASYNC(Type type, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool isScene, bool isAsync, Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, orderType, visible, isScene, callback);
	}
	#region UI_SCENE
	// UI作为场景时深度应该为固定值
	public static T LOAD_SCENE_HIDE<T>(int renderOrder) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, false, true) as T;
	}
	public static void LOAD_SCENE_ASYNC_HIDE<T>(int renderOrder, Action callback) where T : LayoutScript
	{
		CmdLayoutManagerLoad.executeAsync(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, false, true, callback);
	}
	public static void LOAD_SCENE_ASYNC_HIDE(Type type, int renderOrder, Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, LAYOUT_ORDER.FIXED, false, true, callback);
	}
	public static void LOAD_SCENE_ASYNC_HIDE(Type type, int renderOrder, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, LAYOUT_ORDER.FIXED, false, true, callback);
	}
	public static LayoutScript LOAD_SCENE_HIDE(Type type, int renderOrder)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, false, true);
	}
	public static T LOAD_SCENE<T>(int renderOrder) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, true, true) as T;
	}
	public static LayoutScript LOAD_SCENE(Type type, int renderOrder)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, true, true);
	}
	public static void LOAD_SCENE_ASYNC<T>(int renderOrder, Action callback) where T : LayoutScript
	{
		CmdLayoutManagerLoad.executeAsync(typeof(T), renderOrder, LAYOUT_ORDER.FIXED, true, true, callback);
	}
	public static void LOAD_SCENE_ASYNC(Type type, int renderOrder, Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, LAYOUT_ORDER.FIXED, true, true, callback);
	}
	public static LayoutScript LOAD_SCENE(Type type, int renderOrder, bool visible)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, LAYOUT_ORDER.FIXED, visible, true);
	}
	public static void LOAD_SCENE_ASYNC(Type type, int renderOrder, bool visible, Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, LAYOUT_ORDER.FIXED, visible, true, callback);
	}
	#endregion
	#region UGUI
	public static LayoutScript LOAD(Type type, bool visible)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.AUTO, visible, false);
	}
	public static LayoutScript LOAD(Type type, int renderOrder, LAYOUT_ORDER orderType, bool visible)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, visible, false);
	}
	public static void LOAD_ASYNC(Type type, int renderOrder, LAYOUT_ORDER orderType, Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, orderType, true, false, callback);
	}
	public static void LOAD_ASYNC(Type type, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, 0, LAYOUT_ORDER.AUTO, true, false, callback);
	}
	public static void LOAD_ASYNC(Type type)
	{
		CmdLayoutManagerLoad.executeAsync(type, 0, LAYOUT_ORDER.AUTO, true, false);
	}
	public static void LOAD_ASYNC<T>(Action callback) where T : LayoutScript
	{
		CmdLayoutManagerLoad.executeAsync(typeof(T), 0, LAYOUT_ORDER.AUTO, true, false, callback);
	}
	public static void LOAD_ASYNC<T>() where T : LayoutScript
	{
		CmdLayoutManagerLoad.executeAsync(typeof(T), 0, LAYOUT_ORDER.AUTO, true, false);
	}
	public static void LOAD_ASYNC<T>(int renderOrder, LAYOUT_ORDER orderType, Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(typeof(T), renderOrder, orderType, true, false, callback);
	}
	public static void LOAD_ASYNC_TOP<T>(Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(typeof(T), 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, true, false, callback);
	}
	public static void LOAD_ASYNC_TOP<T>()
	{
		CmdLayoutManagerLoad.executeAsync(typeof(T), 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, true, false);
	}
	public static void LOAD_ASYNC_HIDE(Type type, int renderOrder, LAYOUT_ORDER orderType, Action callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, orderType, false, false, callback);
	}
	public static void LOAD_ASYNC_HIDE(Type type, int renderOrder, LAYOUT_ORDER orderType, GameLayoutCallback callback)
	{
		CmdLayoutManagerLoad.executeAsync(type, renderOrder, orderType, false, false, callback);
	}
	public static LayoutScript LOAD_TOP_HIDE(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, false, false);
	}
	public static LayoutScript LOAD_HIDE(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.AUTO, false, false);
	}
	public static LayoutScript LOAD_HIDE(Type type, int renderOrder, LAYOUT_ORDER orderType)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, false, false);
	}
	public static T LOAD_HIDE<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.AUTO, false, false) as T;
	}
	public static T LOAD_HIDE<T>(int renderOrder, LAYOUT_ORDER orderType) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, orderType, false, false) as T;
	}
	public static T LOAD_TOP<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, true, false) as T;
	}
	public static T LOAD_TOP<T>(int renderOrder) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, LAYOUT_ORDER.ALWAYS_TOP, true, false) as T;
	}
	public static LayoutScript LOAD_TOP(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, true, false);
	}
	public static T LOAD<T>() where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), 0, LAYOUT_ORDER.AUTO, true, false) as T;
	}
	public static LayoutScript LOAD(Type type)
	{
		return CmdLayoutManagerLoad.execute(type, 0, LAYOUT_ORDER.AUTO, true, false);
	}
	public static T LOAD<T>(int renderOrder, LAYOUT_ORDER orderType) where T : LayoutScript
	{
		return CmdLayoutManagerLoad.execute(typeof(T), renderOrder, orderType, true, false) as T;
	}
	public static LayoutScript LOAD(Type type, int renderOrder, LAYOUT_ORDER orderType)
	{
		return CmdLayoutManagerLoad.execute(type, renderOrder, orderType, true, false);
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
	public static void SLIDER(this myUGUIObject slider, float value)
	{
		CmdWindowSlider.execute(slider, value);
	}
	public static void SLIDER(this myUGUIObject slider, float start, float target, float time)
	{
		CmdWindowSlider.execute(slider, start, target, time, 0.0f, KEY_CURVE.ZERO_ONE, false, null, null);
	}
	#endregion
	// 窗口填充
	#region 窗口填充
	public static void FILL(this myUGUIObject obj, float value = 1.0f)
	{
		CmdWindowFill.execute(obj, value);
	}
	public static void FILL(this myUGUIObject obj, float start, float target, float time)
	{
		obj.FILL_EX(KEY_CURVE.ZERO_ONE, start, target, time, null, null);
	}
	public static void FILL(this myUGUIObject obj, int keyframe, float start, float target, float time)
	{
		obj.FILL_EX(keyframe, start, target, time, null, null);
	}
	public static void FILL_EX(this myUGUIObject obj, float start, float target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.FILL_EX(KEY_CURVE.ZERO_ONE, start, target, time, doingCallback, doneCallback);
	}
	public static void FILL_EX(this myUGUIObject obj, int keyframe, float start, float target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdWindowFill.execute(obj, start, target, time, 0.0f, keyframe, false, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 透明度
	#region 透明度
	public static void ALPHA(this myUGUIObject obj, float alpha = 1.0f)
	{
		CmdWindowAlpha.execute(obj, alpha);
	}
	public static void ALPHA(this myUGUIObject obj, float start, float target, float onceLength)
	{
		obj.ALPHA_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(this myUGUIObject obj, int keyframe, float start, float target, float onceLength)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(this myUGUIObject obj, int keyframe, float start, float target, float onceLength, bool loop)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void ALPHA(this myUGUIObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ALPHA_EX(this myUGUIObject obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(this myUGUIObject obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(this myUGUIObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(this myUGUIObject obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_EX(keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(this myUGUIObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(myUGUIObject obj, float alpha)");
			return;
		}
		CmdWindowAlpha.execute(obj, start, target, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 颜色,也包含透明度
	#region 颜色
	public static void COLOR(this myUGUIObject obj)
	{
		obj.COLOR(Color.white);
	}
	public static void COLOR(this myUGUIObject obj, Color color)
	{
		CmdWindowColor.execute(obj, color);
	}
	public static void COLOR(this myUGUIObject obj, Color start, Color target, float onceLength)
	{
		obj.COLOR_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void COLOR(this myUGUIObject obj, int keyframe, Color start, Color target, float onceLength)
	{
		obj.COLOR_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void COLOR(this myUGUIObject obj, int keyframe, Color start, Color target, float onceLength, bool loop)
	{
		obj.COLOR_EX(keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void COLOR(this myUGUIObject obj, int keyframe, Color start, Color target, float onceLength, bool loop, float offset)
	{
		obj.COLOR_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void COLOR_EX(this myUGUIObject obj, Color start, Color target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.COLOR_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void COLOR_EX(this myUGUIObject obj, Color start, Color target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.COLOR_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void COLOR_EX(this myUGUIObject obj, int keyframe, Color start, Color target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.COLOR_EX(keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void COLOR_EX(this myUGUIObject obj, int keyframe, Color start, Color target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.COLOR_EX(keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void COLOR_EX(this myUGUIObject obj, int keyframe, Color start, Color target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void COLOR(myUGUIObject obj, float alpha)");
			return;
		}
		CmdWindowColor.execute(obj, start, target, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线设置物体透明度
	#region 以指定点列表以及时间点的路线设置物体透明度
	public static void ALPHA_PATH(this myUGUIObject obj)
	{
		CmdWindowAlphaPath.execute(obj);
	}
	public static void ALPHA_PATH(this myUGUIObject obj, Dictionary<float, float> valueKeyFrame)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(this myUGUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(this myUGUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(this myUGUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ALPHA_PATH_EX(this myUGUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(this myUGUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		obj.ALPHA_PATH_EX(valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(this myUGUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float timeOffset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdWindowAlphaPath.execute(obj, valueKeyFrame, doingCallback, doneCallback, valueOffset, timeOffset, speed, loop);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// HSL
	#region HSL
	public static void HSL(this myUGUIObject obj, Vector3 hsl)
	{
		CmdWindowHSL.execute(obj, hsl);
	}
	public static void HSL(this myUGUIObject obj, Vector3 start, Vector3 target, float onceLength)
	{
		obj.HSL(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f);
	}
	public static void HSL(this myUGUIObject obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		obj.HSL(keyframe, start, target, onceLength, false, 0.0f);
	}
	public static void HSL(this myUGUIObject obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		CmdWindowHSL.execute(obj, start, target, onceLength, offset, keyframe, loop, null, null);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 亮度
	#region 亮度
	public static void LUM(this myUGUIObject obj, float lum)
	{
		CmdWindowLum.execute(obj, lum);
	}
	public static void LUM(this myUGUIObject obj, float start, float target, float onceLength)
	{
		obj.LUM(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f);
	}
	public static void LUM(this myUGUIObject obj, int keyframe, float start, float target, float onceLength)
	{
		obj.LUM(keyframe, start, target, onceLength, false, 0.0f);
	}
	public static void LUM(this myUGUIObject obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		CmdWindowLum.execute(obj, onceLength, start, target, offset, keyframe, loop, null, null);
	}
	#endregion
}