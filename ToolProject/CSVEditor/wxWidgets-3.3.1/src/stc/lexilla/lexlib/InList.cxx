// Scintilla source code edit control
/** @file InList.cxx
 ** Check if a string is in a list.
 **/
// Copyright 2024 by Neil Hodgson <neilh@scintilla.org>
// The License.txt file describes the conditions under which this software may be distributed.

#include <cassert>

#include <string>
#include <initializer_list>

#include "InList.h"
#include "LexCharacterSet.h"

namespace Lexilla {

bool InList(std::string const& value, std::initializer_list<std::string> list) noexcept {
	for (std::string const& element : list) {
		if (value == element) {
			return true;
		}
	}
	return false;
}

bool InListCaseInsensitive(std::string const& value, std::initializer_list<std::string> list) noexcept {
	for (std::string const& element : list) {
		if (EqualCaseInsensitive(value, element)) {
			return true;
		}
	}
	return false;
}

}
