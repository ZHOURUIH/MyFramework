#if USE_MICROPHONE
using System;
using System.Collections.Generic;
using static BinaryUtility;

// 录音机数据的解析器
public class RecorderParser
{
	protected const int RECODER_DATA_BLOCK = 1024;  // 音频输入数据一次解析的数据数量,也是频域数据数量
	protected const int SAMPLE_RATE = 44100;		// 音频采样频率
	protected const int BLOCK_BUFFER_SIZE = (int)(SAMPLE_RATE * 0.02f);	// 每次接收到的数据缓冲区的大小
	protected WavRecorder mRecorder;				// 采集本地音频输入的录音机
	protected short[] mFrequencyData;				// 转换后的频域数据
	protected short[] mAllPCMData;					// 总的PCM数据
	protected int mAllPCMCount;						// 总PCM数据的数量,当前已有的PCM数据数量
	protected int mCurDB;							// 当前音量大小
	public RecorderParser()
	{
		mCurDB = -96;
		// 采集音频输入
		mRecorder = new WavRecorder(BLOCK_BUFFER_SIZE, SAMPLE_RATE);
		mRecorder.setRecordCallback(onRecorderData);
		// 缓冲区大小固定为1024,确保比mBlockBufferSize大
		mAllPCMData = new short[RECODER_DATA_BLOCK];
		// 频域缓冲区
		mFrequencyData = new short[RECODER_DATA_BLOCK];
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mRecorder.resetProperty();
		mRecorder.setRecordCallback(onRecorderData);
		memset(mFrequencyData, (short)0);
		memset(mAllPCMData, (short)0);
		mAllPCMCount = 0;
		mCurDB = -96;
	}
	public void destroy()
	{
		stopRecord();
	}
	public void update(float elapsedTime)
	{
		mRecorder?.update(elapsedTime);
	}
	public short[] getFrequencyData() { return mFrequencyData; }
	public int getFrequencyDataCount() { return RECODER_DATA_BLOCK; }
	public int getCurDB() { return mCurDB; }
	public bool startRecord()
	{
		mAllPCMCount = 0;
		return mRecorder.startRecord(0);
	}
	public void stopRecord()
	{
		mRecorder.stopRecord();
	}
	public void setRecordData(short[] data, int dataSize)
	{
		// 检测数据量是否正确
		if (BLOCK_BUFFER_SIZE != dataSize)
		{
			return;
		}
		// 由于缓冲区大小和每次获取的数据数量都是固定的,所以只要有数据就需要移动
		// 将已有的数据移到缓冲区头部,然后将新的数据加入尾部
		if (mAllPCMCount > 0)
		{
			memmove(ref mAllPCMData, 0, dataSize, RECODER_DATA_BLOCK - dataSize);
			memcpy(mAllPCMData, data, RECODER_DATA_BLOCK - dataSize, 0, dataSize * sizeof(short));
		}
		else
		{
			memset(mAllPCMData, (short)0, RECODER_DATA_BLOCK);
			memcpy(mAllPCMData, data, 0, 0, dataSize * sizeof(short));
			mAllPCMCount = dataSize;
		}
	}
	public void onRecorderData(short[] data, int dataSize)
	{
		setRecordData(data, dataSize);
	}
	public void updateDBFrequency()
	{
		// 计算音量大小和频域
		mCurDB = pcm_db_count(mAllPCMData, RECODER_DATA_BLOCK);
	}
}
#endif