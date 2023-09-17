#if USE_MICROPHONE
using System;
using static UnityUtility;

// 音频录音
public class WavRecorder
{
	protected RecordCallback mRecordCallback;	// 录音数据的回调
	protected AudioClip mClip;					// 录制所需的音频
	protected string[] mDeviceList;				// 当前录音设备列表
	protected short[] mReceivedData;			// 录音数据缓冲区
	protected int mSampleRate;                  // 音频采样率
	protected int mMaxRecordTime;				// 最大录制时间
	protected int mStartDevice;					// 开始录制的设备下标
	protected int mLastPosition;				// 上一次的数据位置
	protected int mCurDataCount;				// 当前数据量
	public WavRecorder(int bufferSize, int sampleRate = 44100)
	{
		mSampleRate = sampleRate;
		mDeviceList = Microphone.devices;
		mReceivedData = new short[bufferSize];
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
		// mSampleRate不重置
		// mSampleRate = 0;
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
			if (mStartDevice != -1)
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
		if(mCurDataCount + dataCount <= mReceivedData.Length)
		{
			memcpy(mReceivedData, data, mCurDataCount, 0, dataCount * sizeof(short));
			mCurDataCount += dataCount;
		}
		// 数据量超出缓冲区
		else
		{
			int copyDataCount = mReceivedData.Length - mCurDataCount;
			memcpy(mReceivedData, data, mCurDataCount, 0, copyDataCount * sizeof(short));
			// 数据存满后通知回调
			mRecordCallback(mReceivedData, mReceivedData.Length);
			// 清空缓冲区,存储新的数据
			int remainCount = dataCount - copyDataCount;
			// 缓冲区无法存放剩下的数据,则将多余的数据丢弃
			if (remainCount > mReceivedData.Length)
			{
				memcpy(mReceivedData, data, 0, copyDataCount, (mReceivedData.Length - copyDataCount) * sizeof(short));
				mCurDataCount = mReceivedData.Length - copyDataCount;
			}
			else
			{
				memcpy(mReceivedData, data, 0, copyDataCount, remainCount * sizeof(short));
				mCurDataCount = remainCount;
			}
		}
	}
}
#endif