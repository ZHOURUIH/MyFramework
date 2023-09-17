using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using UObject = UnityEngine.Object;

public abstract class AtlasLoaderBase
{
	protected Dictionary<string, List<AtlasLoadParam>> mLoadRequestList;	// 图集异步加载请求列表
	protected Dictionary<string, List<AtlasLoadParam>> mLoadingList;		// 正在异步加载的图集列表
	protected Dictionary<string, UGUIAtlas> mAtlasList;						// 位于Resources中的图集,key是图集的路径,相对于Resources
	protected const int MAX_LOADING_COUNT = 20;                             // 同时加载的最大数量
	public AtlasLoaderBase()
	{
		mLoadRequestList = new Dictionary<string, List<AtlasLoadParam>>();
		mLoadingList = new Dictionary<string, List<AtlasLoadParam>>();
		mAtlasList = new Dictionary<string, UGUIAtlas>();
	}
	public void destroyAll()
	{
		foreach (var item in mAtlasList)
		{
			baseUnloadAtlas(item.Value, false);
		}
		mAtlasList.Clear();
		mLoadingList.Clear();
	}
	public void update()
	{
		// 检查是否有需要卸载的图集
		if (mAtlasList.Count > 0)
		{
			List<string> temp = null;
			foreach (var item in mAtlasList)
			{
				if (item.Value.getReferenceCount() == 0 && allowUnloadAtlas(item.Key))
				{
					baseUnloadAtlas(item.Value, true);
					item.Value.destroy();
					if (temp == null)
					{
						LIST(out temp);
					}
					temp.Add(item.Key);
				}
			}
			if (temp != null)
			{
				foreach (string item in temp)
				{
					mAtlasList.Remove(item);
				}
				UN_LIST(ref temp);
			}
		}

		do
		{
			// 同一时间只能允许一定数量的图集正在加载
			if (mLoadRequestList.Count == 0)
			{
				break;
			}
			var iter = mLoadRequestList.GetEnumerator();
			iter.MoveNext();
			var requestList = iter.Current.Value;
			if (requestList.Count == 0)
			{
				break;
			}

			string atlasName = iter.Current.Key;
			mLoadingList.TryGetValue(atlasName, out List<AtlasLoadParam> paramList);
			// 请求加载一个新的图集
			if (paramList == null)
			{
				// 如果当前正在加载的图集数量已经达到上限,则不能再开始加载,等待空闲的时候再加入列表
				if (mLoadingList.Count >= MAX_LOADING_COUNT)
				{
					break;
				}
				LIST_PERSIST(out paramList);
				mLoadingList.Add(atlasName, paramList);
				paramList.AddRange(requestList);
				// 开始加载一个新的图集
				baseLoadAtlasAsync(atlasName, (UObject asset, UObject[] assets, byte[] bytes, object userData, string loadPath) =>
				{
					// 根据图集名字找到加载该图集的所有请求,然后通知这些请求,如果不在加载中列表,则可能是中途意外中断了,不再继续执行
					if (!mLoadingList.TryGetValue(loadPath, out List<AtlasLoadParam> paramList))
					{
						return;
					}
					mLoadingList.Remove(loadPath);
					if (!mAtlasList.TryGetValue(loadPath, out UGUIAtlas atlas))
					{
						atlas = atlasLoaded(assets, asset, loadPath);
						if (atlas != null)
						{
							mAtlasList.Add(loadPath, atlas);
						}
					}
					int count = paramList.Count;
					for (int i = 0; i < count; ++i)
					{
						AtlasLoadParam param = paramList[i];
						if (atlas == null && param.mErrorIfNull)
						{
							logError("图集加载失败:" + loadPath);
						}
						param.mCallback?.Invoke(new UGUIAtlasPtr(atlas), param.mUserData);
						UN_CLASS(ref param);
					}
					UN_LIST(ref paramList);
				});
			}
			// 请求的图集正在加载,则只是将请求数据添加到回调列表中等待加载结束即可
			else
			{
				paramList.AddRange(requestList);
			}
			// 处理加载请求后就可以从请求列表中移除
			UN_LIST(ref requestList);
			mLoadRequestList.Remove(atlasName);
		} while (false);
	}
#if UNITY_EDITOR
	public Dictionary<string, UGUIAtlas> getAtlasList() { return mAtlasList; }
#endif
	// 通过此方法获得的图片将无法被卸载,因为外部始终无法拿不到图集凭证来卸载,所以尽量避免使用
	public Sprite getSprite(string atlasName, string spriteName, bool errorIfNull = true, bool loadIfNull = true)
	{
		UGUIAtlasPtr atlas = getAtlas(atlasName, errorIfNull, loadIfNull);
		return atlas.getSprite(spriteName);
	}
	// 获取位于Resources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlas(string atlasName, bool errorIfNull, bool loadIfNull)
	{
		if (mAtlasList.TryGetValue(atlasName, out UGUIAtlas atlasPtr))
		{
			return new UGUIAtlasPtr(atlasPtr);
		}
		if (loadIfNull)
		{
			UObject[] sprites = baseLoadSubResource(atlasName, out UObject mainAsset, errorIfNull);
			UGUIAtlas atlas = atlasLoaded(sprites, mainAsset, atlasName);
			if (atlas != null)
			{
				mAtlasList.Add(atlasName, atlas);
				return new UGUIAtlasPtr(atlas);
			}
		}
		return UGUIAtlasPtr.Default;
	}
	// 异步加载位于Resources中的图集
	public void getAtlasAsync(string atlasName, AtlasLoadDone callback, object userData, bool errorInNull, bool loadIfNull)
	{
		if (mAtlasList.TryGetValue(atlasName, out UGUIAtlas atlas))
		{
			callback?.Invoke(new UGUIAtlasPtr(atlas), userData);
			return;
		}
		if (loadIfNull)
		{
			if (!mLoadRequestList.TryGetValue(atlasName, out List<AtlasLoadParam> paramList))
			{
				LIST_PERSIST(out paramList);
				mLoadRequestList.Add(atlasName, paramList);
			}
			CLASS(out AtlasLoadParam param);
			param.mName = atlasName;
			param.mCallback = callback;
			param.mInResources = true;
			param.mUserData = userData;
			param.mErrorIfNull = errorInNull;
			paramList.Add(param);
		}
	}
	// 卸载图集
	public void unloadAtlas(ref UGUIAtlasPtr atlasPtr)
	{
		if (!atlasPtr.isValid() || atlasPtr.mToken == 0)
		{
			return;
		}
		// 正在加载中的图集无法卸载
		string atlasName = atlasPtr.getName();
		// 可能是热更完毕图集管理器清除以后再卸载的,找不到也正常
		if (!mAtlasList.TryGetValue(atlasName, out UGUIAtlas atlas))
		{
			return;
		}
		
		if (atlasPtr.mAtlas != atlas)
		{
			logError("要卸载的图集不一致:" + atlasName);
		}
		atlasPtr.unuse();
	}
	//---------------------------------------------------------------------------------------------------------------------------
	protected abstract void baseUnloadAtlas(UGUIAtlas atlas, bool showError);
	protected abstract void baseLoadAtlasAsync(string name, AssetLoadDoneCallback doneCallback);
	protected abstract UObject[] baseLoadSubResource(string atlasName, out UObject mainAsset, bool errorIfNull);
	protected virtual bool allowUnloadAtlas(string atlasName) { return true; }
	// 图集资源已经加载完成,从assets中解析并创建图集信息
	protected UGUIAtlas atlasLoaded<T>(T[] assets, UObject mainAsset, string loadPath) where T : UObject
	{
		if (assets == null || assets.Length == 0)
		{
			return null;
		}
		UGUIAtlas atlas = new UGUIAtlas();
		atlas.mFilePath = loadPath;
		atlas.mMainAsset = mainAsset;
		// 找到Texture,位置不一定是第一个,需要遍历查找
		int count = assets.Length;
		for (int i = 0; i < count; ++i)
		{
			T item = assets[i];
			// 找出所有的精灵
			if (item is Sprite)
			{
				atlas.mSpriteList.Add(item.name, item as Sprite);
			}
			// 找到Texture2D
			else if (item is Texture2D)
			{
				atlas.mTexture = item as Texture2D;
			}
		}
		if (atlas.mTexture == null)
		{
			logError("can not find texture2D in loaded Texture:" + loadPath);
		}
		return atlas;
	}
}