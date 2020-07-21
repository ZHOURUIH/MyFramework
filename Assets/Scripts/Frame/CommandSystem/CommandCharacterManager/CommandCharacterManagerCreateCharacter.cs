using UnityEngine;
using System.Collections;
using System;

public class CommandCharacterManagerCreateCharacter : Command
{
	public Type mCharacterType;
	public string mName;
	public bool mCreateNode;
	public uint mID;
	public override void init()
	{
		base.init();
		mCharacterType = null;
		mName = null;
		mCreateNode = true;
		mID = 0;
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
