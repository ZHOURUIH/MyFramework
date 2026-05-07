using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(ImageAtlasPath))]
public class SequenceImagePreview : SequenceImagePreviewBase
{
#if UNITY_EDITOR
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
#endif
}
