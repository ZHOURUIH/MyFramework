using UnityEngine;
using System.Collections.Generic;

public class KeyInfo : FrameBase
{
	public OnKeyCurrentDown mCallback;
	public object mListener;
	public bool mShiftDown;
	public bool mCtrlDown;
	public bool mAltDown;
	public KeyCode mKey;
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