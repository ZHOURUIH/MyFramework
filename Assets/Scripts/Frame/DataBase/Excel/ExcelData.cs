using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExcelData : FrameBase
{
	public int mID;
	public virtual void read(SerializerRead reader)
	{
		reader.read(out mID);
	}
}