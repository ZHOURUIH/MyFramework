using UnityEngine;

[ExecuteAlways]
// 序列帧精灵渲染预览,在编辑器中预览SpriteRenderer序列帧动画
public class SequenceSpriteRendererPreview : SequenceSpritePreviewBase
{
#if UNITY_EDITOR
	protected SpriteRenderer mRenderer;
	public override void Awake()
	{
		base.Awake();
		mRenderer = GetComponentInChildren<SpriteRenderer>();
	}
	protected override Component getSpriteComponent()
	{
		return mRenderer;
	}
#endif
}
