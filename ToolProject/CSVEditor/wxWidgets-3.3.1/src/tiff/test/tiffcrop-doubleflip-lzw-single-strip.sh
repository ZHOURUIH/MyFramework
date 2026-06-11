#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/lzw-single-strip.tiff"
outfile="o-tiffcrop-doubleflip-lzw-single-strip.tiff"
f_test_convert "$TIFFCROP -F both" $infile $outfile
f_tiffinfo_validate $outfile
