using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommandTransformableRotateCurve : Command
{
	public List<Vector3> mRotList;
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
		mRotList = null;
		mTremblingCallBack = null;
		mTrembleDoneCallBack = null;
		mName = EMPTY_STRING;
		mOnceLength = 1.0f;
		mAmplitude = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		ComponentOwner obj = mReceiver as ComponentOwner;
		TransformableComponentRotateCurve component = obj.getComponent(out component);
		// 停止其他旋转组件
		obj.breakComponent<IComponentModifyRotation>(component.GetType());
		component.setTremblingCallback(mTremblingCallBack);
		component.setTrembleDoneCallback(mTrembleDoneCallBack);
		component.setActive(true);
		component.setKeyRotList(mRotList);
		component.play(mName, mLoop, mOnceLength, mOffset, mFullOnce, mAmplitude);		
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mOnceLength:" + mOnceLength + ", mOffset:" + mOffset + 
			", mLoop:" + mLoop + ", mAmplitude:" + mAmplitude + ", mFullOnce:" + mFullOnce;
	}
}