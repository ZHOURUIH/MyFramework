#include "Config.h"
#include "ImageUtility.h"

void main()
{
	// 各个工具函数的初始化需要优先调用
	MathUtility::initMathUtility();
	StringUtility::initStringUtility();
	Config::parse("./AtlasPackerConfig.txt");

	cout << "开始打包全部图集,打包成功后将会把散图删除..." << endl;
	string texturePath = "./";
	if (!ImageUtility::texturePackerAll(texturePath, Config::mAtlasPath))
	{
		system("pause");
	}
	else
	{
		// 将打包成功的图集散图删除
		myVector<string> folderList;
		FileUtility::findFolders(texturePath, folderList, false);
		FOR_VECTOR(folderList)
		{
			FileUtility::deleteFolder(folderList[i]);
		}
	}
	return;
}