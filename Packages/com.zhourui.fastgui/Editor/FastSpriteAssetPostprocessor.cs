using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Refreshes FastSpriteRenderer geometry after Sprite Editor / TextureImporter changes.
// The delay waits until Unity finishes remapping imported Sprite objects.
public sealed class FastSpriteAssetPostprocessor : AssetPostprocessor
{
	private static readonly HashSet<string> sImportedPaths = new();
	private static bool sQueued;

	private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
	{
		if (!string.IsNullOrEmpty(assetPath))
		{
			sImportedPaths.Add(assetPath);
		}
		if (sQueued)
		{
			return;
		}
		sQueued = true;
		EditorApplication.delayCall += refresh;
	}

	private static void refresh()
	{
		sQueued = false;
		if (sImportedPaths.Count == 0)
		{
			return;
		}

		FastSpriteRenderSystem.clearSpriteMeshCache();
		FastSpriteRenderer[] renderers =
			Resources.FindObjectsOfTypeAll<FastSpriteRenderer>();
		for (int i = 0; i < renderers.Length; ++i)
		{
			FastSpriteRenderer renderer = renderers[i];
			if (renderer == null || EditorUtility.IsPersistent(renderer))
			{
				continue;
			}
			Sprite sprite = renderer.getSprite();
			if (sprite == null || !sImportedPaths.Contains(AssetDatabase.GetAssetPath(sprite)))
			{
				continue;
			}
			renderer.refreshAll();
		}
		sImportedPaths.Clear();
		EditorApplication.QueuePlayerLoopUpdate();
		SceneView.RepaintAll();
	}
}
