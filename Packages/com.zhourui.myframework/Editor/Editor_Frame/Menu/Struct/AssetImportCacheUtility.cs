#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class AssetImportCacheUtility
{
	[MenuItem("Assets/强制清除资源导入缓存", false, 3000)]
	private static void clearSelectedAssetImportCache()
	{
		foreach (UnityEngine.Object obj in Selection.objects)
		{
			string assetPath = AssetDatabase.GetAssetPath(obj);
			if (assetPath.isEmpty())
			{
				continue;
			}

			clearAssetImportCache(assetPath);
		}
	}

	public static void clearAssetImportCache(string assetPath)
	{
		AssetImporter importer = AssetImporter.GetAtPath(assetPath);
		if (importer == null)
		{
			Debug.LogError("找不到AssetImporter:" + assetPath);
			return;
		}

		// Sprite Multiple需要额外清理子资源映射。
		if (importer is TextureImporter textureImporter &&
			textureImporter.spriteImportMode == SpriteImportMode.Multiple)
		{
			clearSpriteData(textureImporter);
		}

		// 强制重新导入指定资源。
		AssetDatabase.ImportAsset(
			assetPath,
			ImportAssetOptions.ForceUpdate |
			ImportAssetOptions.ForceSynchronousImport);

		Debug.Log("已强制清除并重新导入资源:" + assetPath);
	}

	private static void clearSpriteData(TextureImporter textureImporter)
	{
		SpriteDataProviderFactories factory = new();
		factory.Init();

		ISpriteEditorDataProvider dataProvider =
			factory.GetSpriteEditorDataProviderFromObject(textureImporter);

		if (dataProvider == null)
		{
			Debug.LogError(
				"无法获取SpriteDataProvider:" +
				textureImporter.assetPath);
			return;
		}

		dataProvider.InitSpriteEditorDataProvider();

		// 清除所有旧Sprite Rect。
		dataProvider.SetSpriteRects(Array.Empty<SpriteRect>());

		// 清除Sprite名称和FileID之间的历史映射。
		ISpriteNameFileIdDataProvider nameFileIdProvider =
			dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();

		if (nameFileIdProvider != null)
		{
			nameFileIdProvider.SetNameFileIdPairs(
				Array.Empty<SpriteNameFileIdPair>());
		}

		dataProvider.Apply();

		// 先让“清空状态”真正落盘并完成一次导入。
		textureImporter.SaveAndReimport();
	}
}
#endif