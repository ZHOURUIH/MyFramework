using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static FrameDefine;
using static StringUtility;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;
using static FrameUtility;
using UObject = UnityEngine.Object;

// 用于管理SpriteAtlas和MultiSprite封装以后的对象
public class AtlasManager : FrameSystem
{
	protected Dictionary<string, List<AtlasLoadParam>> mLoadRequestList = new();    // 图集异步加载请求列表
	protected Dictionary<string, List<AtlasLoadParam>> mLoadingList = new();        // 正在异步加载的图集列表
	protected SafeDictionary<string, AtlasBase> mAtlasList = new();                 // key是图集的路径,相对于GameResources
	protected HashSet<string> mDontUnloadAtlas = new();                             // 即使没有引用也不会卸载的图集
	protected Dictionary<string, ResourceRef<SpriteAtlas>> mSpriteAtlasList = new();// 已加载的SpriteAtlas列表
	protected Dictionary<string, string> mAtlasPathList;							// 根据图集名字查找SpriteAtlas文件的路径
	protected float mCheckTimer;                                                    // 检查的计时器
	protected const int MAX_LOADING_COUNT = 20;                                     // 同时加载的最大数量
	protected const float CHECK_INTERVAL = 2.0f;                                    // 检查的间隔时间
	public AtlasManager()
	{
		if (isEditor())
		{
			mCreateObject = true;
		}
	}
	public override void init()
	{
		base.init();
		// 注册一下SpriteAtlas的回调,否则在真机上没办法自动加载SpriteAtlas中的Sprite
		SpriteAtlasManager.atlasRequested += onAtlasRequested;
		if (isEditor())
		{
			mObject.AddComponent<TPSpriteManagerDebug>();
		}
	}
	public override void destroy()
	{
		base.destroy();
		SpriteAtlasManager.atlasRequested -= onAtlasRequested;
	}
	public void destroyAll()
	{
		mAtlasList.forValue(item => item.destroy());
		mAtlasList.clear();
		mLoadingList.Clear();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 检查是否有需要卸载的图集
		if (mAtlasList.count() > 0 && tickTimerLoop(ref mCheckTimer, elapsedTime, CHECK_INTERVAL))
		{
			using var a = new SafeDictionaryReader<string, AtlasBase>(mAtlasList);
			foreach (var item in a.mReadList)
			{
				// 图集本身的引用为0,且其中的图片没有在任何地方被引用,并且这个图集不在不允许卸载的列表中,则可以卸载这个图集
				if (item.Value.getReferenceCount() == 0 && allowUnloadAtlas(item.Key))
				{
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
			var first = mLoadRequestList.first();
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
				LIST_PERSIST(out paramList, requestList);
				mLoadingList.Add(atlasName, paramList);
				// 开始加载一个新的图集
				mResourceManager.loadGameResourceAsync(atlasName, (ResourceRef<UObject> asset, UObject[] assets, byte[] _, string loadPath) =>
				{
					// 根据图集名字找到加载该图集的所有请求,然后通知这些请求,如果不在加载中列表,则可能是中途意外中断了,不再继续执行
					if (!mLoadingList.Remove(loadPath, out var paramList))
					{
						return;
					}
					if (!mAtlasList.tryGetValue(loadPath, out AtlasBase atlas))
					{
						atlas = atlasLoaded(assets, asset, loadPath);
						mAtlasList.addIf(loadPath, atlas, atlas != null);
					}
					foreach (AtlasLoadParam param in paramList)
					{
						if (atlas == null && param.mErrorIfNull)
						{
							logError("图集加载失败:" + loadPath);
						}
						CLASS(out AtlasRef ptr).setAtlas(atlas);
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
	public void addDontUnloadAtlas(string fileName) { mDontUnloadAtlas.Add(fileName); }
	public SafeDictionary<string, AtlasBase> getAtlasList() { return mAtlasList; }
	// 获取位于GameResources目录中的图集,如果不存在则可以选择是否同步加载
	public AtlasRef getAtlas(string atlasName, bool errorIfNull = true, bool loadIfNull = true)
	{
		if (mAtlasList.tryGetValue(atlasName, out AtlasBase atlasPtr))
		{
			CLASS(out AtlasRef ptr).setAtlas(atlasPtr);
			return ptr;
		}
		if (loadIfNull)
		{
			UObject[] sprites = mResourceManager.loadSubGameResource<UObject>(atlasName, out ResourceRef<UObject> mainAsset, errorIfNull);
			AtlasBase atlas = atlasLoaded(sprites, mainAsset, atlasName);
			if (atlas != null)
			{
				mAtlasList.add(atlasName, atlas);
				CLASS(out AtlasRef ptr).setAtlas(atlas);
				return ptr;
			}
		}
		return null;
	}
	// 异步加载位于GameResources中的图集,atlasName是GameResources下的相对路径,带后缀
	public CustomAsyncOperation getAtlasAsyncSafe(IRecyclable owner, string atlasName, AtlasPtrCallback callback, bool errorInNull = true, bool loadIfNull = true)
	{
		CustomAsyncOperation op = new();
		if (mAtlasList.tryGetValue(atlasName, out AtlasBase atlas))
		{
			CLASS(out AtlasRef ptr).setAtlas(atlas);
			callback?.Invoke(ptr);
			return op.setFinish();
        }
		if (loadIfNull)
		{
			long assignID = owner?.getAssignID() ?? 0;
			AtlasLoadParam param = mLoadRequestList.getOrAddListPersist(atlasName).addClass();
			param.mName = atlasName;
			param.mCallback = (AtlasRef atlas) =>
			{
				if (assignID == (owner?.getAssignID() ?? 0))
				{
					callback?.Invoke(atlas);
				}
				op.setFinish();
			};
			param.mErrorIfNull = errorInNull;
		}
		return op;
	}
	// atlasName是GameResources下的相对路径,带后缀
	// 一般来说很少使用,通常都是先获取图集对象,然后通过图集对象获取精灵,除非只是想快速获取一个精灵,又不想关心这个精灵所在的图集是什么
	public SpriteRef getSprite(string atlasName, string spriteName)
	{
		AtlasRef atlas = getAtlas(atlasName);
		Sprite sprite = atlas?.getSprite(spriteName);
		if (sprite == null)
		{
			return null;
		}
		CLASS(out SpriteRef ptr).setSprite(sprite, atlas);
		return ptr;
	}
	public void unloadSprite(ref SpriteRef ptr)
	{
		UN_CLASS(ref ptr);
	}
	// 卸载图集
	public bool unloadAtlas(AtlasRef atlasPtr)
	{
		if (atlasPtr == null || !atlasPtr.isValid())
		{
			return false;
		}
		// 正在加载中的图集无法卸载
		string atlasName = atlasPtr.getFilePath();
		// 可能是热更完毕图集管理器清除以后再卸载的,找不到也正常
		if (mAtlasList.tryGetValue(atlasName, out AtlasBase atlas) && atlasPtr.getAtlas() != atlas)
		{
			logError("要卸载的图集不一致:" + atlasName);
		}
		UN_CLASS(ref atlasPtr);
		return true;
	}
	public void unloadAtlas(ref AtlasRef atlasPtr)
	{
		unloadAtlas(atlasPtr);
		atlasPtr = null;
	}
	public void unloadAtlas(List<AtlasRef> atlasPtrList)
	{
		atlasPtrList.For(item => unloadAtlas(item));
		atlasPtrList.Clear();
	}
	public void unloadAtlas<Key>(Dictionary<Key, AtlasRef> atlasPtrList)
	{
		atlasPtrList.forValue(item => unloadAtlas(item));
		atlasPtrList.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onAtlasRequested(string name, Action<SpriteAtlas> action)
	{
		if (mAtlasPathList == null)
		{
			mAtlasPathList = new();
			var text = mResourceManager.loadGameResource<TextAsset>(R_MISC_PATH + ATLAS_PATH_CONFIG);
			foreach(string line in text.get().text.splitLine())
			{
				mAtlasPathList.Add(getFileNameNoSuffixNoDir(line), line);
			}
			mResourceManager.unload(ref text);
		}

		ResourceRef<SpriteAtlas> atlas = mSpriteAtlasList.get(name);
		if (atlas == null)
		{
			string path = mAtlasPathList.get(name);
			if (path == null)
			{
				Debug.LogError("在AtlasPathConfig中找不到指定名字的图集:" + name);
				action(null);
				return;
			}
			atlas = mResourceManager.loadGameResource<SpriteAtlas>(path);
			mSpriteAtlasList.Add(name, atlas);
		}
		action(atlas.get());
	}
	// 图集资源已经加载完成,从assets中解析并创建图集信息
	protected static AtlasBase atlasLoaded<T>(T[] assets, ResourceRef<UObject> mainAsset, string loadPath) where T : UObject
	{
		if (assets.isEmpty())
		{
			return null;
		}
		if (loadPath.endWith(SPRITE_ATLAS_SUFFIX))
		{
			AtlasUGUI atlas = new(mainAsset);
			atlas.setFilePath(loadPath);
			var spriteAtlas = mainAsset.get() as SpriteAtlas;
			if (spriteAtlas.spriteCount == 0)
			{
				if (isEditor())
				{
					logError("在编辑器中无法获取到一个SpriteAtlas中的图片数量,可能未开启图集的打包,请在PlayerSettings->Editor->Sprite Atlas Mode中设置为Sprite Atlas V2 - Enable,或者也可能这个图集中真的没有图片, path:" + loadPath);
				}
				else
				{
					logError("在真机运行时无法获取到一个SpriteAtlas中的图片数量,可能没有正常触发SpriteAtlasManager.atlasRequested,或者也可能这个图集中真的没有图片, path:" + loadPath);
				}
			}
			atlas.setAtlas(spriteAtlas);
			Sprite[] spriteList = new Sprite[spriteAtlas.spriteCount];
			if (spriteAtlas.GetSprites(spriteList) != spriteList.Length)
			{
				logError("sprite count error");
			}
			foreach (Sprite sprite in spriteList)
			{
				if (sprite == null)
				{
					continue;
				}
				string name = sprite.name.removeEndString("(Clone)");
				sprite.name = name;
				atlas.addSprite(sprite, name);
			}
			if (!atlas.isValid())
			{
				logError("can not find spriteatlas in loaded Texture:" + loadPath);
			}
			return atlas;
		}
		else
		{
			AtlasTP atlas = new(mainAsset);
			atlas.setFilePath(loadPath);
			// 找到Texture,位置不一定是第一个,需要遍历查找
			foreach (T item in assets)
			{
				// 找出所有的精灵
				if (item is Sprite sprite)
				{
					atlas.addSprite(sprite, sprite.name);
				}
				// 找到Texture2D
				else if (item is Texture2D tex2D)
				{
					atlas.setAtlas(tex2D);
				}
			}
			if (!atlas.isValid())
			{
				logError("can not find spriteatlas in loaded Texture:" + loadPath);
			}
			return atlas;
		}
	}
	protected bool allowUnloadAtlas(string atlasName) { return !mDontUnloadAtlas.Contains(atlasName); }
}