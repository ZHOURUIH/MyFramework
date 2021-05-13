using UnityEngine;
using System.Collections.Generic;

// 用于管理变换的关键帧文件,主要是用于给Path类的组件提供参数
// 文件可使用PathRecorder进行录制生成
public class PathKeyframeManager : FrameSystem
{
    protected Dictionary<string, Dictionary<float, Vector3>> mTranslatePathList;    // key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, Vector3>> mRotatePathList;		// key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, Vector3>> mScalePathList;		// key是文件名,value是对应的位置关键帧列表
	protected Dictionary<string, Dictionary<float, float>> mAlphaPathList;			// key是文件名,value是对应的位置关键帧列表
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
		// 加载所有关键帧
		// 平移
		readAllFile(mTranslatePathList, ".translate");
		// 旋转
		readAllFile(mRotatePathList, ".rotate");
		// 缩放
		readAllFile(mScalePathList, ".scale");
		// 透明度
		readAllFile(mAlphaPathList, ".alpha");
	}
	public Dictionary<float, Vector3> getTranslatePath(string fileName)
	{
		mTranslatePathList.TryGetValue(fileName, out Dictionary<float, Vector3> translatePath);
		return translatePath;
	}
	public Dictionary<float, Vector3> getRotatePath(string fileName)
	{
		mRotatePathList.TryGetValue(fileName, out Dictionary<float, Vector3> rotatePath);
		return rotatePath;
	}
	public Dictionary<float, Vector3> getScalePath(string fileName)
	{
		mScalePathList.TryGetValue(fileName, out Dictionary<float, Vector3> scalePath);
		return scalePath;
	}
	public Dictionary<float, float> getAlphaPath(string fileName)
	{
		mAlphaPathList.TryGetValue(fileName, out Dictionary<float, float> alphaPath);
		return alphaPath;
	}
	//----------------------------------------------------------------------------------------------------------------------------
	protected void readAllFile(Dictionary<string, Dictionary<float, Vector3>> list, string suffix)
	{
		LIST_MAIN(out List<string> fileList);
		findStreamingAssetsFiles(FrameDefine.F_PATH_KEYFRAME_PATH, fileList, suffix, true, true);
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			var pathList = new Dictionary<float, Vector3>();
			readPathFile(fileList[i], pathList);
			list.Add(getFileNameNoSuffix(fileList[i], true), pathList);
		}
		UN_LIST_MAIN(fileList);
	}
	protected void readAllFile(Dictionary<string, Dictionary<float, float>> list, string suffix)
	{
		LIST_MAIN(out List<string> fileList);
		findStreamingAssetsFiles(FrameDefine.F_PATH_KEYFRAME_PATH, fileList, suffix, true, true);
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			var pathList = new Dictionary<float, float>();
			readPathFile(fileList[i], pathList);
			list.Add(getFileNameNoSuffix(fileList[i], true), pathList);
		}
		UN_LIST_MAIN(fileList);
	}
	protected void readPathFile(string filePath, Dictionary<float, Vector3> path)
	{
		string str = openTxtFile(filePath, true);
		string[] lines = split(str, true, "\n");
		int lineCount = lines.Length;
		for(int i = 0; i < lineCount; ++i)
		{
			lines[i] = removeAll(lines[i], "\r");
			string[] elems = split(lines[i], true, ":");
			if (elems.Length == 2)
			{
				path.Add(SToF(elems[0]), SToVector3(elems[1]));
			}
			else
			{
				logError(filePath + "第" + i + "行错误:" + lines[i]);
			}
		}
	}
	protected void readPathFile(string filePath, Dictionary<float, float> path)
	{
		string str = openTxtFile(filePath, true);
		string[] lines = split(str, true, "\n");
		int lineCount = lines.Length;
		for (int i = 0; i < lineCount; ++i)
		{
			lines[i] = removeAll(lines[i], "\r");
			string[] elems = split(lines[i], true, ":");
			if (elems.Length == 2)
			{
				path.Add(SToF(elems[0]), SToF(elems[1]));
			}
			else
			{
				logError(filePath + "第" + i + "行错误:" + lines[i]);
			}
		}
	}
}
