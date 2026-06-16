using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(ImageAtlasPath))]
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
