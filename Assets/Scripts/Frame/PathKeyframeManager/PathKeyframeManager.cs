using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static StringUtility;
using static FrameBase;
using static BinaryUtility;
using static FrameDefine;

// 用于管理变换的关键帧文件,主要是用于给Path类的组件提供参数
// 文件可使用PathRecorder进行录制生成
public class PathKeyframeManager : FrameSystem
{
	protected Dictionary<string, Dictionary<float, Vector3>> mTranslatePathList;    // key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, Vector3>> mRotatePathList;		// key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, Vector3>> mScalePathList;		// key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, float>> mAlphaPathList;          // key是文件名,value是对应的位置关键帧列表
	public PathKeyframeManager()
	{
		mTranslatePathList = new Dictionary<string, Dictionary<float, Vector3>>();
		mRotatePathList = new Dictionary<string, Dictionary<float, Vector3>>();
		mScalePathList = new Dictionary<string, Dictionary<float, Vector3>>();
		mAlphaPathList = new Dictionary<string, Dictionary<float, float>>();
	}
	public override void destroy()
	{
		mTranslatePathList.Clear();
		mRotatePathList.Clear();
		mScalePathList.Clear();
		mAlphaPathList.Clear();
		base.destroy();
	}
	public override void init()
	{
		base.init();
	}
	public Dictionary<float, Vector3> getTranslatePath(string fileName, bool loadIfNull = true)
	{
		if (!mTranslatePathList.TryGetValue(fileName, out var translatePath) && loadIfNull)
		{
			if (!mGameFramework.isResourceAvailable())
			{
				return null;
			}
			translatePath = new Dictionary<float, Vector3>();
			readPathFile(SA_PATH_KEYFRAME_PATH + fileName + "_translate.bytes", translatePath);
			mTranslatePathList.Add(getFileNameNoSuffix(fileName, true), translatePath);
		}
		return translatePath;
	}
	public Dictionary<float, Vector3> getRotatePath(string fileName, bool loadIfNull = true)
	{
		if (!mRotatePathList.TryGetValue(fileName, out var rotatePath) && loadIfNull)
		{
			if (!mGameFramework.isResourceAvailable())
			{
				return null;
			}
			rotatePath = new Dictionary<float, Vector3>();
			readPathFile(SA_PATH_KEYFRAME_PATH + fileName + "_rotate.bytes", rotatePath);
			mRotatePathList.Add(getFileNameNoSuffix(fileName, true), rotatePath);
		}
		return rotatePath;
	}
	public Dictionary<float, Vector3> getScalePath(string fileName, bool loadIfNull = true)
	{
		if (!mScalePathList.TryGetValue(fileName, out var scalePath) && loadIfNull)
		{
			if (!mGameFramework.isResourceAvailable())
			{
				return null;
			}
			scalePath = new Dictionary<float, Vector3>();
			readPathFile(SA_PATH_KEYFRAME_PATH + fileName + "_scale.bytes", scalePath);
			mScalePathList.Add(getFileNameNoSuffix(fileName, true), scalePath);
		}
		return scalePath;
	}
	public Dictionary<float, float> getAlphaPath(string fileName, bool loadIfNull = true)
	{
		if (!mAlphaPathList.TryGetValue(fileName, out var alphaPath) && loadIfNull)
		{
			if (!mGameFramework.isResourceAvailable())
			{
				return null;
			}
			alphaPath = new Dictionary<float, float>();
			readPathFile(SA_PATH_KEYFRAME_PATH + fileName + "_alpha.bytes", alphaPath);
			mAlphaPathList.Add(getFileNameNoSuffix(fileName, true), alphaPath);
		}
		return alphaPath;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void readPathFile(string filePath, Dictionary<float, Vector3> path)
	{
		var file = mResourceManager.loadResource<TextAsset>(filePath);
		string fileString = bytesToString(file.bytes);
		splitLine(fileString, out string[] lines);
		int lineCount = lines.Length;
		for (int i = 0; i < lineCount; ++i)
		{
			string[] elems = split(lines[i], ':');
			if (elems.Length != 2)
			{
				logError(filePath + "第" + i + "行错误:" + lines[i]);
				return;	
			}
			path.Add(SToF(elems[0]), SToVector3(elems[1]));
		}
		mResourceManager.unload(ref file);
	}
	protected void readPathFile(string filePath, Dictionary<float, float> path)
	{
		var file = mResourceManager.loadResource<TextAsset>(filePath);
		string fileString = bytesToString(file.bytes);
		splitLine(fileString, out string[] lines);
		int lineCount = lines.Length;
		for (int i = 0; i < lineCount; ++i)
		{
			string[] elems = split(lines[i], ':');
			if (elems.Length != 2)
			{
				logError(filePath + "第" + i + "行错误:" + lines[i]);
				continue;
			}
			path.Add(SToF(elems[0]), SToF(elems[1]));
		}
		mResourceManager.unload(ref file);
	}
}