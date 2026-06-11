#pragma once

#include "FrameDefine.h"

class Undo
{
public:
	Undo() = default;
	virtual ~Undo() = default;
	virtual void undo() = 0;
protected:
};