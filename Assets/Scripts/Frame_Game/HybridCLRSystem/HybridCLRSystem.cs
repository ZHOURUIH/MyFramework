using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
#if USE_HYBRID_CLR
using HybridCLR;
#endif
using static FrameBase;
using static FileUtility;
using static FrameDefineBase;
using static UnityUtility;
using static FrameUtility;
using static StringUtility;

// HybridCLR系统,用于启动HybridCLR热更
public class HybridCLRSystem
{
	public static void launchHotFix(byte[] aesKey, byte[] aesIV, Action errorCallback = null)
	{
		try
		{
			// 存储所有需要跨域的参数
			backupCrossParam();
			// 启动热更系统
#if UNITY_EDITOR || !USE_HYBRID_CLR
			launchEditor(errorCallback);
#else
			launchRuntime(aesKey, aesIV, errorCallback);
#endif
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void launchRuntime(byte[] aesKey, byte[] aesIV, Action errorCallback)
	{
#if USE_HYBRID_CLR
		Dictionary<string, byte[]> downloadFiles = new();
		foreach (string aotFile in AOTGenericReferences.PatchedAOTAssemblyList)
		{
			downloadFiles.Add(aotFile, null);
		}
		downloadFiles.Add(HOTFIX_FRAME_FILE, null);
		downloadFiles.Add(HOTFIX_FILE, null);
		int finishCount = 0;
		foreach (string item in new List<string>(downloadFiles.Keys))
		{
			string fileDllName = item;
			openFileAsync(availableReadPath(fileDllName + ".bytes"), true, (byte[] bytes) =>
			{
				if (bytes == null)
				{
					downloadFiles = null;
					errorCallback?.Invoke();
					return;
				}
				if (downloadFiles == null)
				{
					return;
				}
				downloadFiles.set(fileDllName, bytes);
				if (++finishCount < downloadFiles.Count)
				{
					return;
				}
				foreach (string aotFile in AOTGenericReferences.PatchedAOTAssemblyList)
				{
					// 为aot assembly加载原始metadata
					// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
					// 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
					// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
					// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
					LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(downloadFiles.get(aotFile), HomologousImageMode.SuperSet);
					if (err != LoadImageErrorCode.OK)
					{
						Debug.Log("LoadMetadataForAOTAssembly失败:" + aotFile + ", " + err);
					}
				}
				// 加载以后不再卸载
				Assembly.Load(decryptAES(downloadFiles.get(HOTFIX_FRAME_FILE), aesKey, aesIV));
				launchInternal(Assembly.Load(decryptAES(downloadFiles.get(HOTFIX_FILE), aesKey, aesIV)));
			});
		}
#endif
	}
	protected static void launchEditor(Action errorCallback)
	{
		Assembly hotFixAssembly = null;
		string dllName = getFileNameNoSuffixNoDir(HOTFIX_FILE);
		foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (item.GetName().Name == dllName)
			{
				hotFixAssembly = item;
				break;
			}
		}
		if (hotFixAssembly == null)
		{
			errorCallback?.Invoke();
			return;
		}
		launchInternal(hotFixAssembly);
	}
	protected static void launchInternal(Assembly hotFixAssembly)
	{
		if (hotFixAssembly == null)
		{
			logError("加载热更程序集失败:" + HOTFIX_FILE);
			return;
		}
		Type type = hotFixAssembly.GetType("GameHotFix");
		if (type == null)
		{
			logError("在热更程序集中找不到GameHotFix类");
			return;
		}
		if (type.BaseType.Name != "GameHotFixBase")
		{
			logError("GameHotFix类需要继承自GameHotFixBase");
			return;
		}
		// 创建热更对象示例的函数签名为public void createHotFixInstance()
		MethodInfo methodCreate = type.GetMethod("createHotFixInstance");
		if (methodCreate == null)
		{
			logError("在GameHotFix类中找不到静态函数createHotFixInstance");
			return;
		}

		// 查找start函数
		MethodInfo methodStart = type.GetMethod("start");
		if (methodStart == null)
		{
			logError("在GameHotFix类中找不到函数start");
			return;
		}
		// 执行热更的启动函数
		Action callback = ()=>
		{
			Debug.Log("热更初始化完毕");
			// 热更初始化完毕后将非热更层加载的所有资源都清除,这样避免中间的黑屏
			GameObject go = mGameFramework.gameObject;
			destroyUnityObject(mGameFramework, true);
			destroyUnityObject(go, true);
		};
		// 使用createHotFixInstance创建一个HotFix的实例,然后调用此实例的start函数
		methodStart.Invoke(methodCreate.Invoke(null, null), new object[1]{ callback });
	}
}