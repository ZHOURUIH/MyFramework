using UnityEngine;
using System;
using static FrameBaseUtility;
using static UnityUtility;
using static MathUtility;

// 以指定的位置列表进行移动
public class CmdTransformableMoveCurveSpan
{
	public static void execute(ITransformable obj, Span<Vector3> posList, float onceLength, float timeOffset, int keyframe, bool loop, KeyFrameCallback doing, KeyFrameCallback done)
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
		obj.getOrAddComponent(out COMTransformableMoveCurve com);
		com.setDoingCallback(doing);
		com.setDoneCallback(done);
		com.setActive(true);
		com.setKeyList(posList);
		com.play(keyframe, loop, onceLength, timeOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
}