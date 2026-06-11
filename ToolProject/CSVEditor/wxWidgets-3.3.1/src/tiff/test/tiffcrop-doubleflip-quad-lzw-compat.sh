#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/quad-lzw-compat.tiff"
outfile="o-tiffcrop-doubleflip-quad-lzw-compat.tiff"
f_test_convert "$TIFFCROP -F both" $infile $outfile
f_tiffinfo_validate $outfile
