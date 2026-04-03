#if UNITY_EDITOR
using UnityEngine;

[ExecuteAlways]
public class SequenceSpritePreview : SequenceImagePreviewBase
{
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
}

#endif