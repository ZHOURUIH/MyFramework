#ifndef _TX_SHARE_MEMORY_SERVER_H_
#define _TX_SHARE_MEMORY_SERVER_H_

#ifdef WINDOWS
#include <aclapi.h>

#pragma warning(disable:4996)

#define ERROR_INVALID_CMDCODE 0xE00001FF  // 已經存在完全一樣的共享內存
#define ERROR_NO_MAPFILE 0xE00002FF  // 未分配共享內存文件
#define ERROR_INVALID_CFMCODE 0xE00003FF  // 校驗碼不匹配

enum MEMORY_STATE
{
	MS_NONE,
	MS_CLIENT_WRITE,
	MS_CLIENT_READ,
	MS_SERVER_WRITE,
	MS_SERVER_READ,
};

static const int mUserDataLength = 64;
#pragma pack(1)
struct DATA_HEADER
{
	int mCmd; // 命令码
	int mDataSize;	// 数据区长度
	char mFlag;		// 数据区状态,0表示无状态,1表示客户端数据已写入,2表示客户端已读取数据,3表示服务器已写入,4表示服务器已读取
	char mUserData[mUserDataLength];	// 64个字节的自定义数据
	DATA_HEADER()
	{
		mCmd = 0;
		mDataSize = 0;
		mFlag = MS_NONE;
		ZeroMemory(mUserData, mUserDataLength);
	}
};
typedef DWORD(WINAPI *PSetEntriesInAcl)(unsigned long, PEXPLICIT_ACCESS, PACL, PACL*);

#pragma pack()

//////////////////////////////////////////////////////////////////////
// 類定義，共享內存服務端
class txShareMemoryServer
{
public:
	txShareMemoryServer();
	virtual ~txShareMemoryServer();
	bool Create(char *szMapName, int dwSize); // 新建共享內存
	void* GetBuffer();      // 獲取內存文件指針
	int getMappingSize() { return mMappingSize; }
	void Destory();       // 銷毀已有的共享內存
	// 返回值为读取的数据长度,小于0表示读取错误,文件头需要填写命令码,数据长度
	int ReadCmdData(DATA_HEADER& header, void* pOutBuf);	
	// 需要填写文件头中的命令码,数据长度
	bool WriteCmdData(const DATA_HEADER& header, const void* pBuf);
protected:
	void _Init();    // 初始化參數
	bool _IsWinNTLater();  // 判斷當前操作系統
protected:
	PSetEntriesInAcl m_fnpSetEntriesInAcl;
	HANDLE m_hFile;   // 映射文件句柄
	HANDLE m_hFileMap;   // 內存文件句柄
	void* m_lpFileMapBuffer; // 緩衝區指針
	char *m_pFileName;  // 映射文件名
	char *m_pMapName;  // 內存文件名
	int mMappingSize;   // 緩衝區大小
	bool m_bCreateFlag;  // 是否已創建共享內存
	int m_dwLastError;  // 錯誤代碼
};
#endif
#endif
