using UnityEngine;
using System.Collections.Generic;

public class KeyInfo : FrameBase
{
	public object mListener;
	public OnKeyCurrentDown mCallback;
	public KeyCode mKey;
	public bool mCtrlDown;
	public bool mShiftDown;
	public bool mAltDown;
}