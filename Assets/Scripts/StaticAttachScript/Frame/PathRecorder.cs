using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PathRecorder : MonoBehaviour
{
    protected Dictionary<float, Vector3> mTranslatePath = new Dictionary<float, Vector3>();
    protected Dictionary<float, Vector3> mRotatePath = new Dictionary<float, Vector3>();
    protected Dictionary<float, Vector3> mScalePath = new Dictionary<float, Vector3>();
    protected Dictionary<float, float> mAlphaPath = new Dictionary<float, float>();
    protected float mStartTime;
    protected bool mRecording;
    public GameObject mAnimatorObject;
    public GameObject mRecorderTarget;
    public string mFilePath;
    public string mFileName;
    public bool mRecordeTranslate = true;
    public bool mRecordeRotate = true;
    public bool mRecordeScale = true;
    public bool mRecordeAlpha = true;
    public bool mStartRecorder;
    public void Start()
    {
        mAnimatorObject = gameObject;
        mFilePath = FrameDefine.F_PATH_KEYFRAME_PATH;
    }
    public void OnValidate()
    {
        if(mStartRecorder)
        {
            mStartRecorder = false;
            if(mRecording)
            {
                Debug.LogError("正在录制,无法重新开始录制");
                return;
            }
            if(mAnimatorObject == null || mRecorderTarget == null)
            {
                Debug.LogError("需要指定状态机节点和录制目标");
                return;
            }
            Animator animator = mAnimatorObject.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("状态机节点上找不到Animator");
                return;
            }
            AnimationRecorder recorder = animator.GetBehaviour<AnimationRecorder>();
            if(recorder == null)
            {
                Debug.LogError("找不到AnimationRecorder, 需要在要录制的动画上添加该组件");
                return;
            }
            if (StringUtility.isEmpty(mFilePath) || !FileUtility.isDirExist(mFilePath))
            {
                Debug.LogError("文件路径非法");
                return;
            }
            if (StringUtility.isEmpty(mFileName))
            {
                Debug.LogError("文件名非法");
                return;
            }
            if (mRecordeAlpha && 
                mRecorderTarget.GetComponent<Graphic>() == null && 
                mRecorderTarget.GetComponent<Renderer>() == null)
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
        if(mRecordeTranslate)
        {
            mTranslatePath.Add(0.0f, mRecorderTarget.transform.position);
        }
        if (mRecordeRotate)
        {
            mRotatePath.Add(0.0f, mRecorderTarget.transform.eulerAngles);
        }
        if(mRecordeScale)
        {
            mScalePath.Add(0.0f, mRecorderTarget.transform.lossyScale);
        }
        if(mRecordeAlpha)
        {
			if(mRecorderTarget.GetComponent<Graphic>() != null)
			{
				mAlphaPath.Add(0.0f, mRecorderTarget.GetComponent<Graphic>().color.a);
			}
			else if (mRecorderTarget.GetComponent<Renderer>() != null)
			{
				mAlphaPath.Add(0.0f, mRecorderTarget.GetComponent<Renderer>().material.color.a);
			}
		}
    }
    public void notifyRecording()
    {
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
			if (mRecorderTarget.GetComponent<Graphic>() != null)
			{
				mAlphaPath.Add(curTime, mRecorderTarget.GetComponent<Graphic>().color.a);
			}
			else if (mRecorderTarget.GetComponent<Renderer>() != null)
			{
				mAlphaPath.Add(curTime, mRecorderTarget.GetComponent<Renderer>().material.color.a);
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
                content += StringUtility.floatToString(item.Key) + ":" + StringUtility.vector3ToString(item.Value) + "\n";
            }
            FileUtility.writeTxtFile(pathWithName + ".translate", content);
        }
        // 旋转
        if (mRecordeRotate)
        {
            compress(mRotatePath);
            string content = "";
            foreach (var item in mRotatePath)
            {
                content += StringUtility.floatToString(item.Key) + ":" + StringUtility.vector3ToString(item.Value) + "\n";
            }
            FileUtility.writeTxtFile(pathWithName + ".rotate", content);
        }
        // 缩放
        if (mRecordeScale)
        {
            compress(mScalePath);
            string content = "";
            foreach (var item in mScalePath)
            {
                content += StringUtility.floatToString(item.Key) + ":" + StringUtility.vector3ToString(item.Value) + "\n";
            }
            FileUtility.writeTxtFile(pathWithName + ".scale", content);
        }
        if(mRecordeAlpha)
        {
            compress(mAlphaPath);
            string content = "";
            foreach (var item in mAlphaPath)
            {
                content += StringUtility.floatToString(item.Key) + ":" + StringUtility.floatToString(item.Value) + "\n";
            }
            FileUtility.writeTxtFile(pathWithName + ".alpha", content);
        }
        mAnimatorObject.GetComponent<Animator>().enabled = false;
        mRecording = false;
    }
    //---------------------------------------------------------------------------------------------------------------------
    protected static void compress(Dictionary<float, Vector3> path)
    {
        List<float> keys = new List<float>(path.Keys);
        Vector3 lastValue = path[keys[0]];
        int keyCount = keys.Count;
        for (int i = 0; i < keyCount; ++i)
        {
            // 跳过第一个和最后一个
            if(i == 0 || i == keyCount - 1)
            {
                continue;
            }
            Vector3 curValue = path[keys[i]];
            if(MathUtility.isVectorEqual(ref curValue, ref lastValue))
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
        List<float> keys = new List<float>(path.Keys);
        float lastValue = path[keys[0]];
        int keyCount = keys.Count;
        for (int i = 0; i < keyCount; ++i)
        {
            // 跳过第一个和最后一个
            if (i == 0 || i == keyCount - 1)
            {
                continue;
            }
            float curValue = path[keys[i]];
            if (MathUtility.isFloatEqual(curValue, lastValue))
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