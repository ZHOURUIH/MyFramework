#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/quad-lzw-compat.tiff"
outfile="o-tiff2rgba-quad-lzw-compat.tiff"
f_test_convert "$TIFF2RGBA" $infile $outfile
f_tiffinfo_validate $outfile
