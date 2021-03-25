using UnityEngine;
using System.Collections.Generic;

public class CommandTransformableMoveCurve : Command
{
	public List<Vector3> mPosList;
	public KeyFrameCallback mDoingCallback;
	public KeyFrameCallback mDoneCallback;
	public KEY_FRAME mKeyframe;
	public float mOnceLength;
	public float mOffset;
	public bool mFullOnce;
	public bool mLoop;
	public override void resetProperty()
	{
		base.resetProperty();
		mPosList = null;
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_FRAME.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mFullOnce = false;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		obj.getComponent(out TransformableComponentMoveCurve component);
		component.setTremblingCallback(mDoingCallback);
		component.setTrembleDoneCallback(mDoneCallback);
		component.setActive(true);
		component.setKeyList(mPosList);
		component.play((int)mKeyframe, mLoop, mOnceLength, mOffset, mFullOnce);
		if (component.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setEnable(true);
		}
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mKeyframe:", mKeyframe.ToString()).
				Append(", mOnceLength:", mOnceLength).
				Append(", mOffset:", mOffset).
				Append(", mLoop:", mLoop).
				Append(", mFullOnce:", mFullOnce);
	}
}