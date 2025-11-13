using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 缩放物体
public class CmdTransformableScale
{
	// 缩放中回调
	// 缩放完成时回调
	// 起始缩放值
	// 目标缩放值
	// 单次所需时间
	// 起始时间偏移
	// 所使用的关键帧ID
	// 是否循环
	public static void execute(ITransformable obj, Vector3 startScale, Vector3 targetScale, float onceLength, float offset, int keyframe, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor() && 
			obj is myUGUIObject uiObj && 
			!isFloatZero(onceLength) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableScale com);
		com.setDoingCallback(doingCallback);
		com.setDoneCallback(doneCallback);
		com.setActive(true);
		com.setStart(startScale);
		com.setTarget(targetScale);
		com.play(keyframe, loop, onceLength, offset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public static void execute(ITransformable obj, Vector3 startScale)
	{
		if (obj == null)
		{
			return;
		}
		obj.getComponent(out COMTransformableScale com);
		if (com == null || !com.isActive())
		{
			obj.setScale(startScale);
			return;
		}
		com.setStart(startScale);
		com.setTarget(startScale);
		com.play(0, false, 0.0f, 0.0f);
	}
}