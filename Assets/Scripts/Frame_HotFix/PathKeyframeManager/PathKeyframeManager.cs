using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static StringUtility;
using static FrameBaseHotFix;
using static BinaryUtility;
using static FrameDefine;

// 用于管理变换的关键帧文件,主要是用于给Path类的组件提供参数
// 文件可使用PathRecorder进行录制生成
public class PathKeyframeManager : FrameSystem
{
	protected Dictionary<string, Dictionary<float, Vector3>> mTranslatePathList = new();    // key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, Vector3>> mRotatePathList = new();		// key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, Vector3>> mScalePathList = new();		// key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, float>> mAlphaPathList = new();          // key是文件名,value是对应的位置关键帧列表
	public override void destroy()
	{
		mTranslatePathList.Clear();
		mRotatePathList.Clear();
		mScalePathList.Clear();
		mAlphaPathList.Clear();
		base.destroy();
	}
	public Dictionary<float, Vector3> getTranslatePath(string fileName, bool loadIfNull = true)
	{
		if (!mTranslatePathList.TryGetValue(fileName, out var translatePath) && loadIfNull)
		{
			if (!mGameFrameworkHotFix.isResourceAvailable())
			{
				return null;
			}
			translatePath = mTranslatePathList.add(getFileNameNoSuffixNoDir(fileName), new());
			readPathFile(R_PATH_KEYFRAME_PATH + fileName + "_translate.bytes", translatePath);
		}
		return translatePath;
	}
	public Dictionary<float, Vector3> getRotatePath(string fileName, bool loadIfNull = true)
	{
		if (!mRotatePathList.TryGetValue(fileName, out var rotatePath) && loadIfNull)
		{
			if (!mGameFrameworkHotFix.isResourceAvailable())
			{
				return null;
			}
			rotatePath = mRotatePathList.add(getFileNameNoSuffixNoDir(fileName), new());
			readPathFile(R_PATH_KEYFRAME_PATH + fileName + "_rotate.bytes", rotatePath);
		}
		return rotatePath;
	}
	public Dictionary<float, Vector3> getScalePath(string fileName, bool loadIfNull = true)
	{
		if (!mScalePathList.TryGetValue(fileName, out var scalePath) && loadIfNull)
		{
			if (!mGameFrameworkHotFix.isResourceAvailable())
			{
				return null;
			}
			scalePath = mScalePathList.add(getFileNameNoSuffixNoDir(fileName), new());
			readPathFile(R_PATH_KEYFRAME_PATH + fileName + "_scale.bytes", scalePath);
		}
		return scalePath;
	}
	public Dictionary<float, float> getAlphaPath(string fileName, bool loadIfNull = true)
	{
		if (!mAlphaPathList.TryGetValue(fileName, out var alphaPath) && loadIfNull)
		{
			if (!mGameFrameworkHotFix.isResourceAvailable())
			{
				return null;
			}
			alphaPath = mAlphaPathList.add(getFileNameNoSuffixNoDir(fileName), new());
			readPathFile(R_PATH_KEYFRAME_PATH + fileName + "_alpha.bytes", alphaPath);
		}
		return alphaPath;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void readPathFile(string filePath, Dictionary<float, Vector3> path)
	{
		var file = mResourceManager.loadGameResource<TextAsset>(filePath);
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
			path.Add(SToF(elems[0]), SToV3(elems[1]));
		}
		mResourceManager.unload(ref file);
	}
	protected void readPathFile(string filePath, Dictionary<float, float> path)
	{
		var file = mResourceManager.loadGameResource<TextAsset>(filePath);
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