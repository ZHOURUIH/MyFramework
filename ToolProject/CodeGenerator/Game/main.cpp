#include "Utility.h"
#include "CodeState.h"
#include "CodeMySQL.h"
#include "CodeSQLite.h"
#include "CodeNetPacket.h"
#include "CodeUnityBuild.h"
#include "CodeFrameSystem.h"
#include "CodeClassDeclareAndHeader.h"
#include "CodeComponent.h"
#include "CodeEnumCheck.h"
#include "CodeBaseCheck.h"

void main()
{
	if (!CodeUtility::initPath())
	{
		cout << "ÅäÖÃÎÄ¼þ½âÎöÊ§°Ü" << endl;
		system("pause");
		return;
	}
	if (CodeUtility::isGenerateVirtualClient())
	{
		CodeNetPacket::generateVirtualClient();
	}
	else
	{
		CodeNetPacket::generate();
		CodeSQLite::generate();
		CodeMySQL::generate();
		CodeState::generate();
		CodeUnityBuild::generate();
		CodeFrameSystem::generate();
		CodeClassDeclareAndHeader::generate();
		CodeComponent::generate();
		CodeEnumCheck::generate();
		CodeBaseCheck::generate();
	}
	if (FrameDefine::mHasError)
	{
		system("pause");
	}
}