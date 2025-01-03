#ifndef _SERVER_BASE_H_
#define _SERVER_BASE_H_

#include "SystemUtility.h"

class ServerBase : public SystemUtility
{
public:
	static void notifyConstructDone();
	static void notifyComponentDeconstruct();
public:
};

#endif
