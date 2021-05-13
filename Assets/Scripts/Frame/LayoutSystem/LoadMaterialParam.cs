using UnityEngine;
using System.Collections.Generic;

public class LoadMaterialParam : FrameBase
{
	public string mMaterialName;
	public bool mNewMaterial;
	public override void resetProperty()
	{
		base.resetProperty();
		mMaterialName = null;
		mNewMaterial = false;
	}
}