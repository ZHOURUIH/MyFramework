#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/lzw-single-strip.tiff"
outfile="o-tiffcrop-R90-lzw-single-strip.tiff"
f_test_convert "$TIFFCROP -R90" $infile $outfile
f_tiffinfo_validate $outfile
