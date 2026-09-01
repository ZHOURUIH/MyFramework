using UnityEngine;

[ExecuteAlways]
[ExecuteInEditMode]
[RequireComponent(typeof(ImageAtlasPath))]
// 序列帧精灵渲染预览,在编辑器中预览SpriteRenderer序列帧动画
public class SequenceSpriteRendererPreview : SequenceSpritePreviewBase
{
#if UNITY_EDITOR
	protected SpriteRenderer mRenderer;
	protected override Component getSpriteComponent()
	{
		if (mRenderer == null)
		{
			mRenderer = GetComponentInChildren<SpriteRenderer>();
		}
		return mRenderer;
	}
#endif
}
