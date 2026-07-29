using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(ImageAtlasPath))]
// 序列帧图片预览,在编辑器中预览图片序列帧动画
public class SequenceImagePreview : SequenceSpritePreviewBase
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
