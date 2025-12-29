
public class FilterTreeNodeParam
{
	public OBJECT_ITEM mObjectType;
	public ushort mEquipType;
	public PLAYER_JOB mJob;
}

public class FilterTreeNode : UGUITreeNode
{
	protected myUGUITextTMP mText;
	protected myUGUIObject mSelectMark;
	protected FilterTreeNodeParam mParam = new();
	public FilterTreeNode(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal()
	{
		newObject(out mText, "Text");
		newObject(out mSelectMark, "SelectMark");
	}
	public override void init()
	{
		base.init();
		mSelectMark.setActive(false);
	}
	public void setText(string text)
	{
		mText.setText(text);
	}
	public void setParam(OBJECT_ITEM objectType, ushort equipType, PLAYER_JOB job) 
	{
		mParam.mEquipType = equipType;
		mParam.mObjectType = objectType;
		mParam.mJob = job;
	}
	public FilterTreeNodeParam getPram() { return mParam; }
	public override void setSelect(bool select)
	{
		base.setSelect(select);
		mSelectMark.setActive(select);
	}
	public override void setExpand(bool expand)
	{
		base.setExpand(expand);
		foreach (UGUITreeNode item in mChildNodeList)
		{
			item.setActive(expand);
		}
	}
}