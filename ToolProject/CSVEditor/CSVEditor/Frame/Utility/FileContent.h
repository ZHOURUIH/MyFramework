#pragma once

#include "FrameDefine.h"

class FileContent
{
public:
	virtual ~FileContent()
	{
		delete[] mBuffer;
	}
	void createBuffer(int bufferSize);
public:
	char* mBuffer = nullptr;
	int mFileSize = 0;
};