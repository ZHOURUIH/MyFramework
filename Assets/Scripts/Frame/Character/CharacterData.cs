using System;

// 角色数据的基类
public class CharacterData : ClassObject
{
	public string mName;	// 角色名
	public long mGUID;		// 玩家唯一ID,由服务器发送过来的
	public CharacterData()
	{}
	public override void resetProperty()
	{
		base.resetProperty();
		mName = null;
		mGUID = 0;
	}
}