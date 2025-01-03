using System;

// 用于在界面显示枚举的名字
public class EnumLabelAttribute : Attribute
{
	protected string mLabel;	// 枚举名字
	public EnumLabelAttribute(string label)
	{
		mLabel = label;
	}
	public string getLabel() { return mLabel; }
}