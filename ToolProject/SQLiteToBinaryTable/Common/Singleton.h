#ifndef _SINGLETON_H_
#define _SINGLETON_H_

#include "FrameDefine.h"

template <typename T> class Singleton
{
protected:
	static T* ms_Singleton;
public:
	Singleton(void)
	{
#if defined(_MSC_VER) && _MSC_VER < 1200
		int offset = (int)(T*)1 - (int)(Singleton<T>*)(T*)1;
		ms_Singleton = (T*)((int)this + offset);
#else
		if (ms_Singleton != nullptr)
		{
			// 此处不能使用ERROR
			return;
		}
		ms_Singleton = static_cast<T*>(this);
#endif
	}
	~Singleton(void)
	{
		ms_Singleton = 0;
	}
	static T& getSingleton(void)
	{
		return (*ms_Singleton);
	}
	static T* getSingletonPtr(void)
	{
		return ms_Singleton;
	}
};

#endif
