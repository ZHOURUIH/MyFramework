using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;
using static MathUtility;

// 全部都是对于UI布局或窗口的操作,部分Transformable的通用操作在ToolFrame中
public class LT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 布局
	#region 布局
	public static void LOAD_LAYOUT(int id, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool immediately, string param, bool isScene, bool isAsync, LayoutAsyncDone callback)
	{
		CmdLayoutManagerLoad.execute(id, renderOrder, orderType, param, immediately, visible, isScene, isAsync, callback);
	}
	#region UGUI
	public static void LOAD_UGUI(int id, bool visible)
	{
		LOAD_LAYOUT(id, 0, LAYOUT_ORDER.AUTO, visible, false, null, false, false, null);
	}
	public static void LOAD_UGUI(int id, int renderOrder, LAYOUT_ORDER orderType, bool visible)
	{
		LOAD_LAYOUT(id, renderOrder, orderType, visible, false, null, false, false, null);
	}
	public static void LOAD_UGUI(int id, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool immediately)
	{
		LOAD_LAYOUT(id, renderOrder, orderType, visible, immediately, null, false, false, null);
	}
	public static void LOAD_UGUI(int id, int renderOrder, LAYOUT_ORDER orderType, bool visible, bool immediately, string param)
	{
		LOAD_LAYOUT(id, renderOrder, orderType, visible, immediately, param, false, false, null);
	}
	// UI作为场景时深度应该为固定值
	public static void LOAD_UGUI_SCENE(int id, int renderOrder, bool visible, bool immediately, string param)
	{
		LOAD_LAYOUT(id, renderOrder, LAYOUT_ORDER.FIXED, visible, immediately, param, true, false, null);
	}
	public static void LOAD_UGUI_ASYNC(int id, int renderOrder, LAYOUT_ORDER orderType, LayoutAsyncDone callback)
	{
		LOAD_LAYOUT(id, renderOrder, orderType, true, false, null, false, true, callback);
	}
	public static void LOAD_UGUI_SCENE_HIDE(int id, int renderOrder)
	{
		LOAD_UGUI_SCENE(id, renderOrder, false, false, null);
	}
	public static void LOAD_UGUI_SCENE_SHOW(int id, int renderOrder)
	{
		LOAD_UGUI_SCENE(id, renderOrder, true, false, null);
	}
	public static void LOAD_UGUI_TOP_HIDE(int id)
	{
		LOAD_UGUI(id, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, false, false, null);
	}
	public static void LOAD_UGUI_HIDE(int id)
	{
		LOAD_UGUI(id, 0, LAYOUT_ORDER.AUTO, false, false, null);
	}
	public static void LOAD_UGUI_HIDE(int id, int renderOrder, LAYOUT_ORDER orderType)
	{
		LOAD_UGUI(id, renderOrder, orderType, false, false, null);
	}
	public static void LOAD_UGUI_TOP_SHOW(int id)
	{
		LOAD_UGUI(id, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO, true, false, null);
	}
	public static void LOAD_UGUI_SHOW(int id)
	{
		LOAD_UGUI(id, 0, LAYOUT_ORDER.AUTO, true, false, null);
	}
	public static void LOAD_UGUI_SHOW(int id, int renderOrder, LAYOUT_ORDER orderType)
	{
		LOAD_UGUI(id, renderOrder, orderType, true, false, null);
	}
	public static void LOAD_UGUI_SHOW(int id, int renderOrder, LAYOUT_ORDER orderType, bool immediately)
	{
		LOAD_UGUI(id, renderOrder, orderType, true, immediately, null);
	}
	public static void LOAD_UGUI_SHOW(int id, int renderOrder, LAYOUT_ORDER orderType, bool immediately, string param)
	{
		LOAD_UGUI(id, renderOrder, orderType, true, immediately, param);
	}
	#endregion
	#region LAYOUT_VISIBLE
	public static void HIDE_LAYOUT(int id, bool immediately = false, string param = null)
	{
		VISIBLE_LAYOUT(id, false, immediately, param);
	}
	public static void HIDE_LAYOUT_FORCE(int id)
	{
		VISIBLE_LAYOUT_FORCE(id, false);
	}
	public static void SHOW_LAYOUT(int id, bool immediately = false, string param = null)
	{
		VISIBLE_LAYOUT(id, true, immediately, param);
	}
	public static void SHOW_LAYOUT_FORCE(int id)
	{
		VISIBLE_LAYOUT_FORCE(id, true);
	}
	public static void VISIBLE_LAYOUT(int id, bool visible, bool immediately = false, string param = null)
	{
		CMD(out CmdLayoutManagerVisible cmd);
		cmd.mLayoutID = id;
		cmd.mForce = false;
		cmd.mVisible = visible;
		cmd.mImmediately = immediately;
		cmd.mParam = param;
		pushCommand(cmd, mLayoutManager);
	}
	public static void VISIBLE_LAYOUT_FORCE(int id, bool visible)
	{
		CMD(out CmdLayoutManagerVisible cmd);
		cmd.mLayoutID = id;
		cmd.mForce = true;
		cmd.mVisible = visible;
		cmd.mImmediately = false;
		pushCommand(cmd, mLayoutManager);
	}
	public static CmdLayoutManagerVisible HIDE_LAYOUT_DELAY(DelayCmdWatcher watcher, float delayTime, int id, bool immediately = false, string param = null)
	{
		return VISIBLE_LAYOUT_DELAY(watcher, delayTime, id, false, immediately, param);
	}
	public static CmdLayoutManagerVisible HIDE_LAYOUT_DELAY_FORCE(DelayCmdWatcher watcher, float delayTime, int id)
	{
		return VISIBLE_LAYOUT_DELAY_FORCE(watcher, delayTime, id, false);
	}
	public static CmdLayoutManagerVisible SHOW_LAYOUT_DELAY(DelayCmdWatcher watcher, float delayTime, int id, bool immediately = false, string param = null)
	{
		return VISIBLE_LAYOUT_DELAY(watcher, delayTime, id, true, immediately, param);
	}
	public static CmdLayoutManagerVisible SHOW_LAYOUT_DELAY_EX(DelayCmdWatcher watcher, float delayTime, int id, CommandCallback start, bool immediately = false, string param = null)
	{
		return VISIBLE_LAYOUT_DELAY_EX(watcher, delayTime, id, true, start, immediately, param);
	}
	public static CmdLayoutManagerVisible SHOW_LAYOUT_DELAY_FORCE(DelayCmdWatcher watcher, float delayTime, int id)
	{
		return VISIBLE_LAYOUT_DELAY_FORCE(watcher, delayTime, id, true);
	}
	public static CmdLayoutManagerVisible VISIBLE_LAYOUT_DELAY(DelayCmdWatcher watcher, float delayTime, int id, bool visible, bool immediately = false, string param = null)
	{
		return VISIBLE_LAYOUT_DELAY_EX(watcher, delayTime, id, visible, null, immediately, param);
	}
	public static CmdLayoutManagerVisible VISIBLE_LAYOUT_DELAY_EX(DelayCmdWatcher watcher, float delayTime, int id, bool visible, CommandCallback start, bool immediately = false, string param = null)
	{
		CMD_DELAY(out CmdLayoutManagerVisible cmd);
		cmd.mLayoutID = id;
		cmd.mForce = false;
		cmd.mVisible = visible;
		cmd.mImmediately = immediately;
		cmd.mParam = param;
		cmd.addStartCommandCallback(start);
		pushDelayCommand(cmd, mLayoutManager, delayTime, watcher);
		return cmd;
	}
	public static CmdLayoutManagerVisible VISIBLE_LAYOUT_DELAY_FORCE(DelayCmdWatcher watcher, float delayTime, int type, bool visible)
	{
		CMD_DELAY(out CmdLayoutManagerVisible cmd);
		cmd.mLayoutID = type;
		cmd.mForce = true;
		cmd.mVisible = visible;
		cmd.mImmediately = false;
		pushDelayCommand(cmd, mLayoutManager, delayTime, watcher);
		return cmd;
	}
	#endregion
	#region UNLOAD
	public static void UNLOAD_LAYOUT(int id)
	{
		// 需要首先强制隐藏布局
		HIDE_LAYOUT_FORCE(id);
		CmdLayoutManagerUnload.execute(id);
	}
	#endregion
	#endregion
	#region 窗口的显示和隐藏
	public static CmdWindowActive ACTIVE_DELAY(DelayCmdWatcher watcher, myUIObject obj, bool active)
	{
		return ACTIVE_DELAY_EX(watcher, obj, active, 0.001f, null);
	}
	public static CmdWindowActive ACTIVE_DELAY(DelayCmdWatcher watcher, myUIObject obj, bool active, float delayTime)
	{
		return ACTIVE_DELAY_EX(watcher, obj, active, delayTime, null);
	}
	public static CmdWindowActive ACTIVE_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, bool active, float delayTime, CommandCallback startCallback)
	{
		CMD_DELAY(out CmdWindowActive cmd, LOG_LEVEL.LOW);
		cmd.mActive = active;
		cmd.addStartCommandCallback(startCallback);
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
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
	public static CmdWindowFill FILL_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, float start, float target, float time)
	{
		return FILL_DELAY_EX(watcher, obj, delayTime, start, target, time, null);
	}
	public static CmdWindowFill FILL_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, float start, float target, float time, KeyFrameCallback doneCallback)
	{
		CMD_DELAY(out CmdWindowFill cmd, LOG_LEVEL.LOW);
		cmd.mStartValue = start;
		cmd.mTargetValue = target;
		cmd.mOnceLength = time;
		cmd.mKeyframe = KEY_CURVE.ZERO_ONE;
		cmd.mDoneCallback = doneCallback;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
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
	public static CmdWindowAlpha ALPHA_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, float alpha)
	{
		CMD_DELAY(out CmdWindowAlpha cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartAlpha = alpha;
		cmd.mTargetAlpha = alpha;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
	public static CmdWindowAlpha ALPHA_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, float start, float target, float onceLength)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CmdWindowAlpha ALPHA_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, float start, float target, float onceLength)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CmdWindowAlpha ALPHA_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, float start, float target, float onceLength, bool loop)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static CmdWindowAlpha ALPHA_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static CmdWindowAlpha ALPHA_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CmdWindowAlpha ALPHA_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CmdWindowAlpha ALPHA_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CmdWindowAlpha ALPHA_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用CommandWindowAlpha ALPHA_DELAY(IDelayCmdWatcher watcher, myUIObject obj, float delayTime, float alpha)");
			return null;
		}
		CMD_DELAY(out CmdWindowAlpha cmd, LOG_LEVEL.LOW);
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
	public static CmdWindowColor COLOR_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Color color)
	{
		CMD_DELAY(out CmdWindowColor cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartColor = color;
		cmd.mTargetColor = color;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
	}
	public static CmdWindowColor COLOR_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Color start, Color target, float onceLength)
	{
		return COLOR_DELAY_EX(watcher, obj, delayTime, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CmdWindowColor COLOR_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, Color start, Color target, float onceLength)
	{
		return COLOR_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CmdWindowColor COLOR_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, Color start, Color target, float onceLength, bool loop)
	{
		return COLOR_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static CmdWindowColor COLOR_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, Color start, Color target, float onceLength, bool loop, float offset)
	{
		return COLOR_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static CmdWindowColor COLOR_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Color start, Color target, float onceLength, KeyFrameCallback doneCallback)
	{
		return COLOR_DELAY_EX(watcher, obj, delayTime, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CmdWindowColor COLOR_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Color start, Color target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return COLOR_DELAY_EX(watcher, obj, delayTime, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CmdWindowColor COLOR_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, Color start, Color target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return COLOR_DELAY_EX(watcher, obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CmdWindowColor COLOR_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, int keyframe, Color start, Color target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用CommandWindowAlpha ALPHA_DELAY(IDelayCmdWatcher watcher, myUIObject obj, float delayTime, float alpha)");
			return null;
		}
		CMD_DELAY(out CmdWindowColor cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartColor = start;
		cmd.mTargetColor = target;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushDelayCommand(cmd, obj, delayTime, watcher);
		return cmd;
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
		pushCommand<CmdWindowAlphaPath>(obj, LOG_LEVEL.LOW);
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
	public static void ALPHA_PATH_EX(myUIObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdWindowAlphaPath cmd, LOG_LEVEL.LOW);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayCommand<CmdWindowAlphaPath>(watcher, obj, delayTime, LOG_LEVEL.LOW);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, loop, offset, null, null);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPH_PATH_DELAY_EX(watcher, obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, doingCallback, doneCallback);
	}
	public static CmdWindowAlphaPath ALPH_PATH_DELAY_EX(DelayCmdWatcher watcher, myUIObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CMD_DELAY(out CmdWindowAlphaPath cmd, LOG_LEVEL.LOW);
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