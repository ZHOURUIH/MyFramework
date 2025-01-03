#include "ServerBase.h"

void ServerBase::notifyConstructDone()
{
	// SQLite和MySQL的注册只能在此处写,因为在其他系统组件的初始化里面会用到这些
}

void ServerBase::notifyComponentDeconstruct()
{
	// 重新再获取一下所有组件
	notifyConstructDone();
}