#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/quad-lzw-compat.tiff"
outfile="o-tiffcrop-extractz14-quad-lzw-compat.tiff"
f_test_convert "$TIFFCROP -E left -Z1:4,2:4" $infile $outfile
f_tiffinfo_validate $outfile
