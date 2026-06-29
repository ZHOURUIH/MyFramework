using UnityEngine;

[ExecuteAlways]
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
