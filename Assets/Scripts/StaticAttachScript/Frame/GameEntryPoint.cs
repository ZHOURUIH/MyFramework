using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using UnityEngine.Networking;

public class GameEntryPoint : MonoBehaviour
{
	public static GameObject		mGameEntryObjectObject;
#if MAKE_CS_DLL
	public static Assembly			mGameAssembly;
	public static object			mGameInstance;
	public static Type				mGameType;
#endif
	public void Start()
	{
		mGameEntryObjectObject = gameObject;
#if MAKE_CS_DLL
		StartCoroutine(loadAssembly());
#else
		gameObject.AddComponent<Game>();
#endif
	}
	//--------------------------------------------------------------------------------------------------------------------------
#if MAKE_CS_DLL
	protected IEnumerator loadAssembly()
	{
		string dllPath = Application.streamingAssetsPath + "/Game.bytes";
		WWW www = new WWW(dllPath);
		while(!www.isDone)
		{
			yield return www.isDone;
		}
		if(www.error == null)
		{
			mGameAssembly = Assembly.Load(www.bytes);
			mGameType = ReflectionUtility.getTypeInGame("Game");
			ReflectionUtility.checkUnityDefine();
			mGameInstance = gameObject.AddComponent(mGameType);
		}
		www.Dispose();
		www = null;
	}
#endif
}