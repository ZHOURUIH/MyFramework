#pragma once

#include "FrameDefine.h"

// 判断是否为一个类的子类
template<typename BaseClass, typename ChildClass>
struct IsSubClassOf
{
	static constexpr bool test(BaseClass* type) { return true; }
	static constexpr bool test(const BaseClass* type) { return true; }
	static constexpr bool test(...) { return false; }
	static constexpr bool mValue = test((ChildClass*)nullptr);
	using mType = enable_if_t<mValue>;
};