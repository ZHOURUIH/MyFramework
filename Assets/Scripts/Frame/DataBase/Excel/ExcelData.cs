using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Excel表格导出的数据基类,表示表格中的一条数据
public class ExcelData : FrameBase
{
	public int mID;		// 每一条数据的唯一ID
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