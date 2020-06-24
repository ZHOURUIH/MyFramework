using UnityEngine;
using System.Collections;
using System;

public class CommandCharacterManagerCreateCharacter : Command
{
	public Type mCharacterType;
	public uint mID;
	public string mName;
	public bool mCreateNode;
	public override void init()
	{
		base.init();
		mCharacterType = null;
		mName = EMPTY_STRING;
		mID = 0;
		mCreateNode = true;
	}
	public override void execute()
	{
		if(mCharacterType == null)
		{
			return;
		}
		if (mID == 0)
		{
			mID = generateGUID();
		}
		mCharacterManager.createCharacter(mName, mCharacterType, mID, mCreateNode);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mCharacterType:" + mCharacterType.ToString() + ", mID:" + mID;
	}
}
