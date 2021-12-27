using System;

// 预设加载信息,只用于传参用
public class PrefabLoadParam : FrameBase
{
	public CreateObjectCallback mCallback;	// 加载回调
	public object mUserData;				// 回调参数
	public int mTag;						// 物体标签
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mUserData = null;
		mTag = 0;
	}
}