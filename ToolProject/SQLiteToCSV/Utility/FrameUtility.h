#ifndef _FRAME_UTILITY_H_
#define _FRAME_UTILITY_H_

#include "SystemUtility.h"

class FrameUtility : public SystemUtility
{
public:
	template<typename T>
	static void setToVector(const Set<T>& set, Vector<T>& vec)
	{
		FOREACH_CONST(iter, set)
		{
			vec.push_back(*iter);
		}
	}
	template<typename T>
	static void vectorToSet(const Vector<T>& vec, Set<T>& set)
	{
		FOR_CONST(vec)
		{
			set.insert(vec[i]);
		}
	}
	static ushort generateCRC16(char* buffer, uint count);
};

#endif
