using UnityEngine;
using static StringUtility;
using static FrameBaseHotFix;
using static MathUtility;
using static UnityUtility;
using static FrameDefine;
using static FrameBaseUtility;

// 对UGUI的Image的封装,包含全部封装功能,UGUI的静态图片不支持递归变化透明度
public class myUGUIImagePro : myUGUIImage, IShaderWindow
{
	protected WindowShader mWindowShader;					// 图片所使用的shader类,用于动态设置shader参数
	protected CanvasGroup mCanvasGroup;						// 用于是否显示
	protected Material mOriginMaterial;						// 初始的材质,用于重置时恢复材质
	protected string mOriginMaterialPath;					// 初始的材质路径
	protected bool mIsNewMaterial;							// 当前的材质是否是新建的材质对象
	protected bool mCanvasGroupValid;						// 当前CanvasGroup是否有效,在测试中发现判断mCanvasGroup是否为空的写法会比较耗时,所以替换为bool判断
	public override void init()
	{
		base.init();
		string materialName = getMaterialName().removeAll(" (Instance)");
		// 不再将默认材质替换为自定义的默认材质,只判断其他材质
		if (!materialName.isEmpty() &&
			materialName != BUILDIN_UI_MATERIAL)
		{
			if (mOriginMaterial != null && mObject.TryGetComponent<MaterialPath>(out var comMaterialPath))
			{
				mOriginMaterialPath = comMaterialPath.mMaterialPath;
			}
			if (mOriginMaterialPath.isEmpty())
			{
				logError("没有找到MaterialPath组件,name:" + getName());
			}
			mOriginMaterialPath = mOriginMaterialPath.removeStartString(P_GAME_RESOURCES_PATH);
			if (!mOriginMaterialPath.endWith("/unity_builtin_extra"))
			{
				if (!mOriginMaterialPath.Contains('.'))
				{
					logError("材质文件需要带后缀:" + mOriginMaterialPath + ",GameObject:" + getName() + ",parent:" + getParent()?.getName());
				}
				setMaterialName(mOriginMaterialPath, !mShaderManager.isSingleShader(mOriginMaterial.shader.name));
			}
		}
	}
	public override void destroy()
	{
		// 卸载创建出的材质
		if (mIsNewMaterial)
		{
			if (!isEditor())
			{
				destroyUnityObject(mImage.material);
			}
		}
		mImage.material = mOriginMaterial;
		if (mCanvasGroup != null)
		{
			mCanvasGroup.alpha = 1.0f;
			mCanvasGroup = null;
		}
		mCanvasGroupValid = false;
		base.destroy();
	}
	// 是否剔除渲染
	public void cull(bool isCull)
	{
		if (mCanvasGroup == null)
		{
			mCanvasGroup = getOrAddUnityComponent<CanvasGroup>();
		}
		mCanvasGroup.alpha = isCull ? 0.0f : 1.0f;
		mCanvasGroupValid = true;
	}
	public override bool isCulled() { return mCanvasGroupValid && isFloatZero(mCanvasGroup.alpha); }
	public override bool canUpdate() { return !isCulled() && base.canUpdate(); }
	public override bool canGenerateDepth() { return !isCulled(); }
	public void setWindowShader(WindowShader shader)
	{
		mWindowShader = shader;
		// 因为shader参数的需要在update中更新,所以需要启用窗口的更新
		mNeedUpdate = true;
	}
	public WindowShader getWindowShader() { return mWindowShader; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isCulled())
		{
			return;
		}
		if(mImage.material != null)
		{
			mWindowShader?.applyShader(mImage.material);
		}
	}
	public void setMaterialName(string materialName, bool newMaterial, bool loadAsync = false)
	{
		if(mImage == null)
		{ 
			return; 
		}
		mIsNewMaterial = newMaterial;
		// 异步加载
		if (loadAsync)
		{
			mResourceManager.loadGameResourceAsync(R_MATERIAL_PATH + materialName + ".mat", (Material mat) =>
			{
				if (mImage == null)
				{
					return;
				}
				if (mIsNewMaterial)
				{
					// 当需要复制一个新的材质时,刚加载出来的材质实际上就不会再用到了
					// 只有当下次还加载相同的材质时才会直接返回已加载的材质
					// 如果要卸载最开始加载出来的材质,只能通过卸载整个文件夹的资源来卸载
					Material newMat = new(mat);
					newMat.name = materialName + "_" + IToS(mID);
					mImage.material = newMat;
				}
				else
				{
					mImage.material = mat;
				}
			});
		}
		// 同步加载
		else
		{
			var loadedMaterial = mResourceManager.loadGameResource<Material>(R_MATERIAL_PATH + materialName + ".mat");
			if (mIsNewMaterial)
			{
				Material mat = new(loadedMaterial);
				mat.name = materialName + "_" + IToS(mID);
				mImage.material = mat;
			}
			else
			{
				mImage.material = loadedMaterial;
			}
		}
	}
}