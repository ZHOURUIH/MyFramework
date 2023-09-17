using System;
using System.Collections.Generic;
using System.Reflection;
using static UnityUtility;
using static FrameUtility;
using static FileUtility;
using static StringUtility;
using static FrameDefine;

// 插件后缀为plugin,插件依赖的库在编辑器模式下需要放到Plugins中,打包后放到Managed中
public class GamePluginManager : FrameSystem
{
	protected Dictionary<string, IGamePlugin> mPluginList;		// 已加载的插件列表
	public GamePluginManager()
	{
		mPluginList = new Dictionary<string, IGamePlugin>();
	}
	public override void init()
	{
		base.init();
		loadAllPlugin();
		foreach (var item in mPluginList)
		{
			item.Value.init();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var item in mPluginList)
		{
			item.Value.update(elapsedTime);
		}
	}
	public override void destroy()
	{
		foreach (var item in mPluginList)
		{
			item.Value.destroy();
		}
		mPluginList.Clear();
		base.destroy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void loadAllPlugin()
	{
#if UNITY_STANDALONE_WIN
		if(!isDirExist(F_GAME_PLUGIN_PATH))
		{
			return;
		}
		using (new ListScope<string>(out var fileList))
		{
			findFiles(F_GAME_PLUGIN_PATH, fileList, DLL_PLUGIN_SUFFIX);
			int count = fileList.Count;
			for (int i = 0; i < count; ++i)
			{
				loadPlugin(openFile(fileList[i], true), getFileName(fileList[i]));
			}
		}
#endif
	}
	protected bool loadPlugin(byte[] rawDll, string fileName)
	{
		try
		{
			Assembly assembly = Assembly.Load(rawDll);
			Type[] types = assembly.GetTypes();
			int count = types.Length;
			for(int i = 0; i < count; ++i)
			{
				Type type = types[i];
				if (type.GetInterfaces().Length == 0)
				{
					continue;
				}
				var instance = assembly.CreateInstance(type.FullName) as IGamePlugin;
				if (instance == null)
				{
					continue;
				}
				mPluginList.Add(instance.getPluginName(), instance);
				log("game plugin " + instance.getPluginName() + " load success!");
			}
		}
		catch (Exception e)
		{
			log("load game plugin failed! file name : " + fileName + ", info : " + e.Message);
			return false;
		}
		return true;
	}
}