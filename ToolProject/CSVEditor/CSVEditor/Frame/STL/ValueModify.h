#pragma once

// 如果Value是非基础数据类型,则尽量设置为指针,否则在同步时可能无法获取正确的结果
template<typename T>
struct ValueModify
{
	T mValue;
	bool mAdd;
	ValueModify(const T& value, const bool add):
		mValue(value),
		mAdd(add)
	{}
};