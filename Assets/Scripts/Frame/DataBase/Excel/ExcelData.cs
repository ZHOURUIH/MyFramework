using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExcelData : FrameBase
{
	public int mID;
	public override void resetProperty()
	{
		base.resetProperty();
		mID = 0;
	}
	public virtual void read(SerializerRead reader)
	{
		reader.read(out mID);
	}
}