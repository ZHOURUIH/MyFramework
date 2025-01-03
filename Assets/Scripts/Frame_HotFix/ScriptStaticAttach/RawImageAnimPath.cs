using UnityEngine;
using UnityEngine.UI;
using static FrameEditorUtility;
using static FrameDefine;
using static StringUtility;
using static FileUtility;

// 用于记录Image组件上的图片所在的路径,因为在运行时是没办法获得Image上图片的路径,从而也就无法直到所在的图集
// 所以使用一个组件来在编辑模式下就记录路径
public class RawImageAnimPath : MonoBehaviour
{
	public string mTexturePath;		// 序列帧所在的目录,相对于GameResources,不带文件名,以/结尾
	public string mTextureName;		// 序列帧图片名前缀,不带_
	public int mImageCount;			// 图片序列帧的图片数量,图片的名字中的下标应该从0开始
	public bool mRefresh = true;	// 是否刷新,当作刷新按钮使用
	private void OnValidate()
	{
		if(mRefresh)
		{
			refreshPath();
			mRefresh = false;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshPath()
	{
		if (!isEditor())
		{
			return;
		}
		if (!TryGetComponent<RawImage>(out var rawImage))
		{
			Debug.LogError("can not find RawImage Component");
			return;
		}
		string imagePathName = getAssetPath(rawImage.mainTexture);
		mTextureName = getFileNameNoSuffixNoDir(imagePathName).rangeToLast('_');
		mTexturePath = getFilePath(imagePathName, true);
		// 去除Assets/GameResoureces前缀
		removeStartString(ref mTexturePath, P_GAME_RESOURCES_PATH);
		// 获取图片数量
		string suffix = getFileSuffix(imagePathName);
		string preString = F_GAME_RESOURCES_PATH + mTexturePath + mTextureName + "_";
		int index = 0;
		while(true)
		{
			if (!isFileExist(preString + IToS(index) + suffix))
			{
				break;
			}
			++index;
		}
		mImageCount = index;
	}
}