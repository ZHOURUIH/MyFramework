#include "FrameHeader.h"

void FileContent::createBuffer(const int bufferSize)
{
	mFileSize = bufferSize;
	mBuffer = new char[mFileSize];
	if (mBuffer == nullptr)
	{
		ERROR("create file buffer failed!");
	}
}