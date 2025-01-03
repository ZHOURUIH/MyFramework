
public class CharacterGame : Character
{
	protected COMCharacterController mController;
	protected CharacterGameData mData = new();
	public CharacterGame()
	{
		mIsMyself = true;
		mData = new();
	}
	public CharacterGameData getData() { return mData; }
	//---------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent(out mController, true);
	}
};