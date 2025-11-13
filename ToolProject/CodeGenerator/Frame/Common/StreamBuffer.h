#ifndef _STREAM_BUFFER_H_
#define _STREAM_BUFFER_H_

#include "FrameDefine.h"

class StreamBuffer
{
protected:
	uint mBufferSize;
	char* mBuffer;
	uint mDataStart;
	uint mDataLength;
public:
	StreamBuffer(uint bufferSize);
	virtual ~StreamBuffer();
	bool isAvailable(uint count);
	void addData(const char* data, uint count, bool clearIfFull = false);
	void removeData(uint count);
	void clear();
	uint getDataLength() { return mDataLength; }
	char* getData() { return mBuffer + mDataStart; }
	uint getDataStart() { return mDataStart; }
	uint getBufferSize() { return mBufferSize - mDataStart; }
protected:
	//-------------------------------------------------------------------------------------------------------------
	void resizeBuffer(uint size);
};

#endif