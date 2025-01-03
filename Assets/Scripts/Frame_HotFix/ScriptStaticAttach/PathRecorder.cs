using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FrameDefine;
using static StringUtility;
using static FileUtility;
using static MathUtility;
using static FrameDefineBase;

// 物体轨迹记录工具
public class PathRecorder : MonoBehaviour
{
	protected Dictionary<float, Vector3> mTranslatePath = new();    // 记录的位移列表,分开记录是为了可以压缩数据
	protected Dictionary<float, Vector3> mRotatePath = new();       // 记录的旋转列表
	protected Dictionary<float, Vector3> mScalePath = new();        // 记录的缩放列表
	protected Dictionary<float, float> mAlphaPath = new();          // 记录的透明度列表
	protected float mStartTime;										// 起始时间点
	protected bool mRecording;										// 是否正在记录
	public GameObject mAnimatorObject;								// Animator组件所在的节点
	public GameObject mRecorderTarget;								// 记录轨迹的目标物体
	public string mFilePath;										// 保存的文件路径
	public string mFileName;										// 保存的文件名
	public bool mRecordeTranslate = true;							// 是否记录平移
	public bool mRecordeRotate = true;								// 是否记录旋转
	public bool mRecordeScale = true;								// 是否记录缩放
	public bool mRecordeAlpha = true;								// 是否记录透明度
	public bool mStartRecorder;										// 是否开始录制,当作开始按钮使用
	public void Start()
	{
		mAnimatorObject = gameObject;
		mFilePath = F_STREAMING_ASSETS_PATH + SA_PATH_KEYFRAME_PATH;
	}
	public void OnValidate()
	{
		if (mStartRecorder)
		{
			mStartRecorder = false;
			if (mRecording)
			{
				Debug.LogError("正在录制,无法重新开始录制");
				return;
			}
			if (mAnimatorObject == null || mRecorderTarget == null)
			{
				Debug.LogError("需要指定状态机节点和录制目标");
				return;
			}
			if (!mAnimatorObject.TryGetComponent<Animator>(out var animator))
			{
				Debug.LogError("状态机节点上找不到Animator");
				return;
			}
			var recorder = animator.GetBehaviour<AnimationRecorder>();
			if (recorder == null)
			{
				Debug.LogError("找不到AnimationRecorder, 需要在要录制的动画上添加该组件");
				return;
			}
			if (mFilePath.isEmpty() || !isDirExist(mFilePath))
			{
				Debug.LogError("文件路径非法");
				return;
			}
			if (mFileName.isEmpty())
			{
				Debug.LogError("文件名非法");
				return;
			}
			if (mRecordeAlpha &&
				!mRecorderTarget.TryGetComponent<Graphic>(out _) &&
				!mRecorderTarget.TryGetComponent<Renderer>(out _))
			{
				Debug.LogError("录制目标上找不到透明度设置");
			}
			recorder.mPathRecorder = this;
			animator.enabled = false;
			animator.enabled = true;
		}
	}
	public void notifyStartRecord()
	{
		mRecording = true;
		mTranslatePath.Clear();
		mRotatePath.Clear();
		mScalePath.Clear();
		mStartTime = Time.realtimeSinceStartup;
		if (mRecordeTranslate)
		{
			mTranslatePath.Add(0.0f, mRecorderTarget.transform.position);
		}
		if (mRecordeRotate)
		{
			mRotatePath.Add(0.0f, mRecorderTarget.transform.eulerAngles);
		}
		if (mRecordeScale)
		{
			mScalePath.Add(0.0f, mRecorderTarget.transform.lossyScale);
		}
		if (mRecordeAlpha)
		{
			if (mRecorderTarget.TryGetComponent<Graphic>(out var graphic))
			{
				mAlphaPath.Add(0.0f, graphic.color.a);
			}
			else if (mRecorderTarget.TryGetComponent<Renderer>(out var renderer))
			{
				mAlphaPath.Add(0.0f, renderer.material.color.a);
			}
		}
	}
	public void notifyRecording()
	{
		if (!mRecording)
		{
			return;
		}
		float curTime = Time.realtimeSinceStartup - mStartTime;
		if (mRecordeTranslate)
		{
			mTranslatePath.Add(curTime, mRecorderTarget.transform.position);
		}
		if (mRecordeRotate)
		{
			mRotatePath.Add(curTime, mRecorderTarget.transform.eulerAngles);
		}
		if (mRecordeScale)
		{
			mScalePath.Add(curTime, mRecorderTarget.transform.lossyScale);
		}
		if (mRecordeAlpha)
		{
			if (mRecorderTarget.TryGetComponent<Graphic>(out var graphic))
			{
				mAlphaPath.Add(curTime, graphic.color.a);
			}
			else if (mRecorderTarget.TryGetComponent<Renderer>(out var renderer))
			{
				mAlphaPath.Add(curTime, renderer.material.color.a);
			}
		}
	}
	public void notifyEndRecord()
	{
		string pathWithName = mFilePath + mFileName;
		// 压缩数据,去除连续相同的值,然后写入文件
		// 位移
		if (mRecordeTranslate)
		{
			compress(mTranslatePath);
			string content = "";
			foreach (var item in mTranslatePath)
			{
				content += FToS(item.Key) + ":" + V3ToS(item.Value) + "\n";
			}
			writeTxtFile(pathWithName + ".translate", content);
		}
		// 旋转
		if (mRecordeRotate)
		{
			compress(mRotatePath);
			string content = "";
			foreach (var item in mRotatePath)
			{
				content += FToS(item.Key) + ":" + V3ToS(item.Value) + "\n";
			}
			writeTxtFile(pathWithName + ".rotate", content);
		}
		// 缩放
		if (mRecordeScale)
		{
			compress(mScalePath);
			string content = "";
			foreach (var item in mScalePath)
			{
				content += FToS(item.Key) + ":" + V3ToS(item.Value) + "\n";
			}
			writeTxtFile(pathWithName + ".scale", content);
		}
		if (mRecordeAlpha)
		{
			compress(mAlphaPath);
			string content = "";
			foreach (var item in mAlphaPath)
			{
				content += FToS(item.Key) + ":" + FToS(item.Value) + "\n";
			}
			writeTxtFile(pathWithName + ".alpha", content);
		}
		mAnimatorObject.TryGetComponent<Animator>(out var animator);
		animator.enabled = false;
		mRecording = false;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void compress(Dictionary<float, Vector3> path)
	{
		List<float> keys = new(path.Keys);
		Vector3 lastValue = path.get(keys[0]);
		int keyCount = keys.Count;
		for (int i = 0; i < keyCount; ++i)
		{
			// 跳过第一个和最后一个
			if (i == 0 || i == keyCount - 1)
			{
				continue;
			}
			Vector3 curValue = path.get(keys[i]);
			if (isVectorEqual(curValue, lastValue))
			{
				path.Remove(keys[i]);
			}
			else
			{
				lastValue = curValue;
			}
		}
	}
	protected static void compress(Dictionary<float, float> path)
	{
		List<float> keys = new(path.Keys);
		float lastValue = path.get(keys[0]);
		int keyCount = keys.Count;
		for (int i = 0; i < keyCount; ++i)
		{
			// 跳过第一个和最后一个
			if (i == 0 || i == keyCount - 1)
			{
				continue;
			}
			float curValue = path.get(keys[i]);
			if (isFloatEqual(curValue, lastValue))
			{
				path.Remove(keys[i]);
			}
			else
			{
				lastValue = curValue;
			}
		}
	}
}