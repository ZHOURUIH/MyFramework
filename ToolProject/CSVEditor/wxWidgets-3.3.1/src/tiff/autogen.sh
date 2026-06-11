#!/bin/sh

retval=0

set -x

case `uname` in
  Darwin*)
    glibtoolize --force --copy || retval=$?
    ;;
  *)
    libtoolize --force --copy || retval=$?
    ;;
esac
aclocal -I ./m4 || retval=$?
autoheader || retval=$?
automake --foreign --add-missing --copy || retval=$?
autoconf || retval=$?
# Get latest config.guess and config.sub from upstream master since
# these are often out of date.
for file in config.guess config.sub
do
    echo "$0: getting $file..."
    wget -q --timeout=5 -O config/$file.tmp \
      "https://git.savannah.gnu.org/cgit/config.git/plain/${file}" \
      && mv -f config/$file.tmp config/$file \
      && chmod a+x config/$file || retval=$?
    rm -f config/$file.tmp
done

exit $retval
