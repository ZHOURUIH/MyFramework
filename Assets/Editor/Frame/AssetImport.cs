using UnityEditor;
using UnityEngine;

public class AssetsImport : AssetPostprocessor
{
	//所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
	public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string str in importedAsset)
		{
			if (str == FrameDefine.P_ASSET_BUNDLE_PATH + FrameDefine.ILR_FILE)
			{
				Debug.Log("热更dll已经更新");
				break;
			}
		}
	}
	// 导入模型前调用
	public void OnPreprocessModel()
	{
		var modelImporter = assetImporter as ModelImporter;
#if UNITY_2018
		modelImporter.importMaterials = false;
#else
		modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
		modelImporter.sortHierarchyByName = true;
#endif
		modelImporter.importCameras = false;
		modelImporter.importLights = false;
		modelImporter.importBlendShapes = false;
		modelImporter.importVisibility = false;
		modelImporter.preserveHierarchy = true;
		modelImporter.meshCompression = ModelImporterMeshCompression.High;
		modelImporter.weldVertices = true;
		modelImporter.importBlendShapeNormals = ModelImporterNormals.None;
	}
}