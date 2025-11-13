#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using static BinaryUtility;
using static FrameUtility;
using static FileUtility;
using static MathUtility;

// 用于解析wav音频文件
public class WavSound : ClassObject
{
	protected SerializerWrite mWaveSerializer;	// 数据写入的缓冲区,可以生成wav文件数据,但是没有实现文件写入
	protected string mFileName;		// 文件名
	protected short[] mMixPCMData;	// 解析的wav的pcm数据混合后的数据
	protected short mBlockAlign; 	// DATA数据块长度
	protected short mBitsPerSample;	// 单个采样数据大小,如果双声道16位,则是4个字节,也叫PCM位宽
	protected short mOtherSize;		// 附加信息（可选，由上方过滤字节确定）
	protected short mFormatType;	// 编码格式,为1是PCM编码
	protected short mSoundChannels;	// 声道数
	protected byte[] mDataMark;		// data标记
	protected byte[] mDataBuffer;	// 数据区
	protected int mRiffMark;		// riff标记
	protected int mFileSize;		// 音频文件大小 - 8,也就是从文件大小字节后到文件结尾的长度
	protected int mWaveMark;		// wave标记
	protected int mFmtMark;			// fmt 标记
	protected int mFmtChunkSize;	// fmt块大小
	protected int mSamplesPerSec;	// 采样频率
	protected int mAvgBytesPerSec;	// 波形数据传输速率（每秒平均字节数）
	public WavSound()
	{
		mDataMark = new byte[4];
	}
	public WavSound(string file)
	{
		mDataMark = new byte[4];
		readFile(file);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mWaveSerializer = null;
		memset(mDataMark, (byte)0);
		mFileName = null;
		mRiffMark = 0;
		mFileSize = 0;
		mWaveMark = 0;
		mFmtMark = 0;
		mFmtChunkSize = 0;
		mFormatType = 0;
		mSoundChannels = 0;
		mSamplesPerSec = 0;
		mAvgBytesPerSec = 0;
		mBlockAlign = 0;
		mBitsPerSample = 0;
		mOtherSize = 0;
		mDataBuffer = null;
		mMixPCMData = null;
	}
	public byte[] getPCMBuffer()		{ return mDataBuffer; }
	public short[] getMixPCMData()		{ return mMixPCMData; }
	public int getPCMBufferSize()		{ return mDataBuffer.Length; }
	public short getSoundChannels()		{ return mSoundChannels; }
	public int getPCMShortDataCount()	{ return mDataBuffer.Length / sizeof(short); }
	public int getMixPCMDataCount()		{ return mDataBuffer.Length / (sizeof(short) * mSoundChannels); }
	public void readFile(string file)
	{
		mFileName = file;
		openFileAsync(file, true, (byte[] data)=>
		{
			using var a = new ClassScope<SerializerRead>(out var serializer);
			serializer.init(data);
			serializer.read(out mRiffMark);
			serializer.read(out mFileSize);
			serializer.read(out mWaveMark);
			serializer.read(out mFmtMark);
			serializer.read(out mFmtChunkSize);
			serializer.read(out mFormatType);
			serializer.read(out mSoundChannels);
			serializer.read(out mSamplesPerSec);
			serializer.read(out mAvgBytesPerSec);
			serializer.read(out mBlockAlign);
			serializer.read(out mBitsPerSample);
			if (mFmtChunkSize == 18)
			{
				serializer.read(out mOtherSize);
			}
			// 如果不是data块,则跳过,重新读取
			do
			{
				serializer.readBuffer(mDataMark, 4, 4);
				serializer.read(out int dataSize);
				mDataBuffer = new byte[dataSize];
				serializer.readBuffer(mDataBuffer, mDataBuffer.Length, mDataBuffer.Length);
			} while (bytesToString(mDataMark) != "data");
			refreshFileSize();
			int mixDataCount = getMixPCMDataCount();
			mMixPCMData = new short[mixDataCount];
			generateMixPCMData(mMixPCMData, mixDataCount, mSoundChannels, mDataBuffer);
		});
	}
	public static void generateMixPCMData(short[] mixPCMData, int mixDataCount, short channelCount, byte[] dataBuffer)
	{
		Span<byte> tempBytes = stackalloc byte[2];
		// 如果单声道,则直接将mDataBuffer的数据拷贝到mMixPCMData中
		if (channelCount == 1)
		{
			for (int i = 0; i < mixDataCount; ++i)
			{
				tempBytes[0] = dataBuffer[2 * i + 0];
				tempBytes[1] = dataBuffer[2 * i + 1];
				mixPCMData[i] = bytesToShort(tempBytes);
			}
		}
		// 如果有两个声道,则将左右两个声道的平均值赋值到mMixPCMData中
		else if (channelCount == 2)
		{
			for (int i = 0; i < mixDataCount; ++i)
			{
				tempBytes[0] = dataBuffer[4 * i + 0];
				tempBytes[1] = dataBuffer[4 * i + 1];
				short shortData0 = bytesToShort(tempBytes);
				tempBytes[0] = dataBuffer[4 * i + 2];
				tempBytes[1] = dataBuffer[4 * i + 3];
				short shortData1 = bytesToShort(tempBytes);
				mixPCMData[i] = (short)((shortData0 + shortData1) * 0.5f);
			}
		}
	}
	public static void generateMixPCMData(short[] mixPCMData, int mixDataCount, short channelCount, short[] dataBuffer, int bufferSize)
	{
		// 如果单声道,则直接将mDataBuffer的数据拷贝到mMixPCMData中
		if (channelCount == 1)
		{
			memcpy(mixPCMData, dataBuffer, 0, 0, getMin(bufferSize, mixDataCount) * sizeof(short));
		}
		// 如果有两个声道,则将左右两个声道的平均值赋值到mMixPCMData中
		else if (channelCount == 2)
		{
			for (int i = 0; i < mixDataCount; ++i)
			{
				mixPCMData[i] = (short)((dataBuffer[2 * i + 0] + dataBuffer[2 * i + 1]) * 0.5f);
			}
		}
	}
	public void refreshFileSize()
	{
		// 由于舍弃了fact块,所以需要重新计算文件大小,20是fmt块数据区的起始偏移,8是data块的头的大小
		mFileSize = 20 - 8 + mFmtChunkSize + 8 + mDataBuffer.Length;
	}
	public void startWaveStream(WaveFormatEx waveHeader)
	{
		CLASS(out mWaveSerializer);
		mRiffMark = bytesToInt((byte)'R', (byte)'I', (byte)'F', (byte)'F');
		mFileSize = 0;
		mWaveMark = bytesToInt((byte)'W', (byte)'A', (byte)'V', (byte)'E');
		mFmtMark = bytesToInt((byte)'f', (byte)'m', (byte)'t', (byte)' ');
		mFmtChunkSize = 16;
		mFormatType = waveHeader.wFormatTag;
		mSoundChannels = waveHeader.nChannels;
		mSamplesPerSec = waveHeader.nSamplesPerSec;
		mAvgBytesPerSec = waveHeader.nAvgBytesPerSec;
		mBlockAlign = waveHeader.nBlockAlign;
		mBitsPerSample = waveHeader.wBitsPerSample;
		mOtherSize = waveHeader.cbSize;
		mDataMark[0] = (byte)'d';
		mDataMark[1] = (byte)'a';
		mDataMark[2] = (byte)'t';
		mDataMark[3] = (byte)'a';
	}
	public void pushWaveStream(byte[] data, int dataSize)
	{
		mWaveSerializer.writeBuffer(data, dataSize);
	}
	public void endWaveStream()
	{
		mDataBuffer = new byte[mWaveSerializer.getDataSize()];
		memcpy(mDataBuffer, mWaveSerializer.getBuffer(), 0, 0, mDataBuffer.Length);
		UN_CLASS(ref mWaveSerializer);
		int mixDataCount = getMixPCMDataCount();
		mMixPCMData = new short[mixDataCount];
		generateMixPCMData(mMixPCMData, mixDataCount, mSoundChannels, mDataBuffer);
		refreshFileSize();
	}
}
#endif