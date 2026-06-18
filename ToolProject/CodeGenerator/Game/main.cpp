#include "Utility.h"
#include "CodeMySQL.h"
#include "CodeSQLite.h"
#include "CodeExcel.h"
#include "CodeNetPacket.h"
#include "CodeUnityBuild.h"
#include "CodeFrameSystem.h"
#include "CodeClassDeclareAndHeader.h"
#include "CodeComponent.h"
#include "CodeEnumCheck.h"
#include "CodeBaseCheck.h"
#include "CodeCondition.h"

void main()
{
	if (!CodeUtility::initPath())
	{
		cout << "配置文件解析失败" << endl;
		system("pause");
		return;
	}
	if (CodeUtility::isGenerateVirtualClient())
	{
		CodeNetPacket::generateVirtualClient();
	}
	else
	{
		llong time0 = timeGetTime();
		CodeNetPacket::generate();
		llong time1 = timeGetTime();
		LOG("CodeNetPacket耗时:" + StringUtility::LLToS(time1 - time0));
		CodeSQLite::generate();
		llong time2 = timeGetTime();
		LOG("CodeSQLite耗时:" + StringUtility::LLToS(time2 - time1));
		CodeExcel::generate();
		llong time3 = timeGetTime();
		LOG("CodeExcel耗时:" + StringUtility::LLToS(time3 - time2));
		CodeMySQL::generate();
		llong time4 = timeGetTime();
		LOG("CodeMySQL耗时:" + StringUtility::LLToS(time4 - time3));
		CodeCondition::generate();
		llong time5 = timeGetTime();
		LOG("CodeCondition耗时:" + StringUtility::LLToS(time5 - time4));
		CodeUnityBuild::generate();
		llong time6 = timeGetTime();
		LOG("CodeUnityBuild耗时:" + StringUtility::LLToS(time6 - time5));
		CodeFrameSystem::generate();
		llong time7 = timeGetTime();
		LOG("CodeFrameSystem耗时:" + StringUtility::LLToS(time7 - time6));
		CodeClassDeclareAndHeader::generate();
		llong time8 = timeGetTime();
		LOG("CodeClassDeclareAndHeader耗时:" + StringUtility::LLToS(time8 - time7));
		CodeComponent::generate();
		llong time9 = timeGetTime();
		LOG("CodeComponent耗时:" + StringUtility::LLToS(time9 - time8));
		CodeEnumCheck::generate();
		llong time10 = timeGetTime();
		LOG("CodeEnumCheck耗时:" + StringUtility::LLToS(time10 - time9));
		CodeBaseCheck::generate();
		llong time11 = timeGetTime();
		LOG("CodeBaseCheck耗时:" + StringUtility::LLToS(time11 - time10));
	}
	if (FrameDefine::mHasError)
	{
		system("pause");
	}
}