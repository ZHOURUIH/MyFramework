using UnityEditor;
using UnityEngine;

public class AssetsImport : AssetPostprocessor
{
	//所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
	public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string str in importedAsset)
		{
			if (str == FrameDefine.P_STREAMING_ASSETS_PATH + FrameDefine.ILR_FILE)
			{
				Debug.Log("热更dll已经更新");
			}
		}
	}
}