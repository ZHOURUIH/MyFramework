#pragma once

#include "ImageDefine.h"
#include "FreeImage.h"
#include "FrameUtility.h"

class ImageUtility : public FrameUtility
{
public:
	static bool texturePackerAll(const string& texturePath, const string& atlasPath);
protected:
	static void packAtlas(const string& outputPath, const string& outputFileName, const string& sourcePath);
};