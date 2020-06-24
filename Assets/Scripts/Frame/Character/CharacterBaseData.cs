using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterBaseData : GameBase
{
	public CharacterBaseData()
	{
		mGUID = 0;
		mName = EMPTY_STRING;
	}
	public uint		mGUID;		// 玩家唯一ID,由服务器发送过来的
	public string	mName;
}
