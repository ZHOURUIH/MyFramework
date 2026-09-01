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
	protected override Component getSpriteComponent()
	{
		if (mImage == null)
		{
			mImage = GetComponentInChildren<Image>();
		}
		return mImage;
	}
#endif
}