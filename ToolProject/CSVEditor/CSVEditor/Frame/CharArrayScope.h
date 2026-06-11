#pragma once

#include "ArrayScope.h"

class CharArrayScope : public ArrayScope<char>
{
public:
	CharArrayScope(int length) :ArrayScope(length) {}
};