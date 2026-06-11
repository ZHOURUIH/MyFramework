// Scintilla source code edit control
/** @file PropSetSimple.cxx
 ** A basic string to string map.
 **/
// Copyright 1998-2010 by Neil Hodgson <neilh@scintilla.org>
// The License.txt file describes the conditions under which this software may be distributed.

// Maintain a dictionary of properties

#include <cstdlib>
#include <cassert>
#include <cstring>

#include <string>
#include <map>
#include <functional>

#include "PropSetSimple.h"

using namespace Lexilla;

namespace {

using mapss = std::map<std::string, std::string, std::less<std::string>>;

mapss *PropsFromPointer(void *impl) noexcept {
	return static_cast<mapss *>(impl);
}

}

PropSetSimple::PropSetSimple() {
	mapss *props = new mapss;
	impl = static_cast<void *>(props);
}

PropSetSimple::~PropSetSimple() {
	mapss *props = PropsFromPointer(impl);
	delete props;
	impl = nullptr;
}

bool PropSetSimple::Set(std::string const& key, std::string const& val) {
	mapss *props = PropsFromPointer(impl);
	if (!props)
		return false;
	mapss::iterator const it = props->find(key);
	if (it != props->end()) {
		if (val == it->second)
			return false;
		it->second = val;
	} else {
		props->emplace(key, val);
	}
	return true;
}

const char *PropSetSimple::Get(std::string const& key) const {
	mapss *props = PropsFromPointer(impl);
	if (props) {
		mapss::const_iterator const keyPos = props->find(key);
		if (keyPos != props->end()) {
			return keyPos->second.c_str();
		}
	}
	return "";
}

int PropSetSimple::GetInt(std::string const& key, int defaultValue) const {
	const char *val = Get(key);
	assert(val);
	if (*val) {
		return atoi(val);
	}
	return defaultValue;
}
