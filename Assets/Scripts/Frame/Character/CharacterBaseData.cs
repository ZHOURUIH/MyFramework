using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterBaseData : GameBase
{
	public CharacterBaseData()
	{
		mName = null;
		mGUID = 0;
	}
	public string mName;
	public uint	mGUID;		// 玩家唯一ID,由服务器发送过来的
}
