using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 渐变一个窗口的颜色
public class CmdWindowColor
{
	// 变化中回调
	// 变化完成时回调
	// 起始颜色
	// 目标颜色
	// 单次所需时间
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(myUGUIObject obj, Color startColor, Color targetColor, float onceLength, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			!isFloatZero(onceLength) && 
			!obj.getLayout().canUIObjectUpdate(obj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + obj.getName());
		}
		obj.getOrAddComponent(out COMWindowColor com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStart(startColor);
		com.setTarget(targetColor);
		com.play(keyframe, loop, onceLength, offset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(myUGUIObject obj, Color startColor)
	{
		if (obj == null)
		{
			return;
		}
		obj.getOrAddComponent(out COMWindowColor com);
		com.setStart(startColor);
		com.setTarget(startColor);
		com.play(0, false, 0.0f, 0.0f);
	}
}