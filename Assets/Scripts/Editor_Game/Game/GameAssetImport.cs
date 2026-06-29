using UnityEditor;

public class GameAssetImport : AssetPostprocessor
{
	// 图片的导入,UIAtlas中的图片都是Single的Sprite
	public void OnPreprocessTexture()
	{
		//if (!assetPath.startWith(P_UI_ATLAS_PATH))
		//{
		//	return;
		//}
		//var importer = assetImporter as TextureImporter;
		//if (importer == null)
		//{
		//	return;
		//}
		//bool needReimport = false;
		//if (importer.textureType != TextureImporterType.Sprite)
		//{
		//	importer.textureType = TextureImporterType.Sprite;
		//	needReimport = true;
		//}
		//if (importer.spriteImportMode != SpriteImportMode.Single)
		//{
		//	importer.spriteImportMode = SpriteImportMode.Single;
		//	needReimport = true;
		//}
		//if (importer.mipmapEnabled)
		//{
		//	importer.mipmapEnabled = false;
		//	needReimport = true;
		//}
		//if (needReimport)
		//{
		//	importer.SaveAndReimport();
		//}
	}
}