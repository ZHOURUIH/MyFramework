using System;
using UnityEngine;

public delegate void RecordCallback(short[] data, int dataCount);

public class WavRecorder : FrameBase
{
	protected RecordCallback mRecordCallback;
	protected AudioClip mClip;
	protected string[] mDeviceList;
	protected short[] mReceivedData;
	protected int mSampleRate;                  // 音频采样率
	protected int mMaxRecordTime;
	protected int mStartDevice;
	protected int mLastPosition;
	protected int mBufferSize;
	protected int mCurDataCount;
	public WavRecorder(int bufferSize, int sampleRate = 44100)
	{
		mSampleRate = sampleRate;
		mDeviceList = Microphone.devices;
		mBufferSize = bufferSize;
		mReceivedData = new short[mBufferSize];
		mMaxRecordTime = 500;
		mStartDevice = -1;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mRecordCallback = null;
		mClip = null;
		mDeviceList = null;
		memset(mReceivedData, (short)0);
		// mSampleRate, mBufferSize不重置
		// mSampleRate = 0;
		// mBufferSize = 0;
		mMaxRecordTime = 500;
		mStartDevice = -1;
		mLastPosition = 0;
		mCurDataCount = 0;
	}
	public void setRecordCallback(RecordCallback callback)
	{
		mRecordCallback = callback;
	}
	public bool startRecord(int startDevice)
	{
		try
		{
			stopRecord();
			if (startDevice < 0 || startDevice >= mDeviceList.Length)
			{
				return false;
			}
			mStartDevice = startDevice;
			mClip = Microphone.Start(mDeviceList[mStartDevice], false, mMaxRecordTime, mSampleRate);
		}
		catch (Exception e)
		{
			logForce("error in record! : " + e.Message);
			return false;
		}
		return true;
	}
	public void stopRecord()
	{
		if (mStartDevice >= 0 && mStartDevice < mDeviceList.Length)
		{
			Microphone.End(mDeviceList[mStartDevice]);
			mStartDevice = -1;
		}
	}
	public void update(float elapsedTime)
	{
		// 如果正在录音
		if (Microphone.IsRecording(mDeviceList[mStartDevice]))
		{
			int curPos = Microphone.GetPosition(mDeviceList[mStartDevice]);
			if (curPos - mLastPosition > 0 && mRecordCallback != null)
			{
				int dataCount = curPos - mLastPosition;
				float[] data = new float[curPos - mLastPosition];
				mClip.GetData(data, mLastPosition);
				short[] pcmData = new short[dataCount];
				for (int i = 0; i < dataCount; ++i)
				{
					pcmData[i] = (short)(data[i] * 0x7FFF);
				}
				receiveData(pcmData, curPos - mLastPosition);
				mLastPosition = curPos;
			}
		}
		else 
		{
			if (mStartDevice!=-1)
			{
				mClip = Microphone.Start(mDeviceList[mStartDevice], false, mMaxRecordTime, mSampleRate);
				mLastPosition = 0;
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void receiveData(short[] data, int dataCount)
	{
		// 缓冲区还能直接放下数据
		if(mCurDataCount + dataCount <= mBufferSize)
		{
			memcpy(mReceivedData, data, mCurDataCount, 0, dataCount * sizeof(short));
			mCurDataCount += dataCount;
		}
		// 数据量超出缓冲区
		else
		{
			int copyDataCount = mBufferSize - mCurDataCount;
			memcpy(mReceivedData, data, mCurDataCount, 0, copyDataCount * sizeof(short));
			// 数据存满后通知回调
			mRecordCallback(mReceivedData, mBufferSize);
			// 清空缓冲区,存储新的数据
			int remainCount = dataCount - copyDataCount;
			// 缓冲区无法存放剩下的数据,则将多余的数据丢弃
			if (remainCount > mBufferSize)
			{
				memcpy(mReceivedData, data, 0, copyDataCount, (mBufferSize - copyDataCount) * sizeof(short));
				mCurDataCount = mBufferSize - copyDataCount;
			}
			else
			{
				memcpy(mReceivedData, data, 0, copyDataCount, remainCount * sizeof(short));
				mCurDataCount = remainCount;
			}
		}
	}
}