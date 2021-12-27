using UnityEngine;
using System.Collections.Generic;

// 按键信息
public class KeyInfo : FrameBase
{
	public OnKeyCurrentDown mCallback;		// 按键回调
	public object mListener;				// 监听者
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