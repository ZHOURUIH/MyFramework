using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static StringUtility;
using static MathUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameBaseHotFix;

// 专用于显示伤害数字的窗口,使用一个窗口即可同时绘制几千个伤害数字
public class myUGUIDamageNumber : myUGUIObject
{
	protected DamageNumberRenderer mRenderer;			// 渲染组件
	protected UGUIAtlasPtr mOriginAtlasPtr = new();		// 初始的图片图集,用于卸载,当前类只关心初始图集的卸载,后续再次设置的图集不关心是否需要卸载,需要外部设置的地方自己关心
	protected UGUIAtlasPtr mAtlasPtr = new();			// 当前正在使用的图片图集
	protected Sprite mOriginSprite;                     // 备份加载物体时原始的精灵图片
	protected string mNumberStyle;						// 数字图集名
	public override void init()
	{
		base.init();
		// 获取image组件,如果没有则添加,这样是为了使用代码新创建一个image窗口时能够正常使用image组件
		if (!mObject.TryGetComponent(out mRenderer))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个DamageNumberCanvasRenderer组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mRenderer = mObject.AddComponent<DamageNumberRenderer>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}

		mOriginSprite = mRenderer.mImage;
		// 获取初始的精灵所在图集
		if (mOriginSprite != null)
		{
			if (!mObject.TryGetComponent<ImageAtlasPath>(out var imageAtlasPath))
			{
				logError("找不到图集,请添加ImageAtlasPath组件, window:" + mName + ", layout:" + mLayout.getName());
			}
			string atlasPath = imageAtlasPath.mAtlasPath;
			if (atlasPath.isEmpty())
			{
				logError("ImageAtlasPath中记录的路径为空,GameObject:" + getGameObjectPath(mObject));
			}
			// unity_builtin_extra是unity内置的资源,不需要再次加载
			if (!atlasPath.endWith("/unity_builtin_extra"))
			{
				if (mLayout.isInResources())
				{
					atlasPath = atlasPath.removeStartString(P_RESOURCES_PATH);
					mOriginAtlasPtr = mAtlasManager.getAtlasInResources(atlasPath, false);
				}
				else
				{
					atlasPath = atlasPath.removeStartString(P_GAME_RESOURCES_PATH);
					mOriginAtlasPtr = mAtlasManager.getAtlas(atlasPath, false);
				}
				if (mOriginAtlasPtr == null || !mOriginAtlasPtr.isValid())
				{
					logWarning("无法加载初始化的图集:" + atlasPath + ", window:" + mName + ", layout:" + mLayout.getName() +
						",请确保ImageAtlasPath中记录的图片路径正确,记录的路径:" + (imageAtlasPath != null ? imageAtlasPath.mAtlasPath : EMPTY));
				}
			}
			mAtlasPtr = mOriginAtlasPtr;
			mNumberStyle = mRenderer.mImage.name.rangeToLast('_');
			refreshSpriteList();
		}
	}
	public override void notifyAnchorApply()
	{
		base.notifyAnchorApply();
		// 此处默认数字窗口都是以ASPECT_BASE.AB_AUTO的方式等比放大
		mRenderer.setInterval(mRenderer.getInterval() * getScreenScale(ASPECT_BASE.AUTO).x);
	}
	public override void cloneFrom(myUGUIObject obj)
	{
		base.cloneFrom(obj);
		var source = obj as myUGUIDamageNumber;
		mRenderer.setInterval(source.mRenderer.getInterval());
		mNumberStyle = source.mNumberStyle;
		mRenderer.cloneDamageList(source.mRenderer.getDamageItems());
		mRenderer.setNumberSpriteList(source.mRenderer.getSpriteList());
		mRenderer.setDocking(source.getDocking());
	}
	public void setAtlas(UGUIAtlasPtr atlas)
	{
		mAtlasPtr = atlas;
		refreshSpriteList();
	}
	public void setNumberStyle(string style)
	{
		mNumberStyle = style;
		refreshSpriteList();
	}
	public float getNumberTotalWidth(long num)
	{
		using var a = new ListScope<byte>(out var list);
		splitNumber(num, list);
		return list.count() * (mRenderer.getNumberWidth() + mRenderer.getInterval()) - mRenderer.getInterval();
	}
	// pos是屏幕上的坐标,数字和标记一起显示,extraFlags中的对象需要从对象池中创建
	public void addDamageNumber(Vector3 pos, float scale, long num, bool showNumber, List<DamageNumberFlag> extraFlags, float speed, 
						  Dictionary<float, Vector3> positionKeyFrames, Dictionary<float, Vector3> scaleKeyFrames) 
	{
		CLASS(out DamageNumberData data);
		if (showNumber)
		{
			splitNumber(num, data.mNumbers);
		}
		extraFlags.moveTo(data.mExtraFlags);
		data.setPositionKeyframes(positionKeyFrames);
		data.setScaleKeyframes(scaleKeyFrames);
		data.mPositionOffset = pos;
		data.mScaleOffset = new(scale, scale, scale);
		data.mCurTime = 0.0f;
		data.mSpeed = speed;
		data.mTotalWidth = data.mNumbers.count() * (mRenderer.getNumberWidth() + mRenderer.getInterval()) - mRenderer.getInterval();
		data.init();
		mRenderer.addDamageNumber(data); 
	}
	// 包含数字和一个标记,extraFlag需要从对象池中创建
	public void addDamageNumber(Vector3 pos, float scale, long num, DamageNumberFlag extraFlag, float speed,
						  Dictionary<float, Vector3> positionKeyFrames, Dictionary<float, Vector3> scaleKeyFrames)
	{
		CLASS(out DamageNumberData data);
		splitNumber(num, data.mNumbers);
		data.mExtraFlags.add(extraFlag);
		data.setPositionKeyframes(positionKeyFrames);
		data.setScaleKeyframes(scaleKeyFrames);
		data.mPositionOffset = pos;
		data.mScaleOffset = new(scale, scale, scale);
		data.mCurTime = 0.0f;
		data.mSpeed = speed;
		data.mTotalWidth = data.mNumbers.count() * (mRenderer.getNumberWidth() + mRenderer.getInterval()) - mRenderer.getInterval();
		data.init();
		mRenderer.addDamageNumber(data);
	}
	// 不含标记的纯数字
	public void addDamageNumber(Vector3 pos, float scale, long num, float speed, Dictionary<float, Vector3> positionKeyFrames, Dictionary<float, Vector3> scaleKeyFrames)
	{
		CLASS(out DamageNumberData data);
		splitNumber(num, data.mNumbers);
		data.setPositionKeyframes(positionKeyFrames);
		data.setScaleKeyframes(scaleKeyFrames);
		data.mPositionOffset = pos;
		data.mScaleOffset = new(scale, scale, scale);
		data.mCurTime = 0.0f;
		data.mSpeed = speed;
		data.mTotalWidth = data.mNumbers.count() * (mRenderer.getNumberWidth() + mRenderer.getInterval()) - mRenderer.getInterval();
		data.init();
		mRenderer.addDamageNumber(data);
	}
	// 只包含一个标记,不含数字,extraFlag需要从对象池中创建
	public void addDamageNumber(Vector3 pos, float scale, DamageNumberFlag extraFlag, float speed,
						  Dictionary<float, Vector3> positionKeyFrames, Dictionary<float, Vector3> scaleKeyFrames)
	{
		CLASS(out DamageNumberData data);
		data.mExtraFlags.add(extraFlag);
		data.setPositionKeyframes(positionKeyFrames);
		data.setScaleKeyframes(scaleKeyFrames);
		data.mPositionOffset = pos;
		data.mScaleOffset = new(scale, scale, scale);
		data.mCurTime = 0.0f;
		data.mSpeed = speed;
		data.mTotalWidth = data.mNumbers.count() * (mRenderer.getNumberWidth() + mRenderer.getInterval()) - mRenderer.getInterval();
		data.init();
		mRenderer.addDamageNumber(data);
	}
	public void setInterval(int interval)						{ mRenderer.setInterval(interval); }
	public void setDocking(DOCKING_POSITION dock)				{ mRenderer.setDocking(dock); }
	public void clearNumber()									{ mRenderer.clearNumber(); }
	public float getInterval()									{ return mRenderer.getInterval(); }
	public string getNumberStyle()								{ return mNumberStyle; }
	public DOCKING_POSITION getDocking()						{ return mRenderer.getDocking(); }
	public Sprite getSprite(string name)						{ return mAtlasPtr.getSprite(name); }
	public Sprite getSpriteWithNumberStyle(string name)			{ return mAtlasPtr.getSprite(mNumberStyle + "_" + name); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshSpriteList()
	{
		using var a = new ListScope<DamageNumberSpriteData>(out var spriteList);
		for (int i = 0; i < 10; ++i)
		{
			DamageNumberSpriteData data = new();
			data.init(mAtlasPtr.getSprite(mNumberStyle + "_" + IToS(i)));
			data.mSearchKey = (char)('0' + i);
			spriteList.add(data);
		}
		mRenderer.mImage = spriteList.first().mSprite;
		mRenderer.setNumberSpriteList(spriteList);
	}
}