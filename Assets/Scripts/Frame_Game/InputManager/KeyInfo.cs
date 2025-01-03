using System;
using UnityEngine;

// 按键信息
public class KeyInfo : ClassObject
{
	public Action mCallback;				// 按键回调
	public COMBINATION_KEY mCombinationKey;	// 指定可组合的键是否按下
	public KeyCode mKey;					// 按键值
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mCombinationKey = COMBINATION_KEY.NONE;
		mKey = KeyCode.None;
	}
}