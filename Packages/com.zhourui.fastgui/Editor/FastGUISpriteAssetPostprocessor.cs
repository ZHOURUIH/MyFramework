using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class FastGUISpriteAssetPostprocessor : AssetPostprocessor
{
	private static bool sRefreshQueued;
	private static readonly HashSet<string> sImportedSpritePaths = new();

	private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
	{
		if (!string.IsNullOrEmpty(assetPath))
		{
			sImportedSpritePaths.Add(assetPath);
		}
		if (sRefreshQueued)
		{
			return;
		}
		sRefreshQueued = true;
		EditorApplication.delayCall += refreshImportedSprites;
	}

	private static void refreshImportedSprites()
	{
		sRefreshQueued = false;
		if (sImportedSpritePaths.Count == 0)
		{
			return;
		}

		FastImage[] images = Resources.FindObjectsOfTypeAll<FastImage>();
		HashSet<FastCanvas> affectedCanvases = new();

		for (int i = 0; i < images.Length; ++i)
		{
			FastImage image = images[i];
			if (image == null || EditorUtility.IsPersistent(image))
			{
				continue;
			}

			Sprite sprite = image.getSprite();
			if (sprite == null || !sImportedSpritePaths.Contains(AssetDatabase.GetAssetPath(sprite)))
			{
				continue;
			}

			image.refreshEditorSpriteAsset();
			FastCanvas canvas = image.getCanvas();
			if (canvas != null)
			{
				affectedCanvases.Add(canvas);
			}
		}
		sImportedSpritePaths.Clear();

		foreach (FastCanvas canvas in affectedCanvases)
		{
			if (canvas == null || !canvas.isActiveAndEnabled)
			{
				continue;
			}

			FastUIMeshRenderer renderer = canvas.getMeshRenderer();
			if (renderer != null)
			{
				if (!renderer.gameObject.activeSelf)
				{
					renderer.gameObject.SetActive(true);
				}
				renderer.setRenderVisible(canvas.getVisible());
			}

			canvas.refreshDescendantBindings();

			renderer = canvas.getMeshRenderer();
			if (renderer != null)
			{
				if (!renderer.gameObject.activeSelf)
				{
					renderer.gameObject.SetActive(true);
				}
				renderer.setRenderVisible(canvas.getVisible());
				renderer.forceBatchTextureRebind();
			}

			canvas.flushFrameNow();
		}

		SceneView.RepaintAll();
		EditorApplication.QueuePlayerLoopUpdate();
	}
}
