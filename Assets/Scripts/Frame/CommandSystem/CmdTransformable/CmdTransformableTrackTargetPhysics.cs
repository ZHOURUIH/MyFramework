using System;
using UnityEngine;

public class CmdTransformableTrackTargetPhysics : Command
{
	public Transformable mTarget;
	public TrackCallback mDoingCallback;
	public TrackCallback mDoneCallback;
	public Vector3 mOffset;
	public float mSpeed;
	public override void resetProperty()
	{
		base.resetProperty();
		mTarget = null;
		mDoneCallback = null;
		mDoingCallback = null;
		mOffset = Vector3.zero;
		mSpeed = 0.0f;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
#if UNITY_EDITOR
		if (obj is myUIObject)
		{
			var uiObj = obj as myUIObject;
			if (mTarget != null && !uiObj.getLayout().canUIObjectUpdate(uiObj))
			{
				logError("想要使窗口播放缓动动画,但是窗口当前未开启更新");
			}
		}
#endif
		obj.getComponent(out ComponentTrackTargetPhysics com);
		com.setSpeed(mSpeed);
		com.setTargetOffset(mOffset);
		com.setActive(true);
		com.setMoveDoneTrack(mTarget, mDoneCallback);
		com.setTrackingCallback(mDoingCallback);
		// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
		obj.setEnable(true);
	}
}