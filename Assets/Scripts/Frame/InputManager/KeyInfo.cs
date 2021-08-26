using UnityEngine;
using System.Collections.Generic;

// 按键信息
public class KeyInfo : FrameBase
{
	public OnKeyCurrentDown mCallback;		// 按键回调
	public object mListener;				// 监听者
	public bool mShiftDown;					// shift是否按下
	public bool mCtrlDown;					// ctrl是否按下
	public bool mAltDown;					// alt是否按下
	public KeyCode mKey;					// 按键值
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mListener = null;
		mShiftDown = false;
		mCtrlDown = false;
		mAltDown = false;
		mKey = KeyCode.None;
	}
}