// auto generate start
using System;
using System.Collections.Generic;
using UnityEngine;

// 成就表
public class EDAchivement : ExcelDataT<EDAchivement>
{
	public string mTestString;						// 成就名字
	public string mTestInt;							// 成就简单描述
	public string mTestFloat;						// 成就详细描述
	public int mTestList1;							// 完成成就所需的值,此值应该与Condition中的值一致,只是方便获取
	public int mReward;								// 完成成就可获得的奖励,索引到Reward表
	public override bool read(SerializerRead reader)
	{
		bool result = base.read(reader);
		result = result && reader.readString(out mTestString);
		result = result && reader.readString(out mTestInt);
		result = result && reader.readString(out mTestFloat);
		result = result && reader.read(out mTestList1);
		result = result && reader.read(out mReward);
		return result;
	}
}
// auto generate end