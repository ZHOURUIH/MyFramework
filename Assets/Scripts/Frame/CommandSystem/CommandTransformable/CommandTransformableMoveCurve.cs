using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandTransformableMoveCurve : Command
{
	public List<Vector3> mPosList;
	public KeyFrameCallback mTremblingCallBack;
	public KeyFrameCallback mTrembleDoneCallBack;
	public string mName;
	public float mOnceLength;
	public float mAmplitude;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void init()
	{
		base.init();
		mPosList = null;
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mName = null;
		mOnceLength = 1.0f;
		mAmplitude = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		Transformable obj = mReceiver as Transformable;
		TransformableComponentMoveCurve component = obj.getComponent(out component);
		// 停止其他移动组件
		obj.breakComponent<IComponentModifyPosition>(Typeof(component));
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setKeyPosList(mPosList);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + 
			", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}