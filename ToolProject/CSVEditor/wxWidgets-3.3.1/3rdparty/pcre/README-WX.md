PCRE sources used by wxWidgets
==============================

This directory contains PCRE version used by wxWidgets.

It was created by cloning https://github.com/rurban/pcre which is, in turn, a
Git clone of the official PCRE2 subversion repository.

Updating PCRE
-------------

Fast-forward master to the new PCRE version, then merge it into wx branch
using `git merge --no-commit`. Before committing the merge, additionally:

1. Rerun autogen.sh.
1. Copy `src/pcre2_chartables.c.dist` to `src/pcre2_chartables.c`.
1. Examine changes to `src/pcre2.h.generic` and `src/config.h.generic` and
   propagate them to `src/wx/pcre2.h` and `src/wx/config.h` respectively.

Add all changed files to the commit.
