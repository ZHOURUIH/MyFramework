﻿using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;
using static FrameBaseUtility;

// 以指定的位置列表进行移动
public class CmdTransformableMoveCurve : Command
{
	public List<Vector3> mPosList;			// 位置列表
	public KeyFrameCallback mDoingCallback;	// 移动中回调
	public KeyFrameCallback mDoneCallback;	// 移动完成时回调
	public float mOnceLength;				// 单次所需时间
	public float mOffset;					// 起始时间偏移
	public int mKeyframe;					// 所使用的关键帧ID
	public bool mLoop;						// 是否循环
	public override void resetProperty()
	{
		base.resetProperty();
		mPosList = null;
		mDoingCallback = null;
		mDoneCallback = null;
		mKeyframe = KEY_CURVE.NONE;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mLoop = false;
	}
	public override void execute()
	{
		var obj = mReceiver as Transformable;
		if (isEditor() && 
			obj is myUIObject uiObj &&
			!isFloatZero(mOnceLength) && 
			!uiObj.getLayout().canUIObjectUpdate(uiObj))
		{
			logError("想要使窗口播放缓动动画,但是窗口当前未开启更新:" + uiObj.getName());
		}
		obj.getOrAddComponent(out COMTransformableMoveCurve com);
		com.setDoingCallback(mDoingCallback);
		com.setDoneCallback(mDoneCallback);
		com.setActive(true);
		com.setKeyList(mPosList);
		com.play(mKeyframe, mLoop, mOnceLength, mOffset);
		if (com.getState() == PLAY_STATE.PLAY)
		{
			// 需要启用组件更新时,则开启组件拥有者的更新,后续也不会再关闭
			obj.setNeedUpdate(true);
		}
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mKeyframe:", mKeyframe).
				append(", mOnceLength:", mOnceLength).
				append(", mOffset:", mOffset).
				append(", mLoop:", mLoop);
	}
}