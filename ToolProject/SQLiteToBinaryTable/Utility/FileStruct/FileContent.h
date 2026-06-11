#ifndef _FILE_CONTENT_H_
#define _FILE_CONTENT_H_

#include "FrameBase.h"

class FileContent : public FrameBase
{
public:
	virtual ~FileContent()
	{
		DELETE_ARRAY(mBuffer);
	}
	void createBuffer(int bufferSize);
public:
	char* mBuffer = nullptr;
	uint mFileSize = 0;
};

#endif