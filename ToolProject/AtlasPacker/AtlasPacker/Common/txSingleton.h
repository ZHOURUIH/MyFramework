 #ifndef _SINGLETON_H__
 #define _SINGLETON_H__

#include "ServerDefine.h"

template <typename T> class txSingleton
{
protected:
	static T* ms_Singleton;
public:
	txSingleton( void )
	{
#if defined( _MSC_VER ) && _MSC_VER < 1200
		int offset = (int)(T*)1 - (int)(Singleton <T>*)(T*)1;
		ms_Singleton = (T*)((int)this + offset);
#else
		if(ms_Singleton != NULL)
		{
			// 此处不能使用ENGINE_ERROR
			return;
		}
		ms_Singleton = static_cast< T* >( this );
#endif
	}
	~txSingleton( void )
	{
		ms_Singleton = 0;
	}
	static T& getSingleton( void )
	{
		return ( *ms_Singleton );
	}
	static T* getSingletonPtr( void )
	{
		return ms_Singleton;
	}
};

#endif
