using System;
using System.Collections.Generic;
using System.Reflection;

// 插件后缀为bytes,插件依赖的库在编辑器模式下需要放到Plugins中,打包后放到Managed中
public class GamePluginManager : FrameComponent
{
	protected Dictionary<string, IGamePlugin> mPluginList;
	public GamePluginManager(string name)
		:base(name)
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
	//-----------------------------------------------------------------------------------------------------------
	protected void loadAllPlugin()
	{
#if UNITY_STANDALONE_WIN
		if(!isDirExist(CommonDefine.F_GAME_PLUGIN_PATH))
		{
			return;
		}
		List<string> fileList = new List<string>();
		findFiles(CommonDefine.F_GAME_PLUGIN_PATH, fileList, CommonDefine.DLL_PLUGIN_SUFFIX);
		int count = fileList.Count;
		for (int i = 0; i < count; ++i)
		{
			byte[] fileBuffer = null;
			openFile(fileList[i], out fileBuffer, true);
			loadPlugin(fileBuffer, getFileName(fileList[i]));
		}
#endif
	}
	protected bool loadPlugin(byte[] rawDll, string fileName)
	{
		try
		{
			Assembly assembly = Assembly.Load(rawDll);
			Type[] types = assembly.GetTypes();
			foreach (var type in types)
			{
				if (type.GetInterfaces().Length > 0)
				{
					IGamePlugin instance = assembly.CreateInstance(type.FullName) as IGamePlugin;
					if (instance != null)
					{
						mPluginList.Add(instance.getPluginName(), instance);
						logInfo("game plugin " + instance.getPluginName() + " load success!");
					}
				}
			}
		}
		catch (Exception e)
		{
			logInfo("load game plugin failed! file name : " + fileName + ", info : " + e.Message);
			return false;
		}
		return true;
	}
}