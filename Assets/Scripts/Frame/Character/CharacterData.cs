using System;

public class CharacterData : FrameBase
{
	public string mName;
	public long mGUID;		// 玩家唯一ID,由服务器发送过来的
	public override void resetProperty()
	{
		base.resetProperty();
		mName = null;
		mGUID = 0;
	}
}