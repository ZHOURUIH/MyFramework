#pragma once

#include "FrameHeader.h"

enum class OWNER : byte
{
	NONE,
	SERVER,
	CLIENT,
	BOTH,
};