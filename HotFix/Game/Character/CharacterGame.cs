using System;

public class CharacterGame : Character
{
	protected COMCharacterController mController;
	protected CharacterGameData mData;
	public CharacterGameData getData() { return mData; }
	//---------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		mController = addComponent(typeof(COMCharacterController), true) as COMCharacterController;
	}
	protected override CharacterData createCharacterData()
	{
		return mData = new CharacterGameData();
	}
};