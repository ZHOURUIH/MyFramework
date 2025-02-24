using static FrameUtility;

public class UIDemoStart : LayoutScript
{
	protected myUGUIObject mMask;
	public override void assignWindow()
	{
		newObject(out mMask, "Mask");
	}
	public override void init()
	{
		mMask.registeCollider(onMaskClick);
	}
	//--------------------------------------------------------------------------------------------------------------------------
	protected void onMaskClick()
	{
		changeProcedure<StartSceneDemo>();
	}
}