using System;
using UnityEngine;

// 按键监听信息
// 记录按键回调、监听者、组合键状态,由InputSystem维护,支持Ctrl/Shift/Alt等组合键判定
public class KeyListenInfo : ClassObject
{
	public Action mCallback;				// 按键回调
	public IEventListener mListener;		// 监听者
	public COMBINATION_KEY mCombinationKey;	// 指定可组合的键是否按下
	public KeyCode mKey;					// 按键值
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mListener = null;
		mCombinationKey = COMBINATION_KEY.NONE;
		mKey = KeyCode.None;
	}
}