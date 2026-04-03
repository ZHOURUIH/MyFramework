#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SequenceImagePreview : SequenceImagePreviewBase
{
	protected Image mImage;
	public override void Awake()
	{
		base.Awake();
		mImage = GetComponentInChildren<Image>();
	}
	protected override Component getSpriteComponent()
	{
		return mImage;
	}
}

#endif