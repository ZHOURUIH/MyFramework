using UnityEngine;
using System.Collections.Generic;

// 加载材质时的参数
public class LoadMaterialParam : ClassObject
{
	public string mMaterialName;		// 材质名
	public bool mNewMaterial;			// 是否创建新的材质对象
	public override void resetProperty()
	{
		base.resetProperty();
		mMaterialName = null;
		mNewMaterial = false;
	}
}