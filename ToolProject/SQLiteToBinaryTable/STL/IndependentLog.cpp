#include "FrameHeader.h"

void IndependentLog::directError(const string& info)
{
	ERROR(info);
}

void IndependentLog::directLog(const string& info)
{
	LOG(info);
}