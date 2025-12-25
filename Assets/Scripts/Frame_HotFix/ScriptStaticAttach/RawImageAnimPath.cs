using UnityEngine;
using UnityEngine.UI;
using static FrameBaseUtility;
using static FrameDefine;
using static StringUtility;
using static FileUtility;

// 用于记录Image组件上的图片所在的路径,因为在运行时是没办法获得Image上图片的路径,从而也就无法直到所在的图集
// 所以使用一个组件来在编辑模式下就记录路径
[ExecuteInEditMode]
public class RawImageAnimPath : MonoBehaviour
{
	public string mTexturePath;		// 序列帧所在的目录,相对于GameResources,不带文件名,以/结尾
	public string mTextureName;		// 序列帧图片名前缀,不带_
	public int mImageCount;			// 图片序列帧的图片数量,图片的名字中的下标应该从0开始
	public bool mRefresh = true;    // 是否刷新,当作刷新按钮使用
	public bool mAutoRefresh = true;// 是否自动刷新,可能会影响编辑时的性能
	public void Awake()
	{
		enabled = !Application.isPlaying;
		refreshPath();
	}
	private void OnValidate()
	{
		if(mRefresh)
		{
			refreshPath();
			mRefresh = false;
		}
	}
	public void Update()
	{
		if (mAutoRefresh)
		{
			refreshPath();
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshPath()
	{
		if (!isEditor() || Application.isPlaying)
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
		// 去除Assets/GameResoureces前缀
		mTexturePath = getFilePath(imagePathName, true).removeStartString(P_GAME_RESOURCES_PATH);
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