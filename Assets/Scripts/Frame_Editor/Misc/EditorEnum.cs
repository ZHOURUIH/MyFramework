using System;
using System.Collections.Generic;
using static FrameDefine;
using static FrameBaseDefine;

// 打包时资源文件备份的方式
public enum BACKUP_TARGET : byte
{
	NONE,               // 不备份
	BUILD_TEMP,         // 备份到BuildTemp
	INSTALL_TIME_TEMP,  // 备份到InstallTimeTemp
}