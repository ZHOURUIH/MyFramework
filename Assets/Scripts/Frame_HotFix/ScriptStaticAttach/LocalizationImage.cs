using UnityEngine;
using UnityEngine.UI;
using static FrameBaseHotFix;
using static FrameBaseDefine;
using static FrameDefine;

// 用于挂到静态的文本上,也就是只是在界面显示,不会在代码中访问和操作的文本
// 如果是会在代码中访问操作的文本对象则不需要挂此脚本
[RequireComponent(typeof(Image))]
public class LocalizationImage : MonoBehaviour
{
	protected UGUIAtlasPtr mAtlasPtr;
    public string mImageNameWithoutSuffix;
	public Image mImage;
    private void Start()
    {
		if (mImage == null)
		{
			TryGetComponent(out mImage);
		}
		if (mImage == null)
        {
			Debug.LogError("找不到Image组件:" + gameObject.name);
			return;
        }
		if (!TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
		{
			Debug.LogError("找不到图集,请添加ImageAtlasPath组件, window:" + gameObject.name);
			return;
		}
		string atlasPath = imageAtlasPath.mAtlasPath.removeStartString(P_GAME_RESOURCES_PATH);
		mAtlasPtr = mAtlasManager?.getAtlas(atlasPath, false);
		updateVariable();
		mLocalizationManager?.registeAction(onLanguageChanged);
		onLanguageChanged();
	}
	private void OnValidate()
	{
		if (!Application.isPlaying)
        {
			if (mImage == null)
			{
				TryGetComponent(out mImage);
			}
			if (mImage == null)
			{
                return;
            }
			updateVariable();
		}
	}
	private void OnDestroy()
    {
		mAtlasManager?.unloadAtlas(ref mAtlasPtr);
		mLocalizationManager?.unregisteAction(onLanguageChanged);
    }
    private void onLanguageChanged()
    {
        if (mImage != null && mLocalizationManager != null)
        {
			mImage.sprite = mAtlasPtr.getSprite(mImageNameWithoutSuffix + "_" + mLocalizationManager.getCurrentLanguage());
		}
    }
	protected void updateVariable()
	{
		if (mImage.sprite == null)
		{
			Debug.LogError("未指定图片:" + mImage.name, gameObject);
		}
		mImageNameWithoutSuffix = mImage.sprite.name;
		if (!mImageNameWithoutSuffix.endWith("_" + LANGUAGE_CHINESE))
		{
			Debug.LogError("图片名需要以_" + LANGUAGE_CHINESE + "结尾,GameObject:" + name, gameObject);
		}
		mImageNameWithoutSuffix = mImageNameWithoutSuffix.removeEndString("_" + LANGUAGE_CHINESE);
	}
}