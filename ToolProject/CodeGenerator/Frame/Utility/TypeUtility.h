#pragma once

#include "FrameDefine.h"
#include "IsPODType.h"
#include "IsSubClassOf.h"

// 跟类型,模板元编程相关的工具函数
class TypeUtility
{
	// T0是否与T1的类型一致,T0可以带const,&,volatile,在判断时会忽略这些修饰符
	template<typename T0, typename T1>
	static constexpr bool isType() { return is_same<typename decay<T0>::type, T1>(); }
	// 判断ChildClass是否是BaseClass的子类
	template<typename BaseClass, typename ChildClass>
	static constexpr bool isSubClass() { return IsSubClassOf<BaseClass, ChildClass>::mValue; }
	// 判断T是否为基础数据类型
	template<typename T>
	static constexpr bool isPODType() { return IsPod<T>::mValue; }
};