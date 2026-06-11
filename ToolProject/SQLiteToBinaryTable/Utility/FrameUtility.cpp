#include "FrameHeader.h"

ushort FrameUtility::generateCRC16(char* buffer, uint count)
{
	return crc16(0x1F, buffer, count) ^ 0x123F;
}