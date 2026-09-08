using UnityEngine;

// FastUI的Batch本身已经把Material和Texture作为两个独立渲染状态处理。
// TMP多Atlas页只需要复用原TMP Material，并由FastText/FastTextAtlasRun各自的RenderTexture绑定具体Atlas页。
public static class FastUITextMaterialVariantCache
{
	public static void refresh(Material baseMaterial)
	{
		FastUITextRenderMaterialCache.refresh(baseMaterial);
		FastUIMaskMaterialCache.refresh(baseMaterial);
	}
}
