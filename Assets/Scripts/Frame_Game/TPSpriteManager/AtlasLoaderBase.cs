using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using UObject = UnityEngine.Object;

public abstract class AtlasLoaderBase
{
	protected Dictionary<string, List<AtlasLoadParam>> mLoadRequestList = new();	// 图集异步加载请求列表
	protected Dictionary<string, List<AtlasLoadParam>> mLoadingList = new();		// 正在异步加载的图集列表
	protected SafeDictionary<string, UGUIAtlas> mAtlasList = new();					// 位于Resources中的图集,key是图集的路径,相对于Resources
	protected const int MAX_LOADING_COUNT = 20;							            // 同时加载的最大数量
	public void destroyAll()
	{
		foreach (UGUIAtlas item in mAtlasList.getMainList().Values)
		{
			baseUnloadAtlas(item, false);
		}
		mAtlasList.clear();
		mLoadingList.Clear();
	}
	public void update()
	{
		// 检查是否有需要卸载的图集
		if (mAtlasList.count() > 0)
		{
			using var a = new SafeDictionaryReader<string, UGUIAtlas>(mAtlasList);
			foreach (var item in a.mReadList)
			{
				if (item.Value.getReferenceCount() == 0 && allowUnloadAtlas(item.Key))
				{
					baseUnloadAtlas(item.Value, true);
					item.Value.destroy();
					mAtlasList.remove(item.Key);
				}
			}
		}

		do
		{
			// 同一时间只能允许一定数量的图集正在加载
			if (mLoadRequestList.Count == 0)
			{
				break;
			}
			var first = mLoadRequestList.First();
			var requestList = first.Value;
			if (requestList.Count == 0)
			{
				break;
			}

			string atlasName = first.Key;
			var paramList = mLoadingList.get(atlasName);
			// 请求加载一个新的图集
			if (paramList == null)
			{
				// 如果当前正在加载的图集数量已经达到上限,则不能再开始加载,等待空闲的时候再加入列表
				if (mLoadingList.Count >= MAX_LOADING_COUNT)
				{
					break;
				}
				LIST_PERSIST(out paramList);
				mLoadingList.Add(atlasName, paramList.addRange(requestList));
				// 开始加载一个新的图集
				baseLoadAtlasAsync(atlasName, (UObject asset, UObject[] assets, byte[] _, string loadPath) =>
				{
					// 根据图集名字找到加载该图集的所有请求,然后通知这些请求,如果不在加载中列表,则可能是中途意外中断了,不再继续执行
					if (!mLoadingList.Remove(loadPath, out var paramList))
					{
						return;
					}
					if (!mAtlasList.tryGetValue(loadPath, out UGUIAtlas atlas))
					{
						atlas = atlasLoaded(assets, asset, loadPath);
						if (atlas != null)
						{
							mAtlasList.add(loadPath, atlas);
						}
					}
					foreach (AtlasLoadParam param in paramList)
					{
						if (atlas == null && param.mErrorIfNull)
						{
							logError("图集加载失败:" + loadPath);
						}
						CLASS(out UGUIAtlasPtr ptr).setAtlas(atlas);
						param.mCallback?.Invoke(ptr);
						UN_CLASS(param);
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
	public SafeDictionary<string, UGUIAtlas> getAtlasList() { return mAtlasList; }
	// 获取位于Resources目录中的图集,如果不存在则可以选择是否同步加载
	public UGUIAtlasPtr getAtlas(string atlasName, bool errorIfNull, bool loadIfNull)
	{
		if (mAtlasList.tryGetValue(atlasName, out UGUIAtlas atlasPtr))
		{
			CLASS(out UGUIAtlasPtr ptr).setAtlas(atlasPtr);
			return ptr;
		}
		if (loadIfNull)
		{
			UObject[] sprites = baseLoadSubResource(atlasName, out UObject mainAsset, errorIfNull);
			UGUIAtlas atlas = atlasLoaded(sprites, mainAsset, atlasName);
			if (atlas != null)
			{
				mAtlasList.add(atlasName, atlas);
				CLASS(out UGUIAtlasPtr ptr).setAtlas(atlas);
				return ptr;
			}
		}
		return null;
	}
	// 异步加载位于Resources中的图集
	public CustomAsyncOperation getAtlasAsync(string atlasName, UGUIAtlasPtrCallback callback, bool errorInNull, bool loadIfNull)
	{
		CustomAsyncOperation op = new();
		if (mAtlasList.tryGetValue(atlasName, out UGUIAtlas atlas))
		{
			CLASS(out UGUIAtlasPtr ptr).setAtlas(atlas);
			callback?.Invoke(ptr);
			op.mFinish = true;
			return op;
		}
		if (loadIfNull)
		{
			mLoadRequestList.tryGetOrAddListPersist(atlasName).Add(CLASS(out AtlasLoadParam param));
			param.mName = atlasName;
			param.mCallback = (UGUIAtlasPtr atlas) =>
			{
				callback?.Invoke(atlas);
				op.mFinish = true;
			};
			param.mInResources = true;
			param.mErrorIfNull = errorInNull;
		}
		return op;
	}
	// 卸载图集
	public bool unloadAtlas(UGUIAtlasPtr atlasPtr)
	{
		if (atlasPtr == null || !atlasPtr.isValid() || atlasPtr.getToken() == 0)
		{
			return false;
		}
		// 正在加载中的图集无法卸载
		string atlasName = atlasPtr.getName();
		// 可能是热更完毕图集管理器清除以后再卸载的,找不到也正常
		if (!mAtlasList.tryGetValue(atlasName, out UGUIAtlas atlas))
		{
			return false;
		}
		
		if (atlasPtr.getAtlas() != atlas)
		{
			logError("要卸载的图集不一致:" + atlasName);
		}
		atlasPtr.unuse();
		return true;
	}
	// 图集资源已经加载完成,从assets中解析并创建图集信息
	public static UGUIAtlas atlasLoaded<T>(T[] assets, UObject mainAsset, string loadPath) where T : UObject
	{
		if (assets.isEmpty())
		{
			return null;
		}
		UGUIAtlas atlas = new();
		atlas.mFilePath = loadPath;
		atlas.mMainAsset = mainAsset;
		// 找到Texture,位置不一定是第一个,需要遍历查找
		foreach (T item in assets)
		{
			// 找出所有的精灵
			if (item is Sprite sprite)
			{
				atlas.mSpriteList.Add(item.name, sprite);
			}
			// 找到Texture2D
			else if (item is Texture2D tex2D)
			{
				atlas.mTexture = tex2D;
			}
		}
		if (atlas.mTexture == null)
		{
			logError("can not find texture2D in loaded Texture:" + loadPath);
		}
		return atlas;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract void baseUnloadAtlas(UGUIAtlas atlas, bool showError);
	protected abstract CustomAsyncOperation baseLoadAtlasAsync(string name, AssetLoadDoneCallback doneCallback);
	protected abstract UObject[] baseLoadSubResource(string atlasName, out UObject mainAsset, bool errorIfNull);
	protected virtual bool allowUnloadAtlas(string atlasName) { return true; }
}