// Scintilla source code edit control
/** @file InList.h
 ** Check if a string is in a list.
 **/
// Copyright 2024 by Neil Hodgson <neilh@scintilla.org>
// The License.txt file describes the conditions under which this software may be distributed.

#ifndef INLIST_H
#define INLIST_H

namespace Lexilla {

bool InList(std::string const& value, std::initializer_list<std::string> list) noexcept;
bool InListCaseInsensitive(std::string const& value, std::initializer_list<std::string> list) noexcept;

}

#endif
