using System;
using System.Collections.Generic;

public class LabelAttribute : Attribute
{
	protected string mLabel;
	public LabelAttribute(string label)
	{
		mLabel = label;
	}
	public string getLabel() { return mLabel; }
}