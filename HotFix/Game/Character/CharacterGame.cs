using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterGame : Character
{
	protected CharacterGameData mData;
	protected COMCharacterController mController;
	public CharacterGameData getData() { return mData; }
	//---------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		mController = addComponent(typeof(COMCharacterController), true) as COMCharacterController;
	}
	protected override CharacterBaseData createCharacterData()
	{
		return mData = new CharacterGameData();
	}
};