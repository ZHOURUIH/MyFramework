using System;
using System.Collections.Generic;
using System.Reflection;
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static FrameDefine;
using static FrameBaseUtility;

// �����׺Ϊplugin,��������Ŀ��ڱ༭��ģʽ����Ҫ�ŵ�Plugins��,�����ŵ�Managed��
public class GamePluginManager : FrameSystem
{
	protected Dictionary<string, IGamePlugin> mPluginList = new();      // �Ѽ��صĲ���б�
	public override void init()
	{
		base.init();
		if (isWindows())
		{
			loadAllPlugin();
		}
		foreach (IGamePlugin item in mPluginList.Values)
		{
			item.init();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (IGamePlugin item in mPluginList.Values)
		{
			item.update(elapsedTime);
		}
	}
	public override void destroy()
	{
		foreach (IGamePlugin item in mPluginList.Values)
		{
			item.destroy();
		}
		mPluginList.Clear();
		base.destroy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void loadAllPlugin()
	{
		if (!isDirExist(F_GAME_PLUGIN_PATH))
		{
			return;
		}
		foreach (string file in findFilesNonAlloc(F_GAME_PLUGIN_PATH, DLL_PLUGIN_SUFFIX))
		{
			openFileAsync(file, true, (byte[] data) =>
			{
				loadPlugin(data, getFileNameWithSuffix(file));
			});
		}
	}
	protected bool loadPlugin(byte[] rawDll, string fileName)
	{
		try
		{
			Assembly assembly = Assembly.Load(rawDll);
			foreach (Type type in assembly.GetTypes())
			{
				if (type.GetInterfaces().Length == 0 || assembly.CreateInstance(type.FullName) is not IGamePlugin instance)
				{
					continue;
				}
				mPluginList.Add(instance.getPluginName(), instance);
				log("game plugin " + instance.getPluginName() + " load success!");
			}
		}
		catch (Exception e)
		{
			logException(e, "load game plugin failed! file name : " + fileName);
			return false;
		}
		return true;
	}
}