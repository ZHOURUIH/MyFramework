#include "FrameHeader.h"

void FileContent::createBuffer(int bufferSize)
{
	mFileSize = bufferSize;
	mBuffer = NEW_ARRAY(char, mFileSize, mBuffer);
	if (mBuffer == nullptr)
	{
		ERROR("create file buffer failed!");
	}
}