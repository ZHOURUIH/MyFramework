// Scintilla source code edit control
/** @file PropSetSimple.h
 ** A basic string to string map.
 **/
// Copyright 1998-2009 by Neil Hodgson <neilh@scintilla.org>
// The License.txt file describes the conditions under which this software may be distributed.

#ifndef PROPSETSIMPLE_H
#define PROPSETSIMPLE_H

namespace Lexilla {

class PropSetSimple {
	void *impl;
public:
	PropSetSimple();
	// Deleted so PropSetSimple objects can not be copied.
	PropSetSimple(const PropSetSimple&) = delete;
	PropSetSimple(PropSetSimple&&) = delete;
	PropSetSimple &operator=(const PropSetSimple&) = delete;
	PropSetSimple &operator=(PropSetSimple&&) = delete;
	virtual ~PropSetSimple();

	bool Set(std::string const& key, std::string const& val);
	const char *Get(std::string const& key) const;
	int GetInt(std::string const& key, int defaultValue=0) const;
};

}

#endif
