using static FrameUtility;
using static BinaryUtility;

// 自定义缓冲区,用于持续写入数据
public class StreamBuffer : ClassObject
{
	protected byte[] mBuffer;		// 缓冲区
	protected int mBufferSize;		// 缓冲区大小
	protected int mDataLength;      // 当前数据大小
	public StreamBuffer(int bufferSize)
	{
		init(bufferSize);
	}
	public void init(int bufferSize)
	{
		resizeBuffer(bufferSize);
	}
	public override void destroy()
	{
		base.destroy();
		UN_ARRAY_BYTE_THREAD(ref mBuffer);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mBuffer = null;
		mBufferSize = 0;
		mDataLength = 0;
	}
	public byte[] getData(){ return mBuffer; }
	public int getDataLength(){ return mDataLength; }
	public int getBufferSize() { return mBufferSize; }
	public bool merge(StreamBuffer stream)
	{
		return addData(stream.getData(), stream.getDataLength());
	}
	public bool addData(byte[] data, int count)
	{
		// 缓冲区足够放下数据时才处理
		if (count > mBufferSize - mDataLength)
		{
			return false;
		}
		memcpy(mBuffer, data, mDataLength, 0, count);
		mDataLength += count;
		return true;
	}
	public bool addData(byte[] data, int offset, int count)
	{
		// 缓冲区足够放下数据时才处理
		if (count > mBufferSize - mDataLength)
		{
			return false;
		}
		memcpy(mBuffer, data, mDataLength, offset, count);
		mDataLength += count;
		return true;
	}
	public bool removeData(int start, int count)
	{
		if (start + count > mDataLength)
		{
			return false;
		}
		memmove(ref mBuffer, start, start + count, mDataLength - start - count);
		mDataLength -= count;
		memset(mBuffer, (byte)0, mDataLength, count);
		return true;
	}
	public void clear()
	{
		mDataLength = 0;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void resizeBuffer(int size)
	{
		if (mBufferSize >= size)
		{
			return;
		}
		mBufferSize = size;
		if (mBuffer != null)
		{
			// 创建新的缓冲区,将原来的数据拷贝到新缓冲区中,销毁原缓冲区,指向新缓冲区
			ARRAY_BYTE_THREAD(out byte[] newBuffer, mBufferSize);
			if (mDataLength > 0)
			{
				memcpy(newBuffer, mBuffer, 0, 0, mDataLength);
			}
			UN_ARRAY_BYTE_THREAD(ref mBuffer);
			mBuffer = newBuffer;
		}
		else
		{
			ARRAY_BYTE_THREAD(out mBuffer, mBufferSize);
		}
	}
}