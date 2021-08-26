using UnityEngine;
using System.Collections.Generic;

// 以指定的关键帧路径移动物体
public class CmdTransformableMovePath : Command
{
	public Dictionary<float, Vector3> mValueKeyFrame;	// 时间与位置的关键帧列表
	public KeyFrameCallback mDoingCallBack;				// 移动中回调
	public KeyFrameCallback mDoneCallBack;				// 移动完成时回调
	public Vector3 mValueOffset;						// 位置偏移,计算出的位置会再加上这个偏移作为最终世界坐标
	public float mOffset;								// 起始时间偏移
	public float mSpeed;								// 移动速度
	public int mKeyframe;								// 所使用的关键帧ID
	public bool mLoop;									// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mValueKeyFrame = null;
		mDoingCallBack = null;
		mDoneCallBack = null;
		mValueOffset = Vector3.zero;
		mOffset = 0.0f;
		mSpeed = 1.0f;
		mLoop = false;
		mKeyframe = KEY_CURVE.NONE;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if (mValueKeyFrame != null && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out COMTransformableMovePath com);
		com.setDoingCallback(mDoingCallBack);
		com.setDoneCallback(mDoneCallBack);
		com.setActive(true);
		com.setValueKeyFrame(mValueKeyFrame);
		com.setSpeed(mSpeed);
		com.setValueOffset(mValueOffset);
		com.play(mKeyframe, mLoop, mOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(", mOffset:", mOffset).
				append(", mLoop:", mLoop);
	}
}