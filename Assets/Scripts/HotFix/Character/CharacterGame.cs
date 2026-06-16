
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
    public override void resetProperty()
    {
        base.resetProperty();
		mController = null;
		mData.resetProperty();
    }
	//---------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent(out mController, true);
	}
};