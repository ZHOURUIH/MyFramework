using UnityEngine;

// 用于在检视面本将变量名显示为另外的文字
public class CustomLabelAttribute : PropertyAttribute
{
	protected string mLabel;		// 显示的文字
	public CustomLabelAttribute(string label)
	{
		mLabel = label;
	}
	public string getLabel() { return mLabel; }
}